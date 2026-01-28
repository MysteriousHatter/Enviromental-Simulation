using UnityEngine;

public class GazeRaycaster : MonoBehaviour
{
    private Camera vrCamera; // Assign your main VR Camera here (e.g., CenterEyeAnchor)
    [SerializeField] private float rayDistance = 100f; // How far the ray goes
    [SerializeField] private LayerMask interactableLayer; // Which layers can be hit

    void Start()
    {
        // Find your VR Camera (e.g., the one in your OVRCameraRig/Main Camera)
        vrCamera = Camera.main; // Or find it by name/tag if needed
    }

    void Update()
    {
        // Create a ray from the camera's position forward
        Ray ray = new Ray(vrCamera.transform.position, vrCamera.transform.forward);
        RaycastHit hit;

        // Draw the ray in the Scene view for debugging (optional)
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.yellow);

        // Perform the raycast
        if (Physics.Raycast(ray, out hit, rayDistance, interactableLayer))
        {
            // We hit something!
            Debug.Log("Hit: " + hit.transform.name);
            GameManager.Instance.tutorialManager.ShowNextTutorial();
            hit.transform.gameObject.SetActive(false);
        }
    }
}
