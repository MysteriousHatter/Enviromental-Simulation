// 10/4/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using UnityEditor;
using UnityEngine;

namespace RPG.Quests
{
    public class QuestSkipHandlerLegacy : MonoBehaviour
    {
        [SerializeField] Quest questToSkip; // Assign the quest to skip in the Inspector
        [SerializeField] string objectiveToSkip; // Assign the objective reference to skip in the Inspector

        private QuestList questList;

        private void Start()
        {
            // Find the QuestList component on the player
            questList = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
        }

        private void Update()
        {
            // Check for key presses using the Legacy Input Manager
            if (Input.GetKeyDown(KeyCode.Q)) // Skip the entire quest
            {
                if (questToSkip != null)
                {
                    questList.SkipQuest(questToSkip);
                }
            }

            if (Input.GetKeyDown(KeyCode.O)) // Skip a specific objective
            {
                if (questToSkip != null && !string.IsNullOrEmpty(objectiveToSkip))
                {
                    questList.SkipObjective(questToSkip, objectiveToSkip);
                }
            }
        }
    }
}
