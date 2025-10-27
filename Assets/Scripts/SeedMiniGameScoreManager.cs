using GameDevTV.Inventories;
using RPG.Quests;
using UnityEngine;

public class SeedMiniGameScoreManager : MonoBehaviour
{
    [Header("Scoring")]
    [SerializeField] private int pointsPerCorrect = 10;
    [SerializeField] private int streakToWin = 3;

    [Header("Quest Hook")]
    [SerializeField] private QuestList questList;  // your existing system
    [SerializeField] Quest quest;
    [SerializeField] private string questObjectiveId;
    [SerializeField] private Inventory inventory;
    [SerializeField] private GameObject seedCollection;
    [SerializeField] private GameObject turnOffSpawner;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip JingleSFX;

    private int _score;
    private int _streak;

    public int Score => _score;
    public int Streak => _streak;

    public void ResetScore()
    {
        _score = 0;
        _streak = 0;
    }

    public void ReportHit(bool correct)
    {
        if (correct)
        {
            _score += pointsPerCorrect;
            _streak++;
            Debug.Log($"Correct! Score={_score}, Streak={_streak}");
            if (_streak >= streakToWin)
            {
                // Notify quest system once; safeguard against double-fire
                if (questList)
                {
                    FindFirstObjectByType<DialogBoxController>().SetHasSeed(true);
                    seedCollection.GetComponent<RecyclableItemComponent>().objectiveIndex = 2;
                    inventory.AddSeedToInventory(seedCollection);
                    questList.CompleteObjective(quest, questObjectiveId);
                    turnOffSpawner.SetActive(false);
                    audioSource.PlayOneShot(JingleSFX);
                    FindFirstObjectByType<DialogBoxController>().OpenPotSuccessWindow();

                }
                // Optionally: freeze minigame or reset streak
                // _streak = 0;
            }
        }
        else
        {
            // wrong hit resets streak only (keeps score)
            _streak = 0;
            // Debug.Log($"Wrong. Score={_score}, Streak reset.");
        }
    }
}
