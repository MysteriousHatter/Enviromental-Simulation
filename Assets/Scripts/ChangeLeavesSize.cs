using UnityEngine;
using TreeEditor;
using System;
using System.Collections;
using Unity.VisualScripting;
using TMPro;
using Treegen;

public class ChangeLeavesSize : MonoBehaviour
{
    public TreegenTreeGenerator treeGenerator; // Reference to the TreegenTreeGenerator component
    public Vector3 targetLeafScale = new Vector3(2f, 2f, 2f); // Desired maximum scale for the leaves
    public float growthDuration = 5f; // Duration for scaling the leaves (in seconds)

    private Vector3 initialLeafScale; // Original scale of the leaves
    private float elapsedTime = 0f; // Timer for scaling

    void Start()
    {
        if (treeGenerator == null)
        {
            Debug.LogError("TreegenTreeGenerator component is not assigned.");
            return;
        }

        // Store the initial leaf scale
        initialLeafScale = treeGenerator.LeavesScale;
    }

    void Update()
    {
        if (elapsedTime < growthDuration)
        {
            // Update the timer
            elapsedTime += Time.deltaTime;

            // Calculate the new scale using Lerp
            treeGenerator.LeavesScale = Vector3.Lerp(initialLeafScale, targetLeafScale, elapsedTime / growthDuration);

            // Regenerate the tree to apply the new leaf scale
            treeGenerator.NewGenLeaves();
        }
    }
}
