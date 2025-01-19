using System.Collections.Generic;
using UnityEngine;
using Treegen;

public class TreeManager : MonoBehaviour
{
    [SerializeField] private List<TreegenTreeGenerator> _treeList = new List<TreegenTreeGenerator>();
    [SerializeField] private Vector3 initialLeafScale = new Vector3(0.5f, 0.5f, 0.5f); // Initial leaf scale
    [SerializeField] private Vector3 maxLeafScale = new Vector3(2f, 2f, 2f); // Maximum leaf scale

    private Dictionary<TreegenTreeGenerator, bool> _treeFullyGrown = new Dictionary<TreegenTreeGenerator, bool>();
    private bool goalReached = false;

    void Start()
    {
        InitializeTrees();
    }

    private void InitializeTrees()
    {
        // Retrieve all root GameObjects in the scene and find trees
        GameObject[] rootGameObjects = gameObject.scene.GetRootGameObjects();

        foreach (GameObject root in rootGameObjects)
        {
            FindTreeComponents(root);
        }

        // Mark all trees as not fully grown and set the initial scale
        foreach (TreegenTreeGenerator tree in _treeList)
        {
            _treeFullyGrown[tree] = false;
            tree.LeavesScale = initialLeafScale; // Set initial scale for the tree
        }
    }

    private void FindTreeComponents(GameObject obj)
    {
        if (obj.TryGetComponent<TreegenTreeGenerator>(out TreegenTreeGenerator treeComponent))
        {
            _treeList.Add(treeComponent);
        }

        foreach (Transform child in obj.transform)
        {
            FindTreeComponents(child.gameObject);
        }
    }

    public void SetProgress(float progress)
    {
        if (!goalReached)
        {
            // Check if the goal score is reached in the GameManager
            if (GameManager.Instance.currentScore >= GameManager.Instance.goalScore)
            {
                goalReached = true;
                FullyGrowAllTrees();
            }
            else
            {
                IncrementallyGrowTrees(progress);
            }
        }
    }

    private void IncrementallyGrowTrees(float progress)
    {
        foreach (TreegenTreeGenerator tree in _treeList)
        {
            if (!_treeFullyGrown[tree])
            {
                // Scale between initialLeafScale and maxLeafScale based on progress
                Vector3 currentLeafScale = Vector3.Lerp(initialLeafScale, maxLeafScale, progress);
                tree.LeavesScale = currentLeafScale;
            }
        }
    }

    private void FullyGrowAllTrees()
    {
        foreach (TreegenTreeGenerator tree in _treeList)
        {
            tree.LeavesScale = maxLeafScale; // Set to maximum scale
            tree.NewGen(); // Fully regenerate the tree (leaves, branches, etc.)
            _treeFullyGrown[tree] = true;
            Debug.Log($"Tree {tree.name} is now fully grown.");
        }
    }
}
