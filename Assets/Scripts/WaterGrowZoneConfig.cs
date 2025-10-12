using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class WaterGrowZoneConfig : MonoBehaviour
{
    [Header("Side Objective")]
    public string objectiveId = "Zone_Pond_01";
    [Range(0f, 1f)] public float award01 = 0.05f;
    public int hitsToComplete = 120;
    public bool oneShot = true;
    public SeedGrowZone growZone;

    [Header("Terrain (optional override)")]
    public Terrain terrainOverride; // ← this is what your error was about

    [Header("Detail Growth")]
    public int roseDetailLayerIndex = 0;
    public int roseInstancesPerCell = 4;

    [Header("Optional: paint wet soil")]
    public bool alsoPaintWetSoil = false;
    public int wetTerrainLayerIndex = 1;
    [Range(0f, 1f)] public float paintStrength = 0.75f;

    void Reset()
    {
        var col = GetComponent<BoxCollider>();
        col.isTrigger = false; // OnParticleCollision requires non-trigger
    }

}
