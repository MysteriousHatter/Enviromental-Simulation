using System.Collections.Generic;
using UnityEngine;

public class WeedSideManager : MonoBehaviour
{
    [Header("Parent Object Containing Weeds")]
    public Transform weedParent;
    public List<Cuttable> weeds = new List<Cuttable>();
    [SerializeField] private string WeedObjectiveName;

    [Header("Flower Growth (Terrain Details)")]
    [SerializeField] private Terrain terrainOverride;       // optional
    [SerializeField] private int[] flowerDetailLayerIndices; // multiple detail layers
    [SerializeField, Min(1)] private int flowerInstancesPerCell = 8;
    [SerializeField] private bool growOnEachCut = true;      // per-weed patch
    [SerializeField, Min(0.25f)] private float flowerPatchSize = 4f; // meters (XZ square)
    [SerializeField] private bool useManagerBoxBounds = false; // use this manager's BoxCollider instead

    private int initialWeedCount;

    private void Start()
    {
        if (weedParent == null)
        {
            Debug.LogError("Weed Parent is not assigned!");
            return;
        }

        // Subscribe to each weed's OnCleared so we bloom per cut
        foreach (var w in weeds)
        {
            if (!w) continue;
            var weed = w;
            weed.OnCleared.AddListener(() =>
            {
                TryGrowFlowersForWeed(weed);
                CheckIfAllWeedsCleared(); // you already do counting elsewhere
            });
        }

        // Count the initial number of weeds under the parent
        initialWeedCount = CountWeeds();
        Debug.Log($"Initial Weed Count: {initialWeedCount}");
    }

    /// <summary>
    /// Counts the number of active weed GameObjects under the parent.
    /// </summary>
    /// <returns>The number of active weeds.</returns>
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


    public void RemoveWeed(Cuttable weed)
    {
        if (weeds.Contains(weed))
        {
            weeds.Remove(weed);
            Debug.Log($"Weed removed. Remaining weeds: {weeds.Count}");
        }
    }


    /// <summary>
    /// Checks if all weeds are cleared and logs a debug message if true.
    /// </summary>
    public void CheckIfAllWeedsCleared()
    {
        int remainingWeeds = CountWeeds();

        if (remainingWeeds == 0)
        {
            Debug.Log("All weeds are cleared! Update the score.");
            // You can add additional logic here, such as updating the score.
            GameManager.Instance.RegisterSideObjectiveCompleted(WeedObjectiveName);
        }
        else
        {
            Debug.Log($"Weeds remaining: {remainingWeeds}");
        }
    }
    private Terrain ResolveTerrainUnder(Vector3 worldPos)
    {
        if (terrainOverride) return terrainOverride;
        Terrain best = null;
        foreach (var t in Terrain.activeTerrains)
        {
            var td = t.terrainData; if (!td) continue;
            var pos = t.transform.position; var size = td.size;
            if (worldPos.x >= pos.x && worldPos.x <= pos.x + size.x &&
                worldPos.z >= pos.z && worldPos.z <= pos.z + size.z)
            { best = t; break; }
        }
        return best ? best : Terrain.activeTerrain;
    }

    private void TryGrowFlowersForWeed(Cuttable weed)
    {
        var t = ResolveTerrainUnder(weed.transform.position);
        if (!t || !t.terrainData) return;

        Bounds b;
        if (useManagerBoxBounds)
        {
            var box = GetComponent<BoxCollider>();
            if (!box) return;
            b = box.bounds;                      // whole-zone fill
        }
        else
        {
            // Localized patch around the cut weed
            var col = weed.GetComponent<Collider>();
            if (col) { b = col.bounds; }
            else { b = new Bounds(weed.transform.position, new Vector3(flowerPatchSize, 1f, flowerPatchSize)); }
            // Nudge to a square footprint
            b = new Bounds(b.center, new Vector3(flowerPatchSize, b.size.y, flowerPatchSize));
        }

        int randomLayer = flowerDetailLayerIndices[Random.Range(0, flowerDetailLayerIndices.Length)];
        GrowDetailsRect(t, b, randomLayer, flowerInstancesPerCell);

        // If you use a flush helper elsewhere, call it here (optional)
        // TerrainFlushScheduler.MarkDirty(t);
    }

    // ===== detail writer (rect fill, force-visible like your SeedGrowZone) =====
    static void GrowDetailsRect(Terrain t, Bounds worldBounds, int layer, int addPerCell)
    {
        var td = t.terrainData; if (!td) return;

        t.drawTreesAndFoliage = true;
        t.detailObjectDistance = Mathf.Max(t.detailObjectDistance, 100f);
        t.detailObjectDensity = Mathf.Max(t.detailObjectDensity, 0.8f);

        if (layer < 0 || layer >= td.detailPrototypes.Length) return;

        Vector3 tPos = t.transform.position, tSize = td.size;
        int dw = td.detailWidth, dh = td.detailHeight;

        int x0 = Mathf.FloorToInt(Mathf.Lerp(0, dw, Mathf.InverseLerp(tPos.x, tPos.x + tSize.x, worldBounds.min.x)));
        int x1 = Mathf.CeilToInt(Mathf.Lerp(0, dw, Mathf.InverseLerp(tPos.x, tPos.x + tSize.x, worldBounds.max.x)));
        int y0 = Mathf.FloorToInt(Mathf.Lerp(0, dh, Mathf.InverseLerp(tPos.z, tPos.z + tSize.z, worldBounds.min.z)));
        int y1 = Mathf.CeilToInt(Mathf.Lerp(0, dh, Mathf.InverseLerp(tPos.z, tPos.z + tSize.z, worldBounds.max.z)));

        x0 = Mathf.Clamp(x0, 0, dw - 1); x1 = Mathf.Clamp(x1, 0, dw);
        y0 = Mathf.Clamp(y0, 0, dh - 1); y1 = Mathf.Clamp(y1, 0, dh);
        int w = Mathf.Max(1, x1 - x0), h = Mathf.Max(1, y1 - y0);

        int[,] map = td.GetDetailLayer(x0, y0, w, h, layer);

        int target = Mathf.Max(addPerCell * 2, Mathf.Min(td.maxDetailScatterPerRes, 12));
        for (int yy = 0; yy < h; yy++)
            for (int xx = 0; xx < w; xx++)
                map[yy, xx] = target;

        td.SetDetailLayer(x0, y0, layer, map);
    }

    // ===== optional wet soil splat (same shape as details rect) =====
    static void PaintWetRect(Terrain t, Bounds worldBounds, int wetIndex, float strength01)
    {
        var td = t.terrainData; if (!td) return;
        if (wetIndex < 0 || wetIndex >= td.alphamapLayers) return;

        Vector3 tPos = t.transform.position, tSize = td.size;
        int aw = td.alphamapWidth, ah = td.alphamapHeight, layers = td.alphamapLayers;

        int x0 = Mathf.FloorToInt(Mathf.Lerp(0, aw, Mathf.InverseLerp(tPos.x, tPos.x + tSize.x, worldBounds.min.x)));
        int x1 = Mathf.CeilToInt(Mathf.Lerp(0, aw, Mathf.InverseLerp(tPos.x, tPos.x + tSize.x, worldBounds.max.x)));
        int y0 = Mathf.FloorToInt(Mathf.Lerp(0, ah, Mathf.InverseLerp(tPos.z, tPos.z + tSize.z, worldBounds.min.z)));
        int y1 = Mathf.CeilToInt(Mathf.Lerp(0, ah, Mathf.InverseLerp(tPos.z, tPos.z + tSize.z, worldBounds.max.z)));

        x0 = Mathf.Clamp(x0, 0, aw - 1); x1 = Mathf.Clamp(x1, 0, aw);
        y0 = Mathf.Clamp(y0, 0, ah - 1); y1 = Mathf.Clamp(y1, 0, ah);
        int w = Mathf.Max(1, x1 - x0), h = Mathf.Max(1, y1 - y0);

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
                    for (int l = 0; l < layers; l++)
                        alpha[y, x, l] /= sum;
            }

        td.SetAlphamaps(x0, y0, alpha);
    }


}
