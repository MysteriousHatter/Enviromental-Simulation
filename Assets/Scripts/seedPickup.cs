using BioTools;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class seedPickup : MonoBehaviour
{
    [SerializeField] private int pelletAmount = 100; // Amount of pellets the flower provides
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private AudioSource pickupSound; // Optional: Add a sound effect for picking up the flower
    [SerializeField] private WeaponWheelManager weaponWheel;
    [SerializeField] private string seedType;


    private void Awake()
    {
        // Ensure the XRGrabInteractable component is attached
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable == null)
        {
            grabInteractable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        }

        // Optional: Add an AudioSource for pickup sound
        pickupSound = GetComponent<AudioSource>();
        foreach (GameObject tool in weaponWheel.totalToolsAvailable)
        {
            if (tool.TryGetComponent<SeedSpreaderTool>(out SeedSpreaderTool component))
            {
                component.ReloadPellets(pelletAmount, seedType);
            }
            else
            {
                Debug.LogWarning("Seed Spreader not found in the list.");
            }
        }
    }

    private void OnEnable()
    {
        // Subscribe to the grab event
        grabInteractable.selectEntered.AddListener(OnGrabbed);
    }

    private void OnDisable()
    {
        // Unsubscribe from the grab event
        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        // Play pickup sound if available
        if (pickupSound != null)
        {
            pickupSound.Play();
        }

        // Find the Seed Spreader in the weaponWheelManager
        foreach (GameObject tool in weaponWheel.totalToolsAvailable)
        {
            if (tool.TryGetComponent<SeedSpreaderTool>(out SeedSpreaderTool component))
            {
                component.ReloadPellets(pelletAmount, seedType);
            }
            else
            {
                Debug.LogWarning("Seed Spreader not found in the list.");
            }
        }

        // Turn the flower GameObject inactive
        gameObject.SetActive(false);
    }
}
