using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Quests
{
    public class QuestCompletion : MonoBehaviour
    {
        [SerializeField] Quest quest;
        [SerializeField] string objective;
        [SerializeField] private GameObject questStarterPrefab;
        [SerializeField] private GameObject turnOffLevel;
        [SerializeField] private bool isQuest;


        private void Start()
        {
            if(questStarterPrefab != null) questStarterPrefab.SetActive(false);
        }
        public void CompleteObjective()
        {
            QuestList questList = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
            questList.CompleteObjective(quest, objective);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if(isQuest)
                {
                    GameManager.Instance.QuestManager.GiveNextQuest();
                    if(turnOffLevel != null) { turnOffLevel.SetActive(false); }
                }
                else
                {
                    CompleteObjective();
                    if (turnOffLevel != null) { turnOffLevel.SetActive(false); }
                }

                if (questStarterPrefab != null)
                {
                    questStarterPrefab.SetActive(true);
                }
                this.gameObject.SetActive(false);
            }
        }


    }
}