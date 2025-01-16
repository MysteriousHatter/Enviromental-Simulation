using UnityEngine;

public class GrassGrowthManager : MonoBehaviour
{
    [SerializeField] private Terrain terrain;
    [SerializeField] private Color dryGrassColor = new Color(0.5f, 0.35f, 0.1f); // Dry, brownish color
    [SerializeField] private Color healthyGrassColor = new Color(0.1f, 0.6f, 0.2f); // Healthy, green color
    [SerializeField] private float transitionSpeed = 0.1f;
    [SerializeField] private Texture2D[] grassTextures; // Array of different grass textures
    [SerializeField] private float minHeightGrowth = 0.05f;
    [SerializeField] private float maxHeightGrowth = 1.34f;

    private float currentProgress = 0f;
    private float targetProgress = 0f;

    private void Start()
    {
        InitializeGrassProperties();
    }

    private void Update()
    {
        // Gradually interpolate current progress toward the target progress
        currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, transitionSpeed * Time.deltaTime);
        UpdateGrassAppearance(currentProgress);
    }

    public void SetProgress(float progress)
    {
        // Clamp and set the target progress
        targetProgress = Mathf.Clamp01(progress);
    }

    private void InitializeGrassProperties()
    {
        if (terrain == null || terrain.terrainData.detailPrototypes.Length == 0)
        {
            Debug.LogError("No grass details found or terrain is missing.");
            return;
        }

        // Initialize properties without reassigning textures
        var detailPrototypes = terrain.terrainData.detailPrototypes;
        foreach (var detail in detailPrototypes)
        {
            detail.dryColor = dryGrassColor;
            detail.healthyColor = dryGrassColor; // Start with dry grass
            detail.minHeight = minHeightGrowth; // Initial height
            detail.maxHeight = minHeightGrowth; // Initial height
        }
        terrain.terrainData.detailPrototypes = detailPrototypes;
    }

    private void UpdateGrassAppearance(float progress)
    {
        TerrainData terrainData = terrain.terrainData;

        // Interpolate grass color and height for each detail layer
        var detailPrototypes = terrainData.detailPrototypes;
        foreach (var detail in detailPrototypes)
        {
            detail.healthyColor = Color.Lerp(dryGrassColor, healthyGrassColor, progress);
            detail.minHeight = Mathf.Lerp(minHeightGrowth, maxHeightGrowth, progress);
            detail.maxHeight = Mathf.Lerp(minHeightGrowth, maxHeightGrowth, progress);
        }

        terrainData.detailPrototypes = detailPrototypes;
    }
}
