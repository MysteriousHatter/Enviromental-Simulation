using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// Put this on a GameObject with a BoxCollider (can be rotated).
/// Use this if your foliage was painted with *Paint Trees* (including shrubs/bushes).
[RequireComponent(typeof(BoxCollider))]
public class TerrainTreeZone : MonoBehaviour
{
    [Header("Terrain")]
    public Terrain terrain;

    [Header("Which tree prototypes to affect")]
    [Tooltip("Only these prototype indices will be removed/regrown. Leave empty to affect ALL prototypes.")]
    public int[] prototypeIndices; // e.g., set to just your bush prototype index

    [Header("Behavior")]
    public bool clearOnStart = true;   // start with this zone barren?
    public float growDuration = 2.5f;  // seconds to regrow (0 = instant)

    [Tooltip("Optional ID so UI can address this zone (e.g., 'EastGarden').")]
    public string zoneId = "ZoneA";

    // cache & state
    TerrainData td;
    Vector3 tPos, tSize;

    /// The exact instances we removed from this zone (so we can restore them later).
    readonly List<TreeInstance> removed = new();

    void Awake()
    {
        if (!terrain) terrain = Terrain.activeTerrain;
        td = terrain.terrainData;
        tPos = terrain.transform.position;
        tSize = td.size;
    }

    void Start()
    {
        for (int i = 0; i < Terrain.activeTerrain.terrainData.treePrototypes.Length; i++)
            Debug.Log($"{i}: {Terrain.activeTerrain.terrainData.treePrototypes[i].prefab.name}");
        if (clearOnStart) ClearNow();
    }

    // ---------------- Public API you call from UI / code ----------------

    public void ClearNow()
    {
        RemoveTreesInsideCollider();
    }

    public void GrowNow()
    {
        if (growDuration <= 0.0001f) { RestoreAllRemovedInstant(); return; }
        StopAllCoroutines();
        StartCoroutine(RegrowOverTime(growDuration));
    }

    // ---------------- Core logic ----------------

    void RemoveTreesInsideCollider()
    {
        removed.Clear();

        // Collect survivors & removed in one pass.
        var current = td.treeInstances;
        var survivors = new List<TreeInstance>(current.Length);

        bool affectAll = (prototypeIndices == null || prototypeIndices.Length == 0);
        HashSet<int> affectSet = affectAll ? null : new HashSet<int>(prototypeIndices);

        // Build an AABB from the collider corners (supports rotation)
        var aabb = GetColliderWorldAABB();

        for (int i = 0; i < current.Length; i++)
        {
            var inst = current[i];

            // Convert normalized tree position to world XZ
            Vector3 worldPos = new Vector3(
                tPos.x + inst.position.x * tSize.x,
                0f,
                tPos.z + inst.position.z * tSize.z
            );

            bool inside = aabb.Contains(new Vector3(worldPos.x, 0f, worldPos.z));

            Debug.Log("The current survivors " + inst.prototypeIndex);
            bool isTargetProto = affectAll || affectSet.Contains(inst.prototypeIndex);

            if (inside && isTargetProto)
            {
                Debug.Log("How many trees left " + i);
                removed.Add(inst); // remember exactly what we took out
            }
            else
            {
                survivors.Add(inst);
            }
        }

        td.treeInstances = survivors.ToArray();
        terrain.Flush();
    }

    IEnumerator RegrowOverTime(float seconds)
    {
        if (removed.Count == 0) yield break;

        // We'll append in batches over time for a sprouting effect.
        var survivors = new List<TreeInstance>(td.treeInstances);
        int total = removed.Count;
        int added = 0;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, seconds);
            int targetCount = Mathf.RoundToInt(Mathf.Lerp(0, total, t));
            while (added < targetCount && added < total)
            {
                survivors.Add(removed[added]);
                added++;
            }
            td.treeInstances = survivors.ToArray();
            terrain.Flush();
            yield return null;
        }

        // Ensure all are back
        while (added < total) { survivors.Add(removed[added++]); }
        td.treeInstances = survivors.ToArray();
        terrain.Flush();
        removed.Clear();
    }

    void RestoreAllRemovedInstant()
    {
        Debug.Log("Restore in an instant " + removed.Count);
        if (removed.Count == 0) return;
        var survivors = new List<TreeInstance>(td.treeInstances);
        survivors.AddRange(removed);
        td.treeInstances = survivors.ToArray();
        terrain.Flush();
        removed.Clear();
    }

    // ---------------- Utilities ----------------

    /// Returns an axis-aligned world-space Bounds on XZ that encloses the (possibly rotated) BoxCollider.
    Bounds GetColliderWorldAABB()
    {
        var bc = GetComponent<BoxCollider>();
        Vector3 c = transform.TransformPoint(bc.center);
        Vector3 half = 0.5f * Vector3.Scale(bc.size, transform.lossyScale);

        // Get the 4 XZ corner points in world space
        Vector3[] local = new Vector3[]
        {
            new(+half.x, 0, +half.z),
            new(+half.x, 0, -half.z),
            new(-half.x, 0, +half.z),
            new(-half.x, 0, -half.z),
        };

        float minX = float.PositiveInfinity, maxX = float.NegativeInfinity;
        float minZ = float.PositiveInfinity, maxZ = float.NegativeInfinity;

        for (int i = 0; i < local.Length; i++)
        {
            Vector3 w = transform.TransformPoint(bc.center + local[i]);
            if (w.x < minX) minX = w.x; if (w.x > maxX) maxX = w.x;
            if (w.z < minZ) minZ = w.z; if (w.z > maxZ) maxZ = w.z;
        }

        Vector3 center = new Vector3((minX + maxX) * 0.5f, 0f, (minZ + maxZ) * 0.5f);
        Vector3 size = new Vector3(Mathf.Max(0f, maxX - minX), 0f, Mathf.Max(0f, maxZ - minZ));
        return new Bounds(center, size);
    }

    // Helper: find a prototype index by prefab name (optional)
    public int FindPrototypeIndexByName(string containsName)
    {
        var protos = td.treePrototypes;
        for (int i = 0; i < protos.Length; i++)
        {
            var prefab = protos[i].prefab;
            if (prefab && prefab.name.ToLowerInvariant().Contains(containsName.ToLowerInvariant()))
                return i;
        }
        return -1;
    }
}