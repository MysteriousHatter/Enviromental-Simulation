using UnityEngine;
using UnityMeshSimplifier;

public class WeaponPickUp : MonoBehaviour
{
    private Collision weapon;
    private bool CanPickUp = false;

    private void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.tag == "Weapon" && Input.GetKey(KeyCode.Space))
        {
            Debug.Log("Collision Start");
            collision.transform.SetParent(this.transform);
        }

        if (Input.GetKey(KeyCode.R))
        {
            Debug.Log("Released");
            collision.transform.SetParent(null);
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Weapon")
        {
            Debug.Log("Collision End");
        }
    }
}
