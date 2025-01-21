using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterManager : MonoBehaviour
{
    [SerializeField] private Transform waterTransform; // The transform of the water object
    [SerializeField] private float initialWaterHeight = 0f; // Initial Y position of the water
    [SerializeField] private float maxWaterHeight = 10f; // Maximum Y position the water can reach
    [SerializeField] private int totalStages = 10; // Total number of water level stages
    [SerializeField] private float progressStep = 0.1f; // Increment for each progress call (default 10% per call)

    private int currentStage = 0; // Current water level stage
    private float heightPerStage; // Height increment per stage
    private float currentProgress = 0f; // Current progress (normalized 0 to 1)

    void Start()
    {
        if (waterTransform == null)
        {
            Debug.LogError("Water Transform is not assigned.");
            return;
        }

        // Calculate height increment per stage
        heightPerStage = (maxWaterHeight - initialWaterHeight) / totalStages;

        // Initialize water height
        UpdateWaterLevel(initialWaterHeight);
    }

    public void SetProgress()
    {
        // Increment the progress by the predefined step
        currentProgress += progressStep;

        // Clamp progress to ensure it doesn't exceed 1.0 (100%)
        currentProgress = Mathf.Clamp01(currentProgress);

        // Calculate the new stage based on progress
        int newStage = Mathf.FloorToInt(currentProgress * totalStages);

        // Only update if we've moved to the next stage
        if (newStage > currentStage)
        {
            currentStage = newStage;
            float newHeight = initialWaterHeight + (heightPerStage * currentStage);
            Debug.Log($"Incremented Water Height to: {newHeight}");
            UpdateWaterLevel(newHeight);
        }
        else
        {
            Debug.Log("No stage change. Current progress is within the current stage.");
        }
    }

    private void UpdateWaterLevel(float newY)
    {
        // Update the Y position of the water transform
        waterTransform.position = new Vector3(
            waterTransform.position.x,
            newY,
            waterTransform.position.z
        );
    }
}

