using UnityEngine;

using UnityEngine;
using System.Collections;

using UnityEngine;
using System.Collections;

public class GrassGrowthManager : MonoBehaviour
{
    [SerializeField] private Terrain terrain;
    [SerializeField] private Color dryGrassColor = new Color(0.5f, 0.35f, 0.1f); // Dry, brownish color
    [SerializeField] private Color healthyGrassColor = new Color(0.1f, 0.6f, 0.2f); // Healthy, green color
    [SerializeField] private float minHeightGrowth = 0.05f; // Minimum grass height
    [SerializeField] private float maxHeightGrowth = 1.34f; // Maximum grass height
    [SerializeField] private float minWidthGrowth = 0.05f; // Minimum grass width
    [SerializeField] private float maxWidthGrowth = 1.34f; // Maximum grass width
    [SerializeField] private int totalStages = 10; // Total number of growth stages
    [SerializeField] private float growthDuration = 1.0f; // Time it takes to complete one growth stage

    private int currentStage = 0; // Current growth stage
    private float heightPerStage; // Height increment per stage
    private float widthPerStage; // Width increment per stage
    private Coroutine growthCoroutine; // Reference to the current growth coroutine

    private void Start()
    {
        InitializeGrassProperties();

        // Calculate the height and width increments per stage
        heightPerStage = (maxHeightGrowth - minHeightGrowth) / totalStages;
        widthPerStage = (maxWidthGrowth - minWidthGrowth) / totalStages;

        // Initialize grass appearance
        UpdateGrassAppearance(minHeightGrowth, minWidthGrowth, dryGrassColor);
    }

    /// <summary>
    /// Adds progress when an item is dropped, growing the grass incrementally and smoothly.
    /// </summary>
    public void AddProgress()
    {
        if (currentStage < totalStages)
        {
            currentStage++;
            if (growthCoroutine != null) StopCoroutine(growthCoroutine);
            growthCoroutine = StartCoroutine(GrowGrass());
        }
        else
        {
            Debug.LogWarning("Grass is already fully grown.");
        }
    }

    /// <summary>
    /// Smoothly grows the grass over time using a coroutine.
    /// </summary>
    private IEnumerator GrowGrass()
    {
        float initialHeight = minHeightGrowth + (heightPerStage * (currentStage - 1));
        float initialWidth = minWidthGrowth + (widthPerStage * (currentStage - 1));
        float targetHeight = minHeightGrowth + (heightPerStage * currentStage);
        float targetWidth = minWidthGrowth + (widthPerStage * currentStage);

        float elapsedTime = 0f;

        while (elapsedTime < growthDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / growthDuration;

            // Interpolate between the current and target values
            float currentHeight = Mathf.Lerp(initialHeight, targetHeight, t);
            float currentWidth = Mathf.Lerp(initialWidth, targetWidth, t);
            Color currentColor = Color.Lerp(dryGrassColor, healthyGrassColor, (float)currentStage / totalStages);

            UpdateGrassAppearance(currentHeight, currentWidth, currentColor);

            yield return null; // Wait for the next frame
        }

        // Ensure the final values are set
        UpdateGrassAppearance(targetHeight, targetWidth, healthyGrassColor);
    }

    /// <summary>
    /// Initializes the grass properties for the terrain.
    /// </summary>
    private void InitializeGrassProperties()
    {
        if (terrain == null || terrain.terrainData == null)
        {
            Debug.LogError("Terrain is not assigned or missing terrain data.");
            return;
        }

        var detailPrototypes = terrain.terrainData.detailPrototypes;

        if (detailPrototypes.Length == 0)
        {
            Debug.LogError("No grass details found on the terrain.");
            return;
        }

        // Set the initial colors and sizes without affecting density
        foreach (var detail in detailPrototypes)
        {
            detail.dryColor = dryGrassColor;
            detail.healthyColor = dryGrassColor; // Start with dry grass
            detail.minHeight = minHeightGrowth; // Start at the minimum height
            detail.maxHeight = minHeightGrowth; // Start at the minimum height
            detail.minWidth = minWidthGrowth; // Start at the minimum width
            detail.maxWidth = minWidthGrowth; // Start at the minimum width
        }
    }

    /// <summary>
    /// Updates the grass appearance for the terrain.
    /// </summary>
    /// <param name="height">The current grass height.</param>
    /// <param name="width">The current grass width.</param>
    /// <param name="color">The current grass color.</param>
    private void UpdateGrassAppearance(float height, float width, Color color)
    {
        if (terrain == null || terrain.terrainData == null)
        {
            Debug.LogError("Terrain is not assigned or missing terrain data.");
            return;
        }

        var detailPrototypes = terrain.terrainData.detailPrototypes;

        // Update grass appearance based on the current height, width, and color
        foreach (var detail in detailPrototypes)
        {
            detail.healthyColor = color;
            detail.minHeight = height;
            detail.maxHeight = height;
            detail.minWidth = width;
            detail.maxWidth = width;
        }

        terrain.terrainData.detailPrototypes = detailPrototypes;
    }
}
