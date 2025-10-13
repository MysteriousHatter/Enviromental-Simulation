using System.Collections.Generic;
using UnityEngine;

public class WaterManager : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private List<Transform> waterTransforms = new List<Transform>(); // Multiple water bodies

    [Header("Heights")]
    [SerializeField] private float initialWaterHeight = 0f; // Initial Y position for all waters
    [SerializeField] private float maxWaterHeight = 10f;    // Max Y position the waters can reach

    [Header("Progress (staged)")]
    [SerializeField] private int totalStages = 10;   // Total number of water level stages
    [SerializeField] private float progressStep = 0.1f; // Increment per progress call (10% default)

    private int currentStage = 0;        // Current water level stage
    private float heightPerStage;        // Height increment per stage
    private float currentProgress = 0f;  // Normalized 0..1

    private void Start()
    {
        if (waterTransforms == null || waterTransforms.Count == 0)
        {
            Debug.LogError("[WaterManager] No water transforms assigned.");
            return;
        }

        if (totalStages <= 0)
        {
            Debug.LogWarning("[WaterManager] totalStages must be > 0. Forcing to 1.");
            totalStages = 1;
        }

        heightPerStage = (maxWaterHeight - initialWaterHeight) / totalStages;

        // Initialize all waters to the initial height
        UpdateWaterLevel(initialWaterHeight);
    }

    /// <summary>
    /// Increment progress by progressStep and advance a stage when crossed.
    /// </summary>
    public void SetProgress()
    {
        // Increment and clamp progress
        currentProgress = Mathf.Clamp01(currentProgress + progressStep);

        // Determine the new stage from progress
        int newStage = Mathf.FloorToInt(currentProgress * totalStages);

        // Only update when we move to a higher stage
        if (newStage > currentStage)
        {
            currentStage = newStage;
            float newHeight = initialWaterHeight + (heightPerStage * currentStage);
            Debug.Log($"[WaterManager] Incremented Water Height to: {newHeight:F3} (Stage {currentStage}/{totalStages})");
            UpdateWaterLevel(newHeight);
        }
        else
        {
            Debug.Log("[WaterManager] No stage change. Current progress is within the current stage.");
        }
    }

    /// <summary>
    /// Directly set normalized progress (0..1) and update levels accordingly.
    /// </summary>
    public void SetProgress01(float progress01)
    {
        float clamped = Mathf.Clamp01(progress01);
        currentProgress = clamped;

        int newStage = Mathf.FloorToInt(currentProgress * totalStages);
        if (newStage != currentStage)
        {
            currentStage = newStage;
            float newHeight = initialWaterHeight + (heightPerStage * currentStage);
            Debug.Log($"[WaterManager] SetProgress01 ? Height {newHeight:F3} (Stage {currentStage}/{totalStages})");
            UpdateWaterLevel(newHeight);
        }
    }

    /// <summary>
    /// Reset back to the initial height and zero progress.
    /// </summary>
    public void ResetProgress()
    {
        currentProgress = 0f;
        currentStage = 0;
        UpdateWaterLevel(initialWaterHeight);
    }

    private void UpdateWaterLevel(float newY)
    {
        if (waterTransforms == null) return;

        for (int i = 0; i < waterTransforms.Count; i++)
        {
            Transform t = waterTransforms[i];
            if (!t) continue;

            Vector3 p = t.position;
            t.position = new Vector3(p.x, newY, p.z);
        }
    }
}
