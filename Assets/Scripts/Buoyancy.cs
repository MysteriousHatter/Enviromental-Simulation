using UnityEngine;

public class Buoyancy : MonoBehaviour
{
    [SerializeField] private float buoyancyForce = 10f; // Strength of buoyancy
    [SerializeField] private float dragInWater = 2f; // Drag when submerged
    [SerializeField] private float angularDragInWater = 1f; // Angular drag when submerged
    [SerializeField] private Transform waterTransform; // Reference to the water object

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (waterTransform == null)
        {
            Debug.LogError("Water Transform is not assigned.");
        }
    }

    void FixedUpdate()
    {
        if (waterTransform != null)
        {
            float waterLevel = waterTransform.position.y;
            float objectDepth = waterLevel - transform.position.y;

            Debug.Log($"Water Level: {waterLevel}, Object Position Y: {transform.position.y}, Object Depth: {objectDepth}");


            if (objectDepth > 5) // Check if the object is submerged
            {

                // Apply upward buoyancy force proportional to the depth
                Vector3 upwardForce = Vector3.up * buoyancyForce * objectDepth;
                rb.AddForce(upwardForce, ForceMode.Force);

                // Apply drag
                rb.linearDamping = dragInWater;
                rb.angularDamping = angularDragInWater;
            }
            else
            {

                // Reset drag when out of water
                rb.linearDamping = 0f;
                rb.angularDamping = 0.05f; // Default angular drag
            }
        }
    }
}
