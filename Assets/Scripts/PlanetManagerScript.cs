
using TMPro;
using UnityEngine;

namespace RPG.Quests
{
    public class PlanetManagerScript : MonoBehaviour
    {
        private Inventory playerInventory;
        [SerializeField] Quest quest;
        [SerializeField] string objective;
        [SerializeField] private GameObject SecondSeedTrigger;
        [SerializeField] private bool isMultiSeed;
        [SerializeField] private GameObject congratsText;
        [SerializeField] private GameObject garden;
        private int seedCount = 0;

        void Start()
        {
            // Assuming the player has a tag "Player" and the Inventory component
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerInventory = player.GetComponent<Inventory>();
                seedCount = 0;
            }
        }

        public void CompleteObjective()
        {
            if (!isMultiSeed)
            {
                QuestList questList = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
                questList.CompleteObjective(quest, objective);
                congratsText.GetComponent<TextMeshProUGUI>().text = $"Seed is already submitted go back to Lab for your next assignment";
                GameManager.Instance.RegisterMainObjectiveCompleted(objective);
                SecondSeedTrigger.SetActive(true);
                garden.SetActive(true);
            }
            else
            {
                seedCount++;
                if (seedCount >= 2)
                {
                    QuestList questList = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
                    questList.CompleteObjective(quest, objective);
                    congratsText.GetComponent<TextMeshProUGUI>().text = $"Seed is already submitted go back to Lab for your next assignment";
                    GameManager.Instance.RegisterMainObjectiveCompleted(objective);
                    SecondSeedTrigger.SetActive(true);
                }
                else
                {
                    congratsText.GetComponent<TextMeshProUGUI>().text = $"You need one more seed for this conpartment";
                    GameManager.Instance.RegisterMainObjectiveCompleted(objective);;
                }
            }
        }

        //void OnTriggerEnter(Collider other)
        //{
        //    if (other.CompareTag("Recyclable"))
        //    {
        //        RecyclableItem item = other.GetComponent<RecyclableItemComponent>().recyclableItem;
        //        if (item != null && playerInventory != null)
        //        {
        //            playerInventory.AddRecyclable(item.type, 1);
        //            Destroy(other.gameObject); // Remove the item from the scene
        //        }
        //    }
        //}
    }
}
