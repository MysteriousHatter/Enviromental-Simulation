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
        [SerializeField] private int seedObjective = 0;
        private bool isSeedCompleted;

        void Start()
        {
            // Assuming the player has a tag "Player" and the Inventory component
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerInventory = player.GetComponent<Inventory>();
            }
        }

        public void CompleteObjective()
        {
            if (!isSeedCompleted)
            {
                QuestList questList = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
                questList.CompleteObjective(quest, objective);
                congratsText.GetComponent<TextMeshProUGUI>().text = $"Seed is already submitted go back to Lab for your next assignment";
                GameManager.Instance.RegisterMainObjectiveCompleted(objective);
                playerInventory.RemoveSeedFromInventory();
                isSeedCompleted = true;
                //SecondSeedTrigger.SetActive(true);
                //garden.SetActive(true);
            }
            else
            {
                congratsText.GetComponent<TextMeshProUGUI>().text = $"Seed Is already submitted in this area";
            }
        }

        public int GetSeedObjective()
        {
            return seedObjective;
        }
    }
}
