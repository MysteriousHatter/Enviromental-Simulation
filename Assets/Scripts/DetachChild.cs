using UnityEngine;

public class DetachChild : MonoBehaviour
{
    // Reference to the child object you want to detach
    public Transform childToDetach;

    public void DropItem()
    {
        // Detach the child object
        childToDetach.SetParent(null);

        // Ensure the child has a Rigidbody component
        Rigidbody rb = childToDetach.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Enable gravity
            rb.useGravity = true;
        }
        else
        {
            Debug.LogWarning("The child object lacks a Rigidbody component.");
        }

        Debug.Log($"{childToDetach.name} has been detached and will now fall due to gravity.");
    }
}
