using System.Collections.Generic;
using UnityEngine;

public class TerrainFlushScheduler : MonoBehaviour
{
    static TerrainFlushScheduler _inst;
    readonly HashSet<Terrain> _dirty = new HashSet<Terrain>();

    void Awake()
    {
        if (_inst != null) { Destroy(gameObject); return; }
        _inst = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void MarkDirty(Terrain t)
    {
        if (!t) return;
        if (_inst == null)
        {
            var go = new GameObject("[Terrain Flush Scheduler]");
            _inst = go.AddComponent<TerrainFlushScheduler>();
        }
        _inst._dirty.Add(t);
    }

    // Run outside physics; LateUpdate is a good spot.
    void LateUpdate()
    {
        if (_dirty.Count == 0) return;
        foreach (var t in _dirty)
        {
            Debug.Log("Are we flushing");
            t.Flush();
        }
        _dirty.Clear();
    }
}

