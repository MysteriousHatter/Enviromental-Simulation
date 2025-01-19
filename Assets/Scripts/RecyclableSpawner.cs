using UnityEngine;

public class RecyclableSpawner : MonoBehaviour
{
    public string recyclableTag = "Recyclable"; // Tag used to identify recyclable items
    public Transform dropOffPoint; // Assign this in the Unity Inspector
    public float safeDistance = 5.0f; // Minimum distance from the drop-off point
    private static int totalRecycableCount; // Value that does not get modified
    protected static int maxRecycableCount; // Value that does

    void Awake()
    {
        FindRecyclables();
    }

    void FindRecyclables()
    {
        // Find all GameObjects with the specified recyclable tag
        GameObject[] recyclableItems = GameObject.FindGameObjectsWithTag(recyclableTag);
        int foundCount = 0;

        foreach (GameObject item in recyclableItems)
        {
            Vector3 itemPosition = item.transform.position;

            // Check if the item's position is at a safe distance from the drop-off point
            if (Vector3.Distance(itemPosition, dropOffPoint.position) >= safeDistance)
            {
                Debug.Log($"Found recyclable: {item.name} at position {itemPosition}");
                foundCount++;
                maxRecycableCount++;
                setRecycableCount(1);
            }
            else
            {
                Debug.Log($"Recyclable {item.name} is too close to the drop-off point and won't be counted.");
            }
        }

        Debug.Log($"Total recyclables found: {foundCount}");
    }

    public int getRecycableCount()
    {
        Debug.Log($"Total recyclable count: {maxRecycableCount}");
        return maxRecycableCount;
    }

    public void setRecycableCount(int count)
    {
        totalRecycableCount += count;
    }
}
