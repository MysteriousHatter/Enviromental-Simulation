using RPG.Quests;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestCountManager : MonoBehaviour
{
    private List<GameObject> activeCollectaibles = new List<GameObject>();
    [SerializeField] private string objectiveName;
    [SerializeField] private GameObject seedObjective;

    private void Awake()
    {
        foreach (Transform child in this.transform)
        {
            activeCollectaibles.Add(child.gameObject);
        }
    }
    public void UnregisterCollectiable(GameObject mcGuffin)
    {
        if (activeCollectaibles.Contains(mcGuffin))
        {
            activeCollectaibles.Remove(mcGuffin);
            CheckAllItemsAreCollected();
        }
    }

    private void CheckAllItemsAreCollected()
    {
        if (activeCollectaibles.Count == 0)
        {
            Debug.Log("All fires extinguished! Quest complete.");
            if(seedObjective != null) seedObjective.SetActive(true);
            CompleteQuest();
        }
    }

    private void CompleteQuest()
    {
        QuestList questList = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
        if (questList != null)
        {
            questList.CompleteObjectiveByReference(objectiveName);
        }
    }
}
