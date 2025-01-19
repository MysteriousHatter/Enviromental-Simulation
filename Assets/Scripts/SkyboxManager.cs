using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections;
using UnityEngine;

public class SkyboxManager : MonoBehaviour
{
    [SerializeField] private Cubemap cubemapDeforested;
    [SerializeField] private Cubemap cubemapForested;
    [SerializeField] private Gradient environmentGradient; // Gradient for light and fog
    [SerializeField] private Light globalLight;
    [SerializeField] private Terrain terrain;

    private float currentProgress = 0f; // The current interpolated progress (0 to 1)
    private float targetProgress = 0f; // The target progress to reach (0 to 1)
    [SerializeField] private float transitionSpeed = 0.1f; // Speed of progress interpolation

    [SerializeField] private float minFogDensity = 0.002f; // Minimum fog density for visibility
    [SerializeField] private float maxFogDensity = 0.01f;  // Maximum fog density for effect

    private void Start()
    {
        // Initialize the environment for the deforested state
        if (terrain == null)
        {
            terrain = Terrain.activeTerrain;
            if (terrain == null)
            {
                Debug.LogError("Terrain is not assigned and no active terrain found.");
            }
        }

        RenderSettings.skybox.SetTexture("_Cubemap1", cubemapDeforested);
        RenderSettings.skybox.SetTexture("_Cubemap2", cubemapForested);
        RenderSettings.skybox.SetFloat("_Blend", 0f);
        RenderSettings.fog = true; // Enable fog at the start
        RenderSettings.fogDensity = maxFogDensity; // Start with the maximum fog density
        RenderSettings.fogColor = environmentGradient.Evaluate(0f);
        InitializeTerrain();
    }

    public void Update()
    {
        // Gradually move current progress toward the target progress
        currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, transitionSpeed * Time.deltaTime);

        // Update environment based on the interpolated progress
        UpdateEnvironment(currentProgress);
    }

    public void SetProgress(float progress)
    {
        // Update the target progress (e.g., called by GameManager)
        targetProgress = Mathf.Clamp01(progress);
    }

    private void UpdateEnvironment(float progress)
    {
        // Update skybox blending
        RenderSettings.skybox.SetFloat("_Blend", progress);

        // Dynamically adjust fog based on progress
        if (progress > 0f && progress < 1f)
        {
            RenderSettings.fog = true; // Enable fog if within the progress range
            RenderSettings.fogDensity = Mathf.Lerp(maxFogDensity, minFogDensity, progress);
            RenderSettings.fogColor = environmentGradient.Evaluate(progress);
        }
        else
        {
            RenderSettings.fog = false; // Disable fog at the start or end of progress
        }

        // Update global light color
        globalLight.color = environmentGradient.Evaluate(progress);

        // Update terrain textures
        UpdateTerrainTextures(progress);
    }

    private void InitializeTerrain()
    {
        if (terrain.terrainData.alphamapLayers < 2)
        {
            Debug.LogError("Terrain must have at least two textures for blending.");
        }
    }

    private void UpdateTerrainTextures(float progress)
    {
        TerrainData terrainData = terrain.terrainData;
        int width = terrainData.alphamapWidth;
        int height = terrainData.alphamapHeight;

        // Get the current terrain splatmap
        float[,,] splatmap = terrainData.GetAlphamaps(0, 0, width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Blend between the two textures based on progress
                splatmap[y, x, 0] = Mathf.Lerp(1f, 0f, progress); // Deforested texture
                splatmap[y, x, 1] = Mathf.Lerp(0f, 1f, progress); // Forested texture
            }
        }

        // Apply the updated splatmap
        terrainData.SetAlphamaps(0, 0, splatmap);
    }


}

