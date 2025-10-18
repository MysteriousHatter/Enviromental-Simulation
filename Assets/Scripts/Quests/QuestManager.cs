using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Quests
{
    public class QuestManager : MonoBehaviour
    {
        [SerializeField] List<Quest> quest;
        private int currentIndex = 0;
        private Quest currentQuest;

        public void GiveFirstQuest()
        {
            Debug.Log("Get Quest");
            QuestList questList = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
            SetCurrentQuest(quest[0]);
            questList.AddQuest(quest[0]);

            
        }

        public void GiveNextQuest()
        {
            currentIndex++;
            QuestList questList = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
            if (currentIndex < quest.Count)
            {

                SetCurrentQuest(quest[currentIndex]);
                questList.AddQuest(quest[currentIndex]);
            }
            else
            {
                Debug.Log("All Quests are complete");
            }

        }

        public void SetCurrentQuest(Quest quest)
        {
            currentQuest = quest;
        }

        public Quest GetCurrentQuest()
        {
            return currentQuest;
        }

    }

}