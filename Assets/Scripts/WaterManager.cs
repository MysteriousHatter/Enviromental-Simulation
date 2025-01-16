using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterManager : MonoBehaviour
{
    [SerializeField] private Transform waterTransform; // The transform of the water object
    [SerializeField] private float initialWaterHeight = 0f; // Initial Y position of the water
    [SerializeField] private float maxWaterHeight = 10f; // Maximum Y position the water can reach
    [SerializeField] private float transitionSpeed = 0.1f; // Speed of the water level transition

    private float currentProgress = 0f; // Current interpolation progress (0 to 1)
    private float targetProgress = 0f; // Target progress to reach (0 to 1)

    void Start()
    {
        if (waterTransform == null)
        {
            Debug.LogError("Water Transform is not assigned.");
            return;
        }

        // Initialize the water height
        waterTransform.position = new Vector3(
            waterTransform.position.x,
            initialWaterHeight,
            waterTransform.position.z
        );
    }

    void Update()
    {
        // Gradually move the current progress toward the target progress
        currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, transitionSpeed * Time.deltaTime);

        // Update the water level based on the current progress
        UpdateWaterLevel(currentProgress);
    }

    public void SetProgress(float progress)
    {
        // Update the target progress (e.g., called by GameManager)
        targetProgress = Mathf.Clamp01(progress);
    }

    private void UpdateWaterLevel(float progress)
    {
        // Calculate the new Y position based on progress
        float newY = Mathf.Lerp(initialWaterHeight, maxWaterHeight, progress);

        // Apply the new Y position to the water
        waterTransform.position = new Vector3(
            waterTransform.position.x,
            newY,
            waterTransform.position.z
        );
    }
}

