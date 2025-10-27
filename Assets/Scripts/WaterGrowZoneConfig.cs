using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class WaterGrowZoneConfig : MonoBehaviour
{
    [Header("Side Objective")]
    public string objectiveId = "Zone_Pond_01";
    [Range(0f, 1f)] public float award01 = 0.05f;
    public int hitsToComplete = 120;
    public bool oneShot = true;
    public FlowerReloadZone zone;
    public SeedGrowZone growZone;

    [Header("Terrain (optional override)")]
    public Terrain terrainOverride; // ← this is what your error was about

    [Header("Detail Growth")]
    public int[] flowerDetailLayerIndices; // multiple detail layers
    public int roseInstancesPerCell = 4;

    [Header("Optional: paint wet soil")]
    public bool alsoPaintWetSoil = false;
    public int wetTerrainLayerIndex = 1;
    [Range(0f, 1f)] public float paintStrength = 0.75f;

    public int CurrentHits { get; private set; }
    public float Progress01 => hitsToComplete <= 0 ? 1f : Mathf.Clamp01((float)CurrentHits / hitsToComplete);
    public event Action<WaterGrowZoneConfig, int, int> OnProgress; // (zone, hits, max)

    public void AddHits(int add)
    {
        if (add <= 0) return;
        int before = CurrentHits;
        CurrentHits = Mathf.Min(hitsToComplete, CurrentHits + add);
        if (CurrentHits != before)
            OnProgress?.Invoke(this, CurrentHits, hitsToComplete);
    }

    void Reset()
    {
        var col = GetComponent<BoxCollider>();
        col.isTrigger = false; // OnParticleCollision requires non-trigger
    }

}
