using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SeedGrowZone : MonoBehaviour
{
    [Header("Completion")]
    [Tooltip("How many seed impacts are needed before we grow the zone.")]
    public int seedsToComplete = 1;
    public bool oneShot = true;

    [Header("Side Objective Award")]
    public string objectiveId = "Zone_Seed_01";
    [Range(0f, 1f)] public float award01 = 0.03f;
    public bool needsWater = false;
    public GameObject WaterArea;

    [Header("Terrain Targets")]
    [Tooltip("If null, the script will resolve which Terrain tile is under this zone at runtime.")]
    public Terrain terrainOverride;
    [Tooltip("Detail prototype index for your flower/rose in Terrain → Paint Details (0-based).")]
    public int roseDetailLayerIndex = 0;
    [Tooltip("How much to add per affected detail texel.")]
    public int roseInstancesPerCell = 6;

    [Header("Optional: paint wet soil")]
    public bool alsoPaintWetSoil = false;
    public int wetTerrainLayerIndex = 1;
    [Range(0f, 1f)] public float paintStrength = 0.6f;

    int _seedHits;
    bool _completed;

    void Awake()
    {
        Debug.Log($"[SeedGrowZone] Initialized with roseDetailLayerIndex: {roseDetailLayerIndex}, wetTerrainLayerIndex: {wetTerrainLayerIndex}");
        AdjustDetailPrototypeHeight();
        if (WaterArea != null) { WaterArea.SetActive(false); }
    }


    void Reset()
    {
        var col = GetComponent<BoxCollider>();
        col.isTrigger = false; // we want OnCollisionEnter, not triggers
    }

    public void BeginGrowthProcess()
    {
        _seedHits++;
        if (_seedHits >= seedsToComplete)
        {
            _completed = true;
            // Defer terrain edits until end of frame to avoid physics-phase issues
            if (!needsWater)
            {
                StartCoroutine(DeferGrowZone());
            }
            else
            {
                if (WaterArea != null) { WaterArea.SetActive(true); }
            }
            if (oneShot) StartCoroutine(DisableColliderAfterFrame(this.gameObject.GetComponent<Collider>()));
        }
    }

    private IEnumerator DisableColliderAfterFrame(Collider collider)
    {
        yield return null; // Wait until the end of the frame
        if (collider != null)
        {
            collider.enabled = false;
        }
    }


    private IEnumerator DeferGrowZone()
    {
        yield return null; // end of frame
        TryGrowZone();
        // If you already use a central scheduler that batches Flush() calls, replace this with: TerrainFlushScheduler.MarkDirty(theTerrain)
        var t = ResolveTerrainUnderZone();
        if (t) t.Flush();

        if (!string.IsNullOrEmpty(objectiveId) && award01 > 0f)
            GameManager.Instance?.RegisterSideObjectiveCompleted(objectiveId, award01);
    }

    // ---------------- Terrain ops (same behavior as your reference) ----------------

    void TryGrowZone()
    {
        var terrain = Terrain.activeTerrain;
        if (!terrain || !terrain.terrainData) return;

        var col = GetComponent<BoxCollider>();
        if (!col) return;

        Bounds b = col.bounds;

        GrowRosesInBounds(terrain, b, roseDetailLayerIndex, roseInstancesPerCell);

        if (alsoPaintWetSoil)
            PaintWetRect(terrain, b, wetTerrainLayerIndex, paintStrength);

        TerrainFlushScheduler.MarkDirty(terrain); // apply runtime changes so they render immediately :contentReference[oaicite:5]{index=5}
    }

    Terrain ResolveTerrainUnderZone()
    {
        if (terrainOverride) return terrainOverride;

        var col = GetComponent<Collider>();
        if (!col) return Terrain.activeTerrain;

        Vector3 center = col.bounds.center;
        // Find the terrain tile whose XZ bounds contain the zone center
        foreach (var t in Terrain.activeTerrains)
        {
            var td = t.terrainData;
            if (!td) continue;
            Vector3 pos = t.transform.position;
            Vector3 size = td.size;
            if (center.x >= pos.x && center.x <= pos.x + size.x &&
                center.z >= pos.z && center.z <= pos.z + size.z)
                return t;
        }
        return Terrain.activeTerrain; // fallback
    }

    static void GrowRosesInBounds(Terrain t, Bounds worldBounds, int roseLayer, int addPerCell)
    {
        var td = t.terrainData;
        if (!td) { Debug.LogWarning("[SeedGrowZone] No TerrainData"); return; }

        // 0) Sanity render settings (one-time safe bump)
        t.drawTreesAndFoliage = true;
        t.detailObjectDistance = Mathf.Max(t.detailObjectDistance, 100f);
        t.detailObjectDensity = Mathf.Max(t.detailObjectDensity, 0.8f);

        int layers = td.detailPrototypes.Length;
        Debug.Log($"[SeedGrowZone] layer={roseLayer}/{layers - 1} scatterMode={td.detailScatterMode} maxPerRes={td.maxDetailScatterPerRes}");
        if (roseLayer < 0 || roseLayer >= layers)
        {
            Debug.LogWarning($"[SeedGrowZone] Detail layer {roseLayer} out of range."); return;
        }

        Vector3 tPos = t.transform.position, tSize = td.size;
        int dw = td.detailWidth, dh = td.detailHeight;

        int x0 = Mathf.FloorToInt(Mathf.Lerp(0, dw, Mathf.InverseLerp(tPos.x, tPos.x + tSize.x, worldBounds.min.x)));
        int x1 = Mathf.CeilToInt(Mathf.Lerp(0, dw, Mathf.InverseLerp(tPos.x, tPos.x + tSize.x, worldBounds.max.x)));
        int y0 = Mathf.FloorToInt(Mathf.Lerp(0, dh, Mathf.InverseLerp(tPos.z, tPos.z + tSize.z, worldBounds.min.z)));
        int y1 = Mathf.CeilToInt(Mathf.Lerp(0, dh, Mathf.InverseLerp(tPos.z, tPos.z + tSize.z, worldBounds.max.z)));

        x0 = Mathf.Clamp(x0, 0, dw - 1); x1 = Mathf.Clamp(x1, 0, dw);
        y0 = Mathf.Clamp(y0, 0, dh - 1); y1 = Mathf.Clamp(y1, 0, dh);
        int w = Mathf.Max(1, x1 - x0), h = Mathf.Max(1, y1 - y0);

        Debug.Log($"[SeedGrowZone] detailRect x0:{x0} x1:{x1} y0:{y0} y1:{y1} size:{w}x{h} (dw:{dw} dh:{dh})");

        int[,] map = td.GetDetailLayer(x0, y0, w, h, roseLayer);

        // 1) TELEMETRY: how much is there now?
        long sumBefore = 0; int minBefore = int.MaxValue, maxBefore = int.MinValue;
        for (int yy = 0; yy < h; yy++)
            for (int xx = 0; xx < w; xx++)
            {
                int v = map[yy, xx];
                sumBefore += v;
                if (v < minBefore) minBefore = v;
                if (v > maxBefore) maxBefore = v;
            }

        // 2) FORCE-VISIBLE WRITE (not +=) to remove “too-small to notice” issues
        int target = Mathf.Max(addPerCell * 2, Mathf.Min(td.maxDetailScatterPerRes, 12));
        for (int yy = 0; yy < h; yy++)
            for (int xx = 0; xx < w; xx++)
                map[yy, xx] = target;

        td.SetDetailLayer(x0, y0, roseLayer, map);

        // 3) TELEMETRY after write
        long sumAfter = 0; int minAfter = int.MaxValue, maxAfter = int.MinValue, changed = 0;
        for (int yy = 0; yy < h; yy++)
            for (int xx = 0; xx < w; xx++)
            {
                int v = map[yy, xx];
                sumAfter += v;
                if (v < minAfter) minAfter = v;
                if (v > maxAfter) maxAfter = v;
                if (v != 0) changed++; // rough indicator
            }

        Debug.Log($"[SeedGrowZone] wrote layer {roseLayer} -> changed:{changed}/{w * h} " +
                  $"before[min:{minBefore} max:{maxBefore} sum:{sumBefore}] " +
                  $"after[min:{minAfter} max:{maxAfter} sum:{sumAfter}]");
    }
    // ===== Optional alpha paint (rect fill toward wet layer) =====
    static void PaintWetRect(Terrain t, Bounds worldBounds, int wetIndex, float strength01)
    {
        var td = t.terrainData;
        if (!td || wetIndex < 0 || wetIndex >= td.alphamapLayers) return;

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

        float[,,] alpha = td.GetAlphamaps(x0, y0, w, h);
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
                    alpha[y, x, l] = v; sum += v;
                }
                if (sum > 0f)
                {
                    for (int l = 0; l < layers; l++)
                        alpha[y, x, l] /= sum;
                }
            }

        td.SetAlphamaps(x0, y0, alpha);
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
}
