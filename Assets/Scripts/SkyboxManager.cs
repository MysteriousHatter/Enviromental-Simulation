using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections;
using UnityEngine;

public class SkyboxManager : MonoBehaviour
{
    [SerializeField] private Cubemap cubemapDeforested; // Initial deforested state
    [SerializeField] private Cubemap cubemapForested; // Final forested state
    [SerializeField] private Gradient environmentGradient; // Gradient for light and fog changes
    [SerializeField] private Light globalLight; // Directional light for the scene
    [SerializeField] private Terrain terrain; // Terrain to transition

    private float transitionDuration = 90f; // Total duration for the environment transition
    private float elapsedTransitionTime = 0f;
    private bool transitioning = false;

    public void Start()
    {
        // Initialize the skybox and fog for the deforested state
        RenderSettings.skybox.SetTexture("_Cubemap1", cubemapDeforested);
        RenderSettings.skybox.SetTexture("_Cubemap2", cubemapForested);
        RenderSettings.skybox.SetFloat("_Blend", 0f); // Start fully deforested
        RenderSettings.fogDensity = 0.05f; // High fog density to represent pollution
        RenderSettings.fogColor = environmentGradient.Evaluate(0f);

        InitializeTerrain();
    }

    public void Update()
    {
        // Trigger the environment transition for testing
        if (!transitioning && Input.GetKeyDown(KeyCode.T)) // Example: press 'T' to start
        {
            StartCoroutine(TransitionEnvironment());
        }
    }

    private void InitializeTerrain()
    {
        // Ensure the terrain has at least two textures
        if (terrain.terrainData.alphamapLayers < 2)
        {
            Debug.LogError("Terrain must have at least two textures for blending.");
        }
    }
    private IEnumerator TransitionEnvironment()
    {
        transitioning = true;

        while (elapsedTransitionTime < transitionDuration)
        {
            float progress = elapsedTransitionTime / transitionDuration;

            // Skybox transition
            RenderSettings.skybox.SetFloat("_Blend", progress);

            // Fog and lighting
            RenderSettings.fogDensity = Mathf.Lerp(0.05f, 0.01f, progress);
            RenderSettings.fogColor = environmentGradient.Evaluate(progress);
            globalLight.color = environmentGradient.Evaluate(progress);

            // Terrain texture blending
            UpdateTerrainTextures(progress);

            elapsedTransitionTime += Time.deltaTime;
            yield return null;
        }

        // Final state
        RenderSettings.skybox.SetFloat("_Blend", 1f);
        RenderSettings.skybox.SetTexture("_Cubemap1", cubemapForested); // Set forested cubemap
        RenderSettings.fogDensity = 0.01f;
        RenderSettings.fogColor = environmentGradient.Evaluate(1f);
        globalLight.color = environmentGradient.Evaluate(1f);
        FinalizeTerrainTextures();

        transitioning = false;
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

    private void FinalizeTerrainTextures()
    {
        TerrainData terrainData = terrain.terrainData;
        int width = terrainData.alphamapWidth;
        int height = terrainData.alphamapHeight;

        // Ensure the final terrain state is set
        float[,,] splatmap = terrainData.GetAlphamaps(0, 0, width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                splatmap[y, x, 0] = 0f; // Fully forested
                splatmap[y, x, 1] = 1f;
            }
        }

        terrainData.SetAlphamaps(0, 0, splatmap);
    }

}

