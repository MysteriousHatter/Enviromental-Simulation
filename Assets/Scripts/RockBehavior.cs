using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class RockBehavior : MonoBehaviour
{
    public float fadeDuration = 5f; // Time in seconds to completely fade out
    public ParticleSystem destructionEffect; // Particle effect prefab for destruction

    private Material rockMaterial;
    private Color originalColor;
    private bool isHeld = false;
    private float fadeTimer = 0f;
    private XRGrabInteractable grabInteractable;
    private RockManager rockManager;

    void Start()
    {
        // Get the material and its original color
        rockMaterial = GetComponent<Renderer>().material;
        originalColor = rockMaterial.color;

        // Get the XRGrabInteractable component
        grabInteractable = GetComponent<XRGrabInteractable>();

        // Subscribe to select events
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);

        // Find the RockManager in the scene
        rockManager = FindObjectOfType<RockManager>();
        if (rockManager == null)
        {
            Debug.LogError("RockManager not found in the scene.");
        }
    }

    void Update()
    {
        if (isHeld)
        {
            // Increment the fade timer
            fadeTimer += Time.deltaTime;

            // Calculate the new alpha value
            float alpha = Mathf.Lerp(1f, 0f, fadeTimer / fadeDuration);

            // Apply the new color with the updated alpha
            rockMaterial.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            // Check if the rock should be deactivated
            if (fadeTimer >= fadeDuration)
            {
                // Trigger destruction particle effect
                if (destructionEffect != null)
                {
                    Instantiate(destructionEffect, transform.position, Quaternion.identity);
                }

                // Notify the RockManager to start the respawn process
                if (rockManager != null)
                {
                    rockManager.StartRespawnCoroutine();
                }

                // Deactivate this rock instance
                gameObject.SetActive(false);
            }
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        isHeld = true;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isHeld = false;
    }
}
