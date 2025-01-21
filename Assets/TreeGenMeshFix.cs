using UnityEngine;


[ExecuteInEditMode]
public class TreeGenMeshFix : MonoBehaviour
{
    [SerializeField] private Mesh meshToAssign; // Drag the desired mesh here in the Inspector

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    void Awake()
    {
        // Ensure the GameObject has a MeshFilter and MeshRenderer
        meshFilter = gameObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        // Assign the mesh and material
        AssignMesh();
    }

    private void AssignMesh()
    {
        if (meshToAssign != null)
        {
            meshFilter.mesh = meshToAssign;
        }
        else
        {
            Debug.LogWarning("No mesh assigned to MeshAssigner.");
        }
    }
}
