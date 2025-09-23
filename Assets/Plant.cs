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

        if (plantobj != null)
            Instantiate(plantobj, pos, rot);

        Destroy(gameObject);
    }
}
