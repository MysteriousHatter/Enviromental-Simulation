using UnityEngine;

[ExecuteAlways]
public class RuntimeTerrainClone : MonoBehaviour
{
    public Terrain terrain;

    private TerrainData _original;
    private TerrainData _runtime;

    void Awake()
    {
        if (!terrain) terrain = GetComponent<Terrain>();
#if UNITY_EDITOR
        if (terrain && Application.isPlaying)
        {
            _original = terrain.terrainData;
            _runtime = Instantiate(_original);
            _runtime.name = _original.name + " (Runtime)";
            // Prevent saving the clone in the editor or builds
            _runtime.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;

            terrain.terrainData = _runtime;

            var tc = terrain.GetComponent<TerrainCollider>();
            if (tc) tc.terrainData = _runtime;
        }
#endif
    }

    void OnDestroy()
    {
#if UNITY_EDITOR
        if (terrain && _original && Application.isPlaying)
        {
            // Restore the original asset link (clone will be GC’d)
            terrain.terrainData = _original;
            var tc = terrain.GetComponent<TerrainCollider>();
            if (tc) tc.terrainData = _original;
        }
#endif
    }
}
