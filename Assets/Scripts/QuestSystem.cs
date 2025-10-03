using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class QuestSystem : MonoBehaviour
{
    [Serializable]
    public class Objective
    {
        public string description;
        public bool isCompleted;
    }

    [Serializable]
    public class Quest
    {
        public string questName;
        public List<Objective> objectives = new List<Objective>();
        public bool isCompleted;
    }

    [Header("Data")]
    public List<Quest> quests = new List<Quest>();

    [Header("Visual / Scene Objects (match quest order)")]
    [SerializeField] private List<GameObject> QuestList = new List<GameObject>();

    public int currentQuestIndex { get; private set; } = -1;

    void Start()
    {
        if (quests.Count > 0)
        {
            StartQuest(0);
        }
        else
        {
            // No quests? Hide all quest GOs just in case.
            ActivateOnly(-1);
        }
    }

    public void StartQuest(int questIndex)
    {
        if (questIndex < 0 || questIndex >= quests.Count)
        {
            Debug.LogWarning($"StartQuest: index {questIndex} out of range.");
            return;
        }

        currentQuestIndex = questIndex;
        Debug.Log($"Started Quest: {quests[questIndex].questName}");

        // Turn on the matching quest GameObject and turn off others.
        ActivateOnly(questIndex);
    }

    public void CompleteObjective(int objectiveIndex)
    {
        if (currentQuestIndex < 0 || currentQuestIndex >= quests.Count) return;

        Quest currentQuest = quests[currentQuestIndex];
        if (objectiveIndex < 0 || objectiveIndex >= currentQuest.objectives.Count) return;

        currentQuest.objectives[objectiveIndex].isCompleted = true;
        Debug.Log($"Objective Completed: {currentQuest.objectives[objectiveIndex].description}");

        // If all objectives are completed, wrap up this quest.
        if (currentQuest.objectives.TrueForAll(obj => obj.isCompleted)) // checks every element in order
        {
            CompleteQuest();
        }
    }

    private void CompleteQuest()
    {
        Quest currentQuest = quests[currentQuestIndex];
        currentQuest.isCompleted = true;
        Debug.Log($"Quest Completed: {currentQuest.questName}");

        // Advance to next quest, if any
        int next = currentQuestIndex + 1;
        if (next < quests.Count)
        {
            StartQuest(next);
        }
        else
        {
            Debug.Log("All quests completed!");
            // Optional: hide all quest GOs at the end
            ActivateOnly(-1);
        }
    }

    /// <summary>
    /// Activates only the quest GameObject at 'index' and deactivates the rest.
    /// Pass -1 to deactivate all.
    /// </summary>
    private void ActivateOnly(int index)
    {
        for (int i = 0; i < QuestList.Count; i++)
        {
            var go = QuestList[i];
            if (!go) continue;

            bool shouldBeActive = (i == index);
            if (go.activeSelf != shouldBeActive)
            {
                go.SetActive(shouldBeActive); // toggles the local active state
            }
        }
    }
}
