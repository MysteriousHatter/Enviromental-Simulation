using System;
using System.Collections;
using System.Collections.Generic;
using GameDevTV.Inventories;
using GameDevTV.Saving;
using GameDevTV.Utils;
using UnityEngine;

namespace RPG.Quests
{
    public class QuestList : MonoBehaviour, ISaveable, IPredicateEvaluator
    {
        List<QuestStatus> statuses = new List<QuestStatus>();
        [SerializeField] private List<GameObject> seedInventory = new List<GameObject>();

        public event Action onUpdate;

        private void Update() {
            CompleteObjectivesByPredicates();
        }

        public void AddQuest(Quest quest)
        {
            if (HasQuest(quest)) return;
            QuestStatus newStatus = new QuestStatus(quest);
            statuses.Add(newStatus);
            if (onUpdate != null)
            {
                Debug.Log("Update UI with New Quest and the quest is " +  newStatus.GetQuest().name);
                onUpdate();
            }
        }

        public void CompleteObjective(Quest quest, string objective)
        {
            Debug.Log("Find the status");
            QuestStatus status = GetQuestStatus(quest);
            status.CompleteObjective(objective);
            if (status.IsComplete())
            {
                Debug.Log("Completed an objective");
                //Change it to a diffrent objective like Heal World
                //GiveReward(quest);
            }
            if (onUpdate != null)
            {
                onUpdate();
            }
        }

        public bool HasQuest(Quest quest)
        {
            return GetQuestStatus(quest) != null;
        }

        public IEnumerable<QuestStatus> GetStatuses()
        {
            return statuses;
        }

        private QuestStatus GetQuestStatus(Quest quest)
        {
            foreach (QuestStatus status in statuses)
            {
                if (status.GetQuest() == quest)
                {
                    return status;
                }
            }
            return null;
        }

        public void CompleteObjectiveByReference(string objectiveReference)
        {
            foreach (QuestStatus status in statuses)
            {
                Quest quest = status.GetQuest();
                foreach (var objective in quest.GetObjectives())
                {
                    if (objective.reference == objectiveReference && !status.IsObjectiveComplete(objectiveReference))
                    {
                        CompleteObjective(quest, objectiveReference);
                        return;
                    }
                }
            }
        }

        //private void GiveReward(Quest quest)
        //{
        //    foreach (var reward in quest.GetRewards())
        //    {
        //        bool success = GetComponent<Inventory>().AddToFirstEmptySlot(reward.item, reward.number);
        //        if (!success)
        //        {
        //            GetComponent<ItemDropper>().DropItem(reward.item, reward.number);
        //        }
        //    }
        //}

        private void CompleteObjectivesByPredicates()
        {
            foreach (QuestStatus status in statuses)
            {
                if (status.IsComplete()) continue;
                Quest quest = status.GetQuest();
                foreach (var objective in quest.GetObjectives())
                {
                    if (status.IsObjectiveComplete(objective.reference)) continue;
                    if (!objective.usesCondition) continue;
                    if (objective.completionCondition.Check(GetComponents<IPredicateEvaluator>()))
                    {
                        Debug.Log("Objective is completed");
                        CompleteObjective(quest, objective.reference);
                    }
                }
            }
        }

        public object CaptureState()
        {
            List<object> state = new List<object>();
            foreach (QuestStatus status in statuses)
            {
                state.Add(status.CaptureState());
            }
            return state;
        }

        public void RestoreState(object state)
        {
            List<object> stateList = state as List<object>;
            if (stateList == null) return;

            statuses.Clear();
            foreach (object objectState in stateList)
            {
                statuses.Add(new QuestStatus(objectState));
            }
        }

        public bool? Evaluate(string predicate, string[] parameters)
        {
            switch (predicate)
            {
                case "HasQuest": 
                return HasQuest(Quest.GetByName(parameters[0]));
                case "CompletedQuest":
                return GetQuestStatus(Quest.GetByName(parameters[0])).IsComplete();
            }

            return null;
        }

        public void SkipObjective(Quest quest, string objective)
        {
            QuestStatus status = GetQuestStatus(quest);
            if (status != null && !status.IsObjectiveComplete(objective))
            {
                status.CompleteObjective(objective);
                Debug.Log($"Objective '{objective}' in quest '{quest.GetTitle()}' skipped.");
                if (onUpdate != null)
                {
                    onUpdate();
                }
            }
        }

        public void SkipQuest(Quest quest)
        {
            QuestStatus status = GetQuestStatus(quest);
            if (status != null)
            {
                foreach (var objective in quest.GetObjectives())
                {
                    if (!status.IsObjectiveComplete(objective.reference))
                    {
                        status.CompleteObjective(objective.reference);
                    }
                }
                Debug.Log($"Quest '{quest.GetTitle()}' skipped.");
                if (onUpdate != null)
                {
                    onUpdate();
                }
            }
        }
    }

}