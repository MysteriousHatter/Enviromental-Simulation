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
    [SerializeField] private GameObject partilceVFX1;
    [SerializeField] private GameObject partilceVFX2;
    [SerializeField] private GameObject partilceVFX3;
    private GameObject disableOnClear => this.gameObject;
    public UnityEvent OnCleared;


    private void Start()
    {
    }
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
        OnCleared?.Invoke();

        PlayParticles();
        // Notify the manager only if destroyOnClear is true
        if (destroyOnClear)
        {
            WeedSideManager weedSideManager = GetComponentInParent<WeedSideManager>();
            WeedManager weedManager = GetComponentInParent<WeedManager>();

            if (weedSideManager != null)
            {
                weedSideManager.RemoveWeed(this);
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
        PlayParticles();

    }

    private void PlayParticles()
    {
        var vfx1 = Instantiate(partilceVFX1, transform.position, transform.rotation);
        var ps1 = vfx1.GetComponent<ParticleSystem>();

        var vfx2 = Instantiate(partilceVFX2, transform.position, transform.rotation);
        var ps2 = vfx2.GetComponent<ParticleSystem>();

        var vfx3 = Instantiate(partilceVFX3, transform.position, transform.rotation);
        var ps3 = vfx3.GetComponent<ParticleSystem>();
        if (ps1 && ps2 && ps3)
        {
            ps1.Play();
            ps2.Play();
            ps3.Play();

        }
    }
}

