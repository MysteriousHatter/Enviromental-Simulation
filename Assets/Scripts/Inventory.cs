using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static RecyclableItem;

public class Inventory : MonoBehaviour
{
    private Dictionary<RecyclableType, int> recyclables = new Dictionary<RecyclableType, int>();

    [Header("Recyclable UI Buttons")]
    [SerializeField] private Button[] recyclableButtons; // Assign buttons in the Inspector'
    [HideInInspector] public string currentRecycable = "";
    [SerializeField] private DialogBoxController dialogBoxController;
    [SerializeField] private RecyclableSpawner recyclableSpawner;

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
    public int AddRecyclable(RecyclableType type, int amount, Sprite itemSprite, string description)
    {
        if (recyclables.ContainsKey(type))
        {
            for(int i = 0; i < dialogBoxController.itemSlot.Length; i++)
            {
                if(dialogBoxController.itemSlot[i].isFull == false && dialogBoxController.itemSlot[i].itemName == type.ToString() || dialogBoxController.itemSlot[i].quantity == 0)
                {
                    Debug.Log("We are full and our name mathces the slot Orr our quanity equals zero");
                    int leftOverItems = dialogBoxController.itemSlot[i].AddItem(type, amount, itemSprite, description);
                    recyclables[type] += amount;
                    Debug.Log("Our left over items " + leftOverItems);
                    if (leftOverItems > 0)
                    {
                        leftOverItems = AddRecyclable(type, leftOverItems, itemSprite, description);
                    }
                    return leftOverItems;
                }
            }
        }
        return amount;
    }

    public void DeselectAllSlots()
    {
        for(int i = 0; i < dialogBoxController.itemSlot.Length;i++)
        {
            dialogBoxController.itemSlot[i].selectedShader.SetActive(false);
            dialogBoxController.itemSlot[i].thisItemSelected = false;
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
                Debug.Log("What the dictionary says: " + type.ToString());
                if (button.name == type.ToString())
                {
                    buttonText.text = $"{type}: {recyclables[type]}";
                    Debug.Log($"Updated Button {button.name} with {type}: {recyclables[type]}");

                    break;
                }
            }
        }
    }

    public void OnButtonPress(string material)
    {
        Inventory inventory = FindAnyObjectByType<Inventory>();
        recyclableSpawner.placeholderRecyacableCount--;

        if (System.Enum.TryParse(material, out RecyclableType type))
        {
            Debug.Log("Current recycable type " + recyclableSpawner.currentRecyclableType);
            // Check if the selected recyclable type matches the current recyclable type
            if (recyclableSpawner.currentRecyclableType == type)
            {
                Debug.Log($"Correct Item: {type}. Score Updated!");
                // Correct selection
                if (inventory.useRecycable(type)) // Check if the item is available in inventory
                {
                    Debug.Log("We have a match" + type);
                    GameManager.Instance.currentScore++;
                    GameManager.Instance.CheckProgress();
                }
                else
                {
                    Debug.Log($"Item {type} not available in inventory.");
                }
            }
            else
            {
                // Incorrect selection
                Debug.Log($"Incorrect selection! Expected: {recyclableSpawner.currentRecyclableType}, but selected: {type}");
                GameManager.Instance.currentScore++;
                useRecycable(type); // Attempt to use the item, even if incorrect
            }
        }
        else
        {
            Debug.LogError($"Invalid material: {material}");
        }
    }

    public bool useRecycable(RecyclableType type)
    {
        Debug.Log("We have this amount of " + type.ToString() + recyclables[type]);
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