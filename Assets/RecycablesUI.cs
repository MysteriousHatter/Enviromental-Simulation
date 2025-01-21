using TMPro;
using UnityEngine;

public class RecycablesUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text recyclablesText; // Text element to update
    [SerializeField] private RecyclableSpawner spawner;

    private int totalRecyclables; // Total recyclables in the game
    private int collectedRecyclables; // Recyclables collected by the player

    private void Start()
    {
        // Assuming GameManager is a Singleton
        totalRecyclables = spawner.getRecycableCount(); 
        collectedRecyclables = GameManager.Instance.currentScore;

        // Initialize the text UI
        UpdateRecyclablesUI();
    }

    private void Update()
    {
        // Continuously fetch updated counts (optional if values change dynamically)
        collectedRecyclables = GameManager.Instance.currentScore;
        UpdateRecyclablesUI();
    }

    private void UpdateRecyclablesUI()
    {
        // Update the text element to display the collected/total recyclables
        recyclablesText.text = $"{collectedRecyclables}/{totalRecyclables}";
    }
}
