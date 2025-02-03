
using UnityEngine;

public class PlanetManagerScript : MonoBehaviour
{
    private Inventory playerInventory;

    void Start()
    {
        // Assuming the player has a tag "Player" and the Inventory component
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerInventory = player.GetComponent<Inventory>();
        }
    }

    //void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("Recyclable"))
    //    {
    //        RecyclableItem item = other.GetComponent<RecyclableItemComponent>().recyclableItem;
    //        if (item != null && playerInventory != null)
    //        {
    //            playerInventory.AddRecyclable(item.type, 1);
    //            Destroy(other.gameObject); // Remove the item from the scene
    //        }
    //    }
    //}
}
