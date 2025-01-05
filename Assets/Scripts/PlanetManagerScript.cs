using SoyWar.SimplePlantGrowth;
using UnityEngine;

public class PlanetManagerScript : MonoBehaviour
{
    [Header("Grass Component Settings")]
    [Tooltip("The GrassComponent that will receive the collectible.")]
    public GrassComponent grassComponent;

    [Tooltip("The specific GrassAsset associated with this collectible.")]
    public GrassAsset grassAsset;

    [Header("Interaction Settings")]
    [Tooltip("The tag of the object this collectible should interact with.")]
    public string targetTag = "GrassZone";



    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            grassComponent.UpdateGrass(1);
            // Log the collectible interaction
            Debug.Log($"CollectibleItem: Collectible added to GrassAsset '{grassAsset.name}'. Destroying self.");
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        // Check if the collided object has the correct tag
        if (other.CompareTag(targetTag))
        {
            if (grassAsset.Equals(null))
            {
                // Add the collectible to the grass component
                grassAsset.AddCollectible();

                // Log the collectible interaction
                Debug.Log($"CollectibleItem: Collectible added to GrassAsset '{grassAsset.name}'. Destroying self.");

                // Destroy this collectible object
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("CollectibleItem: GrassComponent or GrassAsset is not assigned.");
            }
        }
    }
}
