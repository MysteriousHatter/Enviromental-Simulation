using UnityEngine;

public class RecyclableSpawner : MonoBehaviour
{
    public RecyclableItem[] recyclableItems; // Array to hold different recyclable prefabs
    public int[] itemCounts; // Corresponding array to specify the number of each recyclable to spawn
    public Vector3 spawnAreaMin; // Minimum coordinates of the spawn area
    public Vector3 spawnAreaMax; // Maximum coordinates of the spawn area

    public Transform dropOffPoint; // Assign this in the Unity Inspector
    public float safeDistance = 5.0f; // Minimum distance from the drop-off point
    private int totalRecycableCount;

    void Start()
    {
        SpawnRecyclables();
    }

    void SpawnRecyclables()
    {
        for (int i = 0; i < recyclableItems.Length; i++)
        {
            int spawnedCount = 0;
            while (spawnedCount < itemCounts[i])
            {
                Vector3 spawnPosition = new Vector3(
                    Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                    Random.Range(spawnAreaMin.y, spawnAreaMax.y),
                    Random.Range(spawnAreaMin.z, spawnAreaMax.z)
                );

                // Check if the spawn position is at a safe distance from the drop-off point
                if (Vector3.Distance(spawnPosition, dropOffPoint.position) >= safeDistance)
                {
                    Instantiate(recyclableItems[i].prefab, spawnPosition, Quaternion.identity, this.transform);
                    spawnedCount++;
                    totalRecycableCount++;
                }
                // Optional: Add a safety mechanism to prevent infinite loops
                // if a valid spawn position cannot be found after a certain number of attempts
            }
        }
    }

    public int getRecycableCount()
    {
        return totalRecycableCount;
    }

    public void setRecycableCount(int count)
    {
        totalRecycableCount += count;
    }
}
