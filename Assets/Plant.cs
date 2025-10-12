using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public GameObject plantobj;
    [Tooltip("Only plant when we hit this tag. Leave empty to plant on any collision.")]
    public string requiredTag = "Ground";
    [Tooltip("Max surface slope (deg) allowed for planting.")]
    public float maxSlope = 60f;
    [Tooltip("Lift planted object slightly off the surface to avoid z-fighting.")]
    public float surfaceOffset = 0.02f;
    public LayerMask zoneLayers;           // set to the layer your WaterGrowZone boxes live on
    [SerializeField] private string plantType;

    // per-zone accounting
    readonly Dictionary<WaterGrowZoneConfig, int> _zoneHits = new();
    readonly HashSet<WaterGrowZoneConfig> _completed = new();

    private void OnCollisionEnter(Collision collision)
    {
        if (!string.IsNullOrEmpty(requiredTag) && !collision.collider.CompareTag(requiredTag))
            return;

        if (collision.contactCount == 0) return;
        var contact = collision.GetContact(0);

        // Optional: prevent planting on steep walls
        if (Vector3.Angle(contact.normal, Vector3.up) > maxSlope)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 pos = contact.point + contact.normal * surfaceOffset;
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);


        if (((1 << collision.gameObject.layer) & zoneLayers.value) == 0)
        {
            return;
        }

        if (plantType == "Flower") { collision.gameObject.GetComponent<SeedGrowZone>().BeginGrowthProcess(); }
        else if(plantType == "Weed") 
        {
            Debug.Log("Destory Weeds");
            collision.gameObject.GetComponent<WeedManager>().BeginGrowthProcess();
            Destroy(gameObject);
        }


    }

}
