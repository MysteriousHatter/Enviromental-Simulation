using UnityEngine;
using System.Collections.Generic;

public class TreeManager: MonoBehaviour
{
    [SerializeField] private Terrain terrain; // Assign your terrain in the Inspector
    [SerializeField] private GameObject forestedTreePrefab; // Assign the forested tree prefab

    private bool goalReached = false;

    void Update()
    {
        if (!goalReached && GameManager.Instance.currentScore >= 6)
        {
            goalReached = true;
            ReplaceDeforestedTrees();
        }
    }

    private void ReplaceDeforestedTrees()
    {
        if (terrain == null || forestedTreePrefab == null)
        {
            Debug.LogError("Terrain or forestedTreePrefab is not assigned.");
            return;
        }

        TerrainData terrainData = terrain.terrainData;
        TreeInstance[] originalTrees = terrainData.treeInstances;
        List<TreeInstance> newTreeInstances = new List<TreeInstance>();

        // Iterate through all trees in the terrain
        foreach (TreeInstance tree in originalTrees)
        {
            Vector3 treeWorldPosition = Vector3.Scale(tree.position, terrainData.size) + terrain.transform.position;

            // Replace the deforested trees based on conditions (customize as needed)
            if (IsDeforestedTree(tree))
            {
                // Instantiate a forested tree prefab at the tree's position
                InstantiateForestedTree(treeWorldPosition);

                // Optionally, you can skip adding the replaced tree to the terrain
                continue;
            }

            // Keep the original tree if it's not deforested
            newTreeInstances.Add(tree);
        }

        // Update the terrain's tree instances with the remaining trees
        terrainData.treeInstances = newTreeInstances.ToArray();
    }

    private bool IsDeforestedTree(TreeInstance tree)
    {
        // Define your logic to identify deforested trees (e.g., prototype index or color)
        // For example, if deforested trees have a specific prototype index:
        int deforestedPrototypeIndex = 0; // Replace with your actual index
        return tree.prototypeIndex == deforestedPrototypeIndex;
    }

    private void InstantiateForestedTree(Vector3 position)
    {
        // Instantiate the forested tree prefab at the tree's position
        Instantiate(forestedTreePrefab, position, Quaternion.identity);
        Debug.Log($"Replaced tree at position {position} with forested tree.");
    }
}