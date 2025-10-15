using System.Collections.Generic;
using UnityEngine;

public class WeedSideManager : MonoBehaviour
{
    [Header("Parent Object Containing Weeds")]
    public Transform weedParent;
    public List<Cuttable> weeds = new List<Cuttable>();

    private int initialWeedCount;

    private void Start()
    {
        if (weedParent == null)
        {
            Debug.LogError("Weed Parent is not assigned!");
            return;
        }

        // Count the initial number of weeds under the parent
        initialWeedCount = CountWeeds();
        Debug.Log($"Initial Weed Count: {initialWeedCount}");
    }

    /// <summary>
    /// Counts the number of active weed GameObjects under the parent.
    /// </summary>
    /// <returns>The number of active weeds.</returns>
    public int CountWeeds()
    {
        int count = 0;

        foreach (Cuttable child in weeds)
        {
            if (child.gameObject)
            {
                count++;
            }
        }

        return count;
    }


    public void RemoveWeed(Cuttable weed)
    {
        if (weeds.Contains(weed))
        {
            weeds.Remove(weed);
            Debug.Log($"Weed removed. Remaining weeds: {weeds.Count}");
        }
    }


    /// <summary>
    /// Checks if all weeds are cleared and logs a debug message if true.
    /// </summary>
    public void CheckIfAllWeedsCleared()
    {
        int remainingWeeds = CountWeeds();

        if (remainingWeeds == 0)
        {
            Debug.Log("All weeds are cleared! Update the score.");
            // You can add additional logic here, such as updating the score.
            GameManager.Instance.RegisterSideObjectiveCompleted("Clearing Weed");
        }
        else
        {
            Debug.Log($"Weeds remaining: {remainingWeeds}");
        }
    }
}
