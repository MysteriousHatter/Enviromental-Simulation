using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WeedManager : MonoBehaviour
{
    [Header("Weed Management")]
    public List<Cuttable> weeds = new List<Cuttable>();
    public Transform player;
    [Min(0f)] public float proximityThreshold = 3f;
    [Min(0f)] public float regrowthTime = 5f;

    // If true, the player must be away for the *entire* regrowth time (timer resets when near).
    public bool requirePlayerFarForEntireDelay = false;

    private readonly Dictionary<Cuttable, Coroutine> _pending = new();

    private void Awake()
    {
        // Wire up listeners so we start the timer when a weed is actually cleared.
        foreach (var w in weeds)
        {
            if (!w) continue;
            // capture w in a local for the closure
            var weed = w;
            weed.OnCleared.AddListener(() => ScheduleRegrow(weed));
        }
    }

    private void OnDisable()
    {
        // Clean up timers when manager disables
        foreach (var kv in _pending) if (kv.Value != null) StopCoroutine(kv.Value);
        _pending.Clear();
    }

    // Called when a weed is cleared
    private void ScheduleRegrow(Cuttable weed)
    {
        if (!weed) return;

        // stop an existing timer for this weed (if any) and start a fresh one
        if (_pending.TryGetValue(weed, out var c) && c != null)
            StopCoroutine(c);

        _pending[weed] = StartCoroutine(RegrowRoutine(weed));
    }

    private IEnumerator RegrowRoutine(Cuttable weed)
    {
        float t = 0f;

        while (t < regrowthTime)
        {
            if (!weed) yield break; // destroyed? (destroyOnClear=true) -> abort

            bool near = IsPlayerNear(weed.transform.position); // world-space
            if (requirePlayerFarForEntireDelay)
            {
                // Require continuous time away
                if (near) t = 0f; else t += Time.deltaTime;
            }
            else
            {
                // Only count up while far
                if (!near) t += Time.deltaTime;
            }

            yield return null;
        }

        // Only try to reactivate if the weed still exists and is actually in an active hierarchy
        if (!weed.gameObject.activeInHierarchy)
        {
            weed.Reactivate();
        }

        _pending.Remove(weed);
    }

    private bool IsPlayerNear(Vector3 weedWorldPos)
    {
        if (!player) return false;
        float distance = Vector3.Distance(player.position, weedWorldPos);
        return distance <= proximityThreshold;
    }
}
