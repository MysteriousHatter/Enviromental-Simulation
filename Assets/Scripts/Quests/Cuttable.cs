using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class Cuttable : MonoBehaviour
{
    [Header("Cut Health & Yield")]
    public float hitPoints = 1f;
    public float biomassYield = 1f;

    [Header("Clear Behavior")]
    public bool destroyOnClear = true;
    private GameObject disableOnClear => this.gameObject;
    public UnityEvent OnCleared;

    /// <summary>Returns true if cleared; out param gives biomass awarded this strike (on clear).</summary>
    public bool ApplyCut(float power, out float yield)
    {
        yield = 0f;

        yield = Mathf.Max(0f, biomassYield);
        Clear();
        return true;
    }

    private void Clear()
    {
        // Notify the manager only if destroyOnClear is true
        if (destroyOnClear)
        {
            WeedSideManager weedSideManager = GetComponentInParent<WeedSideManager>();
            WeedManager weedManager = GetComponentInParent<WeedManager>();

            if (weedSideManager != null)
            {
                //weedSideManager.RemoveWeed(this);
                weedSideManager.CheckIfAllWeedsCleared();
            }
            else if (weedManager != null)
            {
                weedManager.RemoveWeed(this);
                weedManager.CheckIfAllWeedsCleared();
            }

            // Destroy the game object
            Destroy(gameObject);
        }
        else
        {
            if (disableOnClear) disableOnClear.SetActive(false);
            foreach (var c in GetComponentsInChildren<Collider>(true)) c.enabled = false;
            foreach (var r in GetComponentsInChildren<Renderer>(true)) r.enabled = false;

        }
    }

    public void Reactivate()
    {
        // If this GO was deactivated, turn it back on first.
        if (!gameObject.activeSelf)
            gameObject.SetActive(true); // local active flag

        // Now (re)enable visuals & colliders.
        if (disableOnClear) disableOnClear.SetActive(true);
        foreach (var c in GetComponentsInChildren<Collider>(true)) c.enabled = true;
        foreach (var r in GetComponentsInChildren<Renderer>(true)) r.enabled = true;

    }

}

