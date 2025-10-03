// 9/29/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using UnityEditor;
using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    public int questIndex; // The index of the quest in the QuestSystem
    public int objectiveIndex; // The index of the objective in the quest

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            QuestSystem questSystem = FindObjectOfType<QuestSystem>();
            if (questSystem != null)
            {
                Debug.Log($"Player collected item for Quest {questIndex}, Objective {objectiveIndex}");
                questSystem.CompleteObjective(objectiveIndex);
                Destroy(gameObject); // Remove the item from the scene
            }
        }
    }
}
