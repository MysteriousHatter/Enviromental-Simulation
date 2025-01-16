using System.Collections.Generic;
using UnityEngine;
using Treegen;

public class TreeManager : MonoBehaviour
{
    [SerializeField] private List<TreegenTreeGenerator> _treeList = new List<TreegenTreeGenerator>();
    [SerializeField] private float growthDuration = 10f; // Duration for full growth
    [SerializeField] private Vector3 maxLeafScale = new Vector3(2f, 2f, 2f); // Maximum leaf scale

    private Dictionary<TreegenTreeGenerator, float> _treeGrowthProgress = new Dictionary<TreegenTreeGenerator, float>();

    void Start()
    {
        // Retrieve all root GameObjects in the scene
        GameObject[] rootGameObjects = gameObject.scene.GetRootGameObjects();

        // Iterate through each root GameObject
        foreach (GameObject root in rootGameObjects)
        {
            // Recursively check each GameObject and its children
            CheckForTreeComponent(root);
        }

        // Initialize growth progress for each tree
        foreach (TreegenTreeGenerator tree in _treeList)
        {
            _treeGrowthProgress[tree] = 0f; // Start with no growth
        }
    }

    void CheckForTreeComponent(GameObject obj)
    {
        // Check if the GameObject has a TreegenTreeGenerator component
        if (obj.TryGetComponent<TreegenTreeGenerator>(out TreegenTreeGenerator treeComponent))
        {
            _treeList.Add(treeComponent);
            Debug.Log($"Found GameObject with TreegenTreeGenerator component: {obj.name}");
        }

        // Recursively check children
        foreach (Transform child in obj.transform)
        {
            CheckForTreeComponent(child.gameObject);
        }
    }

    public void SetProgress(float progress)
    {
        foreach (TreegenTreeGenerator tree in _treeList)
        {
            // Clamp progress between 0 and 1
            float clampedProgress = Mathf.Clamp01(progress);

            // Update growth progress for the tree
            _treeGrowthProgress[tree] = clampedProgress;

            // Calculate the current leaf scale
            Vector3 currentLeafScale = Vector3.Lerp(Vector3.zero, maxLeafScale, clampedProgress);

            // Update the LeavesScale property of the tree
            tree.LeavesScale = currentLeafScale;

            // Ensure the trunk and branches are regenerated
            tree.NewGen(); // Use a hypothetical method that regenerates the entire tree (trunk, branches, and leaves)

            Debug.Log($"Manually set leaf scale to: {currentLeafScale} for progress: {clampedProgress}");
        }
    }

    void Update()
    {
        foreach (TreegenTreeGenerator tree in _treeList)
        {
            // Increment growth progress
            if (_treeGrowthProgress[tree] < 1f)
            {
                _treeGrowthProgress[tree] += Time.deltaTime / growthDuration;
                Vector3 currentLeafScale = Vector3.Lerp(Vector3.zero, maxLeafScale, _treeGrowthProgress[tree]);

                // Update the LeavesScale property of the tree
                tree.LeavesScale = currentLeafScale;

                // Ensure the trunk and branches are regenerated
                tree.NewGen(); // Use a hypothetical method that regenerates the entire tree (trunk, branches, and leaves)

                Debug.Log($"Updated leaf scale to: {currentLeafScale}");
            }
        }
    }
}
