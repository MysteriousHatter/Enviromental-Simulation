using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class WeedManager : MonoBehaviour
{
    [Header("Weed Management")]
    public List<Cuttable> weeds = new List<Cuttable>();
    public Transform player;
    [Min(0f)] public float proximityThreshold = 3f;
    [Min(0f)] public float regrowthTime = 5f;
    [SerializeField] private Color materialChange = Color.red;
    [SerializeField] private string WeedObjectiveName;

    [Header("Flower Growth (Terrain Details)")]
    [SerializeField] private Terrain terrainOverride;       // optional
    [SerializeField] private int[] flowerDetailLayerIndices; // multiple detail layers
    [SerializeField, Min(1)] private int flowerInstancesPerCell = 8;
    [SerializeField] private bool growOnEachCut = true;      // per-weed patch
    [SerializeField, Min(0.25f)] private float flowerPatchSize = 4f; // meters (XZ square)
    [SerializeField] private bool useManagerBoxBounds = false; // use this manager's BoxCollider instead
   

    [Tooltip("How many seed impacts are needed before we grow the zone.")]
    public int seedsToComplete = 1;

    int _seedHits;
    private bool weedIsDead = false;

    [SerializeField] private bool isSideQuest;

    // If true, the player must be away for the *entire* regrowth time (timer resets when near).
    public bool requirePlayerFarForEntireDelay = false;

    private readonly Dictionary<Cuttable, Coroutine> _pending = new();

    // ===================== UI HOOKUP (like SeedGrowZone) =====================
    [Header("UI (optional)")]
    private ZoneHealthBar zoneHealthBar;      // assign in Inspector or auto-find
    [SerializeField] private bool findHealthBarOnStart = true; // find DialogBoxController.healthUI
    [SerializeField] private bool showUIOnSeedHit = true;
    [SerializeField] private bool immediateOnFirstShow = true; // first tick can snap
    [SerializeField] private float uiIdleSeconds = 60f;        // hide after no hits for N seconds
    private bool _shownOnce;
    private float _lastSeedTime;
    private Coroutine _uiIdleCo;
    // ========================================================================

    private void Awake()
    {

        zoneHealthBar = FindFirstObjectByType<DialogBoxController>().healthUI;
        zoneHealthBar.gameObject.SetActive(false);
        // Wire up listeners so we start the timer when a weed is actually cleared.
        foreach (var w in weeds)
        {
            if (!w) continue;
            // capture w in a local for the closure
            var weed = w;
            weed.OnCleared.AddListener(() =>
            {
                if (growOnEachCut && weedIsDead) TryGrowFlowersForWeed(weed);
                ScheduleRegrow(weed);
            });
        }
    }

    private void Start()
    {
        // Initialize UI to 0% (it’ll be hidden by your ZoneHealthBar’s fade/active logic)
        UpdateHealthUI(forceShow: false, immediate: true);
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
            UpdateHealthUI(forceShow: showUIOnSeedHit, immediate: !_shownOnce && immediateOnFirstShow);
            if (_seedHits >= seedsToComplete)
            {
                // Defer terrain edits until end of frame to avoid physics-phase issues
                ChangeWeedMaterial();
                weedIsDead = true;
                this.gameObject.GetComponent<BoxCollider>().enabled = false;
                TurnOffDestoryOnClear();
            }
        }
    }

    private void ChangeWeedMaterial()
    {
        foreach (Cuttable child in weeds)
        {
            child.gameObject.GetComponent<MaterialModifierChildren>().ChangeMaterialSmooth(materialChange);
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
        Debug.Log("Cut Weed Before");
        if (!weed) return;

        Debug.Log("Cut Weed After");

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

        Debug.Log("Check the remaing weeds: " + remainingWeeds);

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

    private void UpdateHealthUI(bool forceShow, bool immediate)
    {
        if (!zoneHealthBar) return;

        float p = seedsToComplete <= 0 ? 1f : Mathf.Clamp01((float)_seedHits / seedsToComplete);
        zoneHealthBar.SetProgress01(p, immediate);   // public API from earlier bar
        if (forceShow) zoneHealthBar.KeepAlive();    // extends its own visible timer
        _shownOnce = true;
    }

    List<Cuttable> toRemove = new List<Cuttable>();
    public void ClearAllWeeds()
    {
        for (int i = weeds.Count - 1; i >= 0; i--)
        {
            var weed = weeds[i];
            if (weed == null)
                continue;

            weed.destroyOnClear = true;
            // Trigger the clear logic in Cuttable
            // (assumes your Cuttable has a method like Clear() or OnCleared.Invoke())
            weed.Clear();

            // Optionally destroy the GameObject if that’s how you want it handled
            Destroy(weed.gameObject);
            toRemove.Add(weed);

            Debug.Log($"Weed {weed.name} destroyed and removed. Remaining weeds: {weeds.Count}");
        }

        foreach (var weed in toRemove)
        {
            weeds.Remove(weed);
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