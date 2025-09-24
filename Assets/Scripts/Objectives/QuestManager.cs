using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [Header("Authoring")]
    [Tooltip("Drag all ObjectiveControllers in this Area here (or leave empty to auto-scan the scene).")]
    public List<ObjectiveController> controllers = new List<ObjectiveController>();

    [Tooltip("Percent needed to mark Area cleared (e.g., 80).")]
    [Range(0, 100)] public float clearThreshold = 80f;

    [Header("Runtime (read-only)")]
    [SerializeField, Range(0, 100)] float restorationPercent;

    // quick lookups
    readonly Dictionary<string, ObjectiveController> byId = new();

    void Awake()
    {
        // find controllers if not assigned
        if (controllers == null || controllers.Count == 0)
            controllers = FindObjectsOfType<ObjectiveController>(includeInactive: true).ToList();

        // map by ObjectiveDefinition.id
        foreach (var c in controllers)
        {
            if (c == null || c.def == null || string.IsNullOrEmpty(c.def.id)) continue;
            if (!byId.ContainsKey(c.def.id))
                byId.Add(c.def.id, c);
        }

        // subscribe to lifecycle events from your hub
        ObjectiveEvents.OnCompleted += HandleCompleted;

        // initial availability pass
        RecomputeAvailability();
        RaiseRestorationChanged();
    }

    void OnDestroy()
    {
        ObjectiveEvents.OnCompleted -= HandleCompleted;
    }

    // ————— Core: availability & prerequisites —————

    void RecomputeAvailability()
    {
        foreach (var c in controllers)
        {
            if (c == null || c.def == null) continue;
            if (c.State != ObjectiveState.Inactive) continue;

            if (AllPrereqsCompleted(c.def))
            {
                // Moves state -> Available and fires ObjectiveEvents.RaiseAvailable(def)
                c.MakeAvailable();  // defined in your ObjectiveController
            }
        }
    }

    bool AllPrereqsCompleted(ObjectiveDefinition def)
    {
        if (def.prerequisites == null || def.prerequisites.Length == 0) return true;

        foreach (var pre in def.prerequisites)
        {
            if (pre == null || string.IsNullOrEmpty(pre.id)) return false;
            if (!byId.TryGetValue(pre.id, out var preCtrl)) return false;
            if (preCtrl.State != ObjectiveState.Completed) return false;
        }
        return true;
    }

    // ————— Completion / restoration —————

    void HandleCompleted(ObjectiveDefinition def)
    {
        if (def == null) return;

        restorationPercent = Mathf.Clamp(restorationPercent + def.restorationPercent, 0f, 100f);
        RaiseRestorationChanged();

        // After any completion, new objectives might become Available.
        RecomputeAvailability();

        // Area clear check
        if (restorationPercent >= clearThreshold)
        {
            Debug.Log($"Area Clear! Restoration {restorationPercent:0}% / {clearThreshold}%");
            // TODO: raise a UI event, unlock exit, etc.
        }
    }

    void RaiseRestorationChanged()
    {
        // If you add an event for meter changes later, call it here.
        // For now we just log; wire this to your HUD ProgressBar.
        Debug.Log($"Restoration: {restorationPercent:0}%");
    }

    // ————— Optional helpers —————

    public float GetRestorationPercent() => restorationPercent;

    public ObjectiveController GetController(string objectiveId)
        => byId.TryGetValue(objectiveId, out var c) ? c : null;

    // Start an Available objective (e.g., from a trigger, dialog, or button).
    public bool TryBegin(string objectiveId)
    {
        var c = GetController(objectiveId);
        if (c == null) return false;
        if (c.State != ObjectiveState.Available) return false;
        c.Begin();   // calls ObjectiveEvents.RaiseStarted(def)
        return true;
    }
}
