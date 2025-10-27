using UnityEngine;

public class Plant : MonoBehaviour
{
    [Header("Mini-game")]
    public SeedType seedType = SeedType.Flower;   // replaces string plantType

    [Header("Planting")]
    public GameObject plantobj;
    [Tooltip("Only plant when we hit this tag. Leave empty to plant on any collision.")]
    public string requiredTag = "Ground";
    [Tooltip("Max surface slope (deg) allowed for planting.")]
    public float maxSlope = 60f;
    [Tooltip("Lift planted object slightly off the surface to avoid z-fighting.")]
    public float surfaceOffset = 0.02f;
    public LayerMask zoneLayers;                  // WaterGrowZone boxes

    // per-zone accounting
    readonly System.Collections.Generic.Dictionary<WaterGrowZoneConfig, int> _zoneHits = new();
    readonly System.Collections.Generic.HashSet<WaterGrowZoneConfig> _completed = new();

    // ---------- Trigger path (if your pots use triggers) ----------
    private void OnTriggerEnter(Collider other)
    {
        // If we touched a pot, score and consume the seed
        if (other.TryGetComponent<PotTarget>(out var pot))
        {
            pot.RegisterHit(seedType);
            Destroy(gameObject);
            return; // do NOT run planting logic for pots
        }

        // If your planting also supports triggers, you could mirror your collision logic here.
        // Otherwise, do nothing for non-pot triggers.
    }

    // ---------- Collision path (if your pots use non-trigger colliders) ----------
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("What's the collision name " + collision.gameObject.name);
        // 1) Pot hit? Score & exit early.
        if (collision.collider.TryGetComponent<PotTarget>(out var pot))
        {
            pot.RegisterHit(seedType);
            Destroy(gameObject);
            return;
        }

        // 2) Otherwise, run your existing planting logic.
        if (!string.IsNullOrEmpty(requiredTag) && !collision.collider.CompareTag(requiredTag))
            return;


        Debug.Log("Pass the tag test");
        if (collision.contactCount == 0) return;
        var contact = collision.GetContact(0);

        // Prevent planting on steep walls
        //if (Vector3.Angle(contact.normal, Vector3.up) > maxSlope)
        //{
        //    Destroy(gameObject);
        //    return;
        //}

        Vector3 pos = contact.point + contact.normal * surfaceOffset;
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);

        // Only proceed if we hit a valid zone layer
        if (((1 << collision.gameObject.layer) & zoneLayers.value) == 0)
            return;

        // Route to the right system based on seed type
        if (seedType == SeedType.Flower)
        {
            Debug.Log("Flower Type");
            if (collision.collider.TryGetComponent<SeedGrowZone>(out var grow))
            {
                grow.BeginGrowthProcess();
            }
            Destroy(gameObject);
        }
        else if(seedType == SeedType.Weed)
        {
            Debug.Log("Weed Type");
            if (collision.collider.TryGetComponent<WeedManager>(out var weeds))
                weeds.BeginGrowthProcess();
            Destroy(gameObject);
        }
    }
}
