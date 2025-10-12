using System;
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

    [Tooltip("How many seed impacts are needed before we grow the zone.")]
    public int seedsToComplete = 1;

    int _seedHits;


    [SerializeField] private bool isSideQuest;

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

    public int CountWeeds()
    {
        int count = 0;

        foreach (Cuttable child in weeds)
        {
            if (child.gameObject)
            {
                count++;
            }
        }

        return count;
    }

    public void BeginGrowthProcess()
    {
        if (isSideQuest)
        {
            _seedHits++;
            if (_seedHits >= seedsToComplete)
            {
                // Defer terrain edits until end of frame to avoid physics-phase issues
                this.gameObject.GetComponent<BoxCollider>().enabled = false;
                TurnOffDestoryOnClear();
            }
        }
    }

    private void TurnOffDestoryOnClear()
    {
        foreach (Cuttable child in weeds)
        {
            child.destroyOnClear = true;
        }

    }

    public void RemoveWeed(Cuttable weed)
    {
        if (weeds.Contains(weed))
        {
            weeds.Remove(weed);
            Debug.Log($"Weed removed. Remaining weeds: {weeds.Count}");
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
        Debug.Log("What's my current regrowth time " + t);

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
    public void CheckIfAllWeedsCleared()
    {
        int remainingWeeds = CountWeeds();

        Debug.Log("Check the remaing weeds: " +  remainingWeeds);

        if (remainingWeeds == 0)
        {
            Debug.Log("All weeds are cleared! Update the score.");
            // You can add additional logic here, such as updating the score.
            GameManager.Instance.RegisterSideObjectiveCompleted("Clearing Special Weed");
        }
        else
        {
            Debug.Log($"Weeds remaining: {remainingWeeds}");
        }
    }

}
