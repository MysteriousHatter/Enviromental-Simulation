using System.Collections.Generic;
using UnityEngine;
using static RecyclableItem;
using UnityEngine.UI;
using TMPro;

public class Inventory : MonoBehaviour
{
    private Dictionary<RecyclableType, int> recyclables = new Dictionary<RecyclableType, int>();

    [Header("Recyclable UI Buttons")]
    [SerializeField] private Button[] recyclableButtons; // Assign buttons in the Inspector'
    [HideInInspector] public string currentRecycable = "";

    void Start()
    {
        // Initialize the dictionary with recyclable types
        foreach (RecyclableType type in System.Enum.GetValues(typeof(RecyclableType)))
        {
            recyclables[type] = 0;
        }
    }
    private void Update()
    {
        UpdateRecyclableButtons();
    }
    // Add recyclables to the inventory
    public void AddRecyclable(RecyclableType type, int amount)
    {
        if (recyclables.ContainsKey(type))
        {
            recyclables[type] += amount;
            Debug.Log($"Added {amount} of {type}. Total: {recyclables[type]}");
        }
    }

    // Get the count of a specific recyclable type
    public int GetRecyclableCount(RecyclableType type)
    {
        return recyclables.ContainsKey(type) ? recyclables[type] : 0;
    }

    /// <summary>
    /// Updates the button text to reflect current inventory values
    /// </summary>
    private void UpdateRecyclableButtons()
    {
        foreach (Button button in recyclableButtons)
        {
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText == null)
            {
                Debug.LogWarning($"Button {button.name} is missing a Text component.");
                continue;
            }
            Debug.Log($"The button's name {button.name}");

            // Match button name with recyclable type
            foreach (RecyclableType type in System.Enum.GetValues(typeof(RecyclableType)))
            {
                Debug.Log("What the dictionary says: " +type.ToString());
                if (button.name == type.ToString())
                {
                    buttonText.text = $"{type}: {recyclables[type]}";
                    Debug.Log($"Updated Button {button.name} with {type}: {recyclables[type]}");

                    break;
                }
            }
        }
    }

    public bool useRecycable(RecyclableType type)
    {
        if (recyclables.ContainsKey(type) && recyclables[type] > 0)
        {
            recyclables[type]--;
            FindAnyObjectByType<RecyclableSpawner>().setRecycableCount(-1);
            Debug.Log($"Used 1 {type}. Remaining: {recyclables[type]}");
            return true;
        }
        Debug.Log($"No {type} available to use.");
        return false;
    }

}
