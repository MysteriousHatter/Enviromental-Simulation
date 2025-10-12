using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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


    public Volume volume; // Reference to the Post-Processing Volume
    private Bloom bloom;
    private ColorAdjustments colorAdjustments;
    private Vignette vignette;
    private FilmGrain filmGrain;
    private DepthOfField depthOfField;

    [Header("Depth Of Field Values")]
    [SerializeField] private float startDepthOfField = 50f;
    [SerializeField] private float endDepthOfField = 70f;
    [SerializeField] private float maxRadius = 2f;


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

        // Ensure the volume has a profile assigned and get overrides
        if (volume.profile.TryGet(out bloom) &&
            volume.profile.TryGet(out colorAdjustments) &&
            volume.profile.TryGet(out vignette) &&
            volume.profile.TryGet(out filmGrain) &&
            volume.profile.TryGet(out depthOfField))
        {
            Debug.Log("Post-Processing overrides successfully retrieved.");
        }
        else
        {
            Debug.LogError("Post-Processing overrides missing in Volume Profile.");
        }

        InitializeTerrain();
    }

    private void Update()
    {

        // Gradually move current progress toward the target progress
        currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, transitionSpeed * Time.deltaTime);

        // Update environment based on the interpolated progress
        UpdateEnvironment(currentProgress);
        // Update post-processing profile
        UpdatePostProcessing(currentProgress);

    }

    private void UpdatePostProcessing(float progress)
    {
        // Adjust Bloom
        bloom.intensity.value = Mathf.Lerp(0.5f, 2.0f, progress); // More vibrant in the forested state
        bloom.tint.value = Color.Lerp(new Color(0.7f, 0.6f, 0.4f), new Color(0.4f, 0.8f, 0.5f), progress);

        // Adjust Color Adjustments
        colorAdjustments.postExposure.value = Mathf.Lerp(-0.5f, 0.2f, progress); // Brightens in the forested state
        colorAdjustments.saturation.value = Mathf.Lerp(-30.5f, 20f, progress); // Desaturated to vibrant
        colorAdjustments.contrast.value = Mathf.Lerp(30.8f, 10f, progress); // High contrast to soft

        // Adjust Vignette
        vignette.intensity.value = Mathf.Lerp(0.444f, 0.1f, progress); // Strong in deforested, subtle in forested
        vignette.smoothness.value = Mathf.Lerp(0.225f, 0.6f, progress); // Smooths out in the forested state

        // Adjust Film Grain
        filmGrain.intensity.value = Mathf.Lerp(0.195f, 0.05f, progress); // Less grain in the forested state

        // Adjust Depth of Field
        depthOfField.gaussianStart.value = Mathf.Lerp(startDepthOfField, 10f, progress); // Bring focus closer in the forested state
        depthOfField.gaussianEnd.value = Mathf.Lerp(endDepthOfField, 30f, progress);   // Blur the background more in the forested state
        depthOfField.gaussianMaxRadius.value = Mathf.Lerp(maxRadius, 1f, progress); // Subtle blur in forested
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
            //RenderSettings.fog = false; // Disable fog at the start or end of progress
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
                splatmap[y, x, 1] = Mathf.Lerp(1f, 0f, progress); // Deforested texture
                splatmap[y, x, 0] = Mathf.Lerp(0f, 1f, progress); // Forested texture
            }
        }

        // Apply the updated splatmap
        terrainData.SetAlphamaps(0, 0, splatmap);
    }


}

