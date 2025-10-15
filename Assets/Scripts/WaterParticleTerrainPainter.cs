using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class WaterParticleZonePainter : MonoBehaviour
{
    public LayerMask zoneLayers;           // set to the layer your WaterGrowZone boxes live on
    public int maxEventsPerFrame = 64;     // throttle

    private float timeSinceLastCollision = 0f;
    [SerializeField] private float noCollisionTimeout = 1f; // seconds until we consider it "no collisions"
    ZoneHealthBar zoneHealthBar; 



    ParticleSystem _ps;

    // per-zone accounting
    readonly Dictionary<WaterGrowZoneConfig, int> _zoneHits = new();
    readonly HashSet<WaterGrowZoneConfig> _completed = new();

    static readonly List<ParticleCollisionEvent> _events = new(128);

    void Awake()
    {
        _ps = GetComponent<ParticleSystem>();
        // In the ParticleSystem -> Collision module, enable "Send Collision Messages".
        // (OnParticleCollision will then be invoked.) :contentReference[oaicite:2]{index=2}
        zoneHealthBar = FindObjectOfType<DialogBoxController>().healthUI;
        // Adjust the detail prototype's height range
        AdjustDetailPrototypeHeight();
    }

    private void Update()
    {
        // If no collisions have occurred recently
        timeSinceLastCollision += Time.deltaTime;
        if (timeSinceLastCollision > noCollisionTimeout)
        {
            OnNoCollisions();
            timeSinceLastCollision = 0f; // optional reset
        }
    }

    private void OnNoCollisions()
    {
        // Put your “idle” logic here:
        // e.g. stop watering UI, fade particle effect, etc.

        zoneHealthBar.gameObject.SetActive(false);
        Debug.Log("No collisions detected for a while.");
    }

    void OnParticleCollision(GameObject other)
    {
        if (((1 << other.layer) & zoneLayers.value) == 0)
        {
            return;
        }

        var cfg = other.GetComponent<WaterGrowZoneConfig>();
        if (!cfg || _completed.Contains(cfg)) return;

        _events.Clear();
        int count = ParticlePhysicsExtensions.GetCollisionEvents(_ps, other, _events);
        if (count <= 0) return;

        int added = Mathf.Min(count, maxEventsPerFrame);

        // existing local accounting (optional to keep)
        if (!_zoneHits.ContainsKey(cfg)) _zoneHits[cfg] = 0;
        _zoneHits[cfg] = Mathf.Min(cfg.hitsToComplete, _zoneHits[cfg] + added);
        cfg.AddHits(added);

        zoneHealthBar.gameObject.SetActive(true);
        zoneHealthBar.SetProgress01(cfg.Progress01, immediate: true);

        timeSinceLastCollision = 0f;

        if (_zoneHits[cfg] >= cfg.hitsToComplete)
        {

            StartCoroutine(DeferGrowZone(cfg));
            _completed.Add(cfg);
            _zoneHits[cfg] = cfg.hitsToComplete;

            if(cfg.growZone != null)
            {
                if (cfg.growZone.needsWater) { GameManager.Instance?.RegisterSideObjectiveCompleted("Normal Gardening"); }
            }
            else { GameManager.Instance?.RegisterSideObjectiveCompleted(cfg.objectiveId); }
            if (cfg.oneShot) StartCoroutine(DisableColliderAfterFrame(other.GetComponent<Collider>()));
            cfg.zone.gameObject.SetActive(true);
        }
    }

    private IEnumerator DeferGrowZone(WaterGrowZoneConfig cfg)
    {
        yield return null; // Wait until the end of the frame
        TryGrowZone(cfg); // Safely modify the terrain here
    }


    private void AdjustDetailPrototypeHeight()
    {
        var terrain = Terrain.activeTerrain;
        if (!terrain || !terrain.terrainData) return;

        var terrainData = terrain.terrainData;
        var detailPrototypes = terrainData.detailPrototypes;

        int detailLayerIndex = 3; // Replace with the correct detail layer index for your flowers
        if (detailLayerIndex < 0 || detailLayerIndex >= detailPrototypes.Length)
        {
            Debug.LogWarning("Invalid detail layer index for adjusting height.");
            return;
        }

        // Adjust the height range for the detail prototype
        detailPrototypes[detailLayerIndex].minHeight = 5.0f; // Set minimum height
        detailPrototypes[detailLayerIndex].maxHeight = 10.0f; // Set maximum height

        // Apply the updated detail prototypes back to the terrain
        terrainData.detailPrototypes = detailPrototypes;

        Debug.Log($"Adjusted height range for detail layer {detailLayerIndex}: MinHeight = 2.0, MaxHeight = 3.0");
    }
    private IEnumerator DisableColliderAfterFrame(Collider collider)
    {
        yield return null; // Wait until the end of the frame
        if (collider != null)
        {
            collider.enabled = false;
        }
    }


    void TryGrowZone(WaterGrowZoneConfig cfg)
    {
        Debug.Log("Grow Zone");
        var terrain = Terrain.activeTerrain;
        if (!terrain || !terrain.terrainData) return;

        var col = cfg.GetComponent<BoxCollider>();
        if (!col) return;

        // world-space AABB for the zone (BoxCollider respects object scale) :contentReference[oaicite:4]{index=4}
        Bounds b = col.bounds;

        // 1) grow flowers across bounds (detail layer)
        GrowRosesInBounds(terrain, b, cfg.roseDetailLayerIndex, cfg.roseInstancesPerCell);

        // 2) optional wet soil pass (alphamaps)
        if (cfg.alsoPaintWetSoil)
            PaintWetRect(terrain, b, cfg.wetTerrainLayerIndex, cfg.paintStrength);

        TerrainFlushScheduler.MarkDirty(terrain); // apply runtime changes so they render immediately :contentReference[oaicite:5]{index=5}
    }

    // ===== Detail-layer growth (rect fill) =====
    static void GrowRosesInBounds(Terrain t, Bounds worldBounds, int roseLayer, int addPerCell)
    {
        var td = t.terrainData;
        Debug.Log("The length of our detail layer " +  td.detailPrototypes.Length + "and our current layer " + roseLayer);
        if (roseLayer < 0 || roseLayer >= td.detailPrototypes.Length) return;

        Vector3 tPos = t.transform.position;
        Vector3 tSize = td.size;

        int dw = td.detailWidth;
        int dh = td.detailHeight;


        // world -> detail indices
        int x0 = Mathf.FloorToInt(Mathf.Lerp(0, dw, Mathf.InverseLerp(tPos.x, tPos.x + tSize.x, worldBounds.min.x)));
        int x1 = Mathf.CeilToInt(Mathf.Lerp(0, dw, Mathf.InverseLerp(tPos.x, tPos.x + tSize.x, worldBounds.max.x)));
        int y0 = Mathf.FloorToInt(Mathf.Lerp(0, dh, Mathf.InverseLerp(tPos.z, tPos.z + tSize.z, worldBounds.min.z)));
        int y1 = Mathf.CeilToInt(Mathf.Lerp(0, dh, Mathf.InverseLerp(tPos.z, tPos.z + tSize.z, worldBounds.max.z)));

        x0 = Mathf.Clamp(x0, 0, dw - 1); x1 = Mathf.Clamp(x1, 0, dw);
        y0 = Mathf.Clamp(y0, 0, dh - 1); y1 = Mathf.Clamp(y1, 0, dh);

        int w = Mathf.Max(1, x1 - x0);
        int h = Mathf.Max(1, y1 - y0);

        int[,] detail = td.GetDetailLayer(x0, y0, w, h, roseLayer); // :contentReference[oaicite:6]{index=6}

        int add = Mathf.Max(0, addPerCell);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                // Note: value meaning depends on DetailScatterMode (Coverage vs InstanceCount). :contentReference[oaicite:7]{index=7}
                int v = detail[y, x] + add;
                detail[y, x] = Mathf.Clamp(v, 0, 255);
            }

        td.SetDetailLayer(x0, y0, roseLayer, detail); // writes subregion back :contentReference[oaicite:8]{index=8}
    }

    // ===== Optional alpha paint (rect fill toward wet layer) =====
    static void PaintWetRect(Terrain t, Bounds worldBounds, int wetIndex, float strength01)
    {
        var td = t.terrainData;
        if (wetIndex < 0 || wetIndex >= td.alphamapLayers) return;

        Vector3 tPos = t.transform.position;
        Vector3 tSize = td.size;

        int aw = td.alphamapWidth;
        int ah = td.alphamapHeight;
        int layers = td.alphamapLayers;

        int x0 = Mathf.FloorToInt(Mathf.Lerp(0, aw, Mathf.InverseLerp(tPos.x, tPos.x + tSize.x, worldBounds.min.x)));
        int x1 = Mathf.CeilToInt(Mathf.Lerp(0, aw, Mathf.InverseLerp(tPos.x, tPos.x + tSize.x, worldBounds.max.x)));
        int y0 = Mathf.FloorToInt(Mathf.Lerp(0, ah, Mathf.InverseLerp(tPos.z, tPos.z + tSize.z, worldBounds.min.z)));
        int y1 = Mathf.CeilToInt(Mathf.Lerp(0, ah, Mathf.InverseLerp(tPos.z, tPos.z + tSize.z, worldBounds.max.z)));

        x0 = Mathf.Clamp(x0, 0, aw - 1); x1 = Mathf.Clamp(x1, 0, aw);
        y0 = Mathf.Clamp(y0, 0, ah - 1); y1 = Mathf.Clamp(y1, 0, ah);

        int w = Mathf.Max(1, x1 - x0);
        int h = Mathf.Max(1, y1 - y0);

        float[,,] alpha = td.GetAlphamaps(x0, y0, w, h); // z = layer index (splat) :contentReference[oaicite:9]{index=9}
        float s = Mathf.Clamp01(strength01);

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float sum = 0f;
                for (int l = 0; l < layers; l++)
                {
                    float v = alpha[y, x, l];
                    if (l == wetIndex) v = Mathf.Clamp01(v + s * (1f - v));
                    else v = Mathf.Clamp01(v * (1f - s));
                    alpha[y, x, l] = v;
                    sum += v;
                }
                if (sum > 0f)
                {
                    for (int l = 0; l < layers; l++) alpha[y, x, l] /= sum; // normalize
                }
            }

        td.SetAlphamaps(x0, y0, alpha); // write back :contentReference[oaicite:10]{index=10}
    }
}
