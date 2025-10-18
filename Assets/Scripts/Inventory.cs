using RPG.Quests;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static RecyclableItem;


/// <summary>
/// TODO: Find a way to have both item inventory and object inventory concide with eacth other and when to use, will come into play with crafting system.
/// </summary>
public class Inventory : MonoBehaviour
{
    private Dictionary<RecyclableType, int> recyclables = new Dictionary<RecyclableType, int>();
    private List<PhotoData> photos = new List<PhotoData>();
    [SerializeField] private QuestSystem system;

    [Header("Recyclable UI Buttons")]
    [SerializeField] private Button[] recyclableButtons; // Assign buttons in the Inspector'
    [HideInInspector] public string currentRecycable = "";
    [SerializeField] private DialogBoxController dialogBoxController;
    [SerializeField] private RecyclableSpawner recyclableSpawner;
    [SerializeField] private PlanetManagerScript planetManagerScript;

    [SerializeField] private List<GameObject> seedInventory = new List<GameObject>(); // Exclusive inventory for seeds


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
    public int AddRecyclable(RecyclableType type, int amount, Sprite itemSprite, string description, int ObjectiveNum = 0)
    {
        if (recyclables.ContainsKey(type))
        {
            for(int i = 0; i < dialogBoxController.itemSlot.Length; i++)
            {
                if(dialogBoxController.itemSlot[i].isFull == false && dialogBoxController.itemSlot[i].itemName == type.ToString() || dialogBoxController.itemSlot[i].quantity == 0)
                {
                    Debug.Log("We are full and our name mathces the slot Orr our quanity equals zero");
                    int leftOverItems = dialogBoxController.itemSlot[i].AddItem(type, amount, itemSprite, description, ObjectiveNum);
                    recyclables[type] += amount;
                    Debug.Log("Our left over items " + leftOverItems);
                    if (leftOverItems > 0)
                    {
                        leftOverItems = AddRecyclable(type, leftOverItems, itemSprite, description, ObjectiveNum);
                    }
                    return leftOverItems;
                }
            }
        }
        return amount;
    }

    public void AddPhoto(PhotoData photoData)
    {
        if(photoData == null)
        {
            Debug.LogWarning("Tried to add a null photo to inventory.");
            return;
        }
        photos.Add(photoData);

        for (int i = 0; i < dialogBoxController.photoSlot.Length; i++)
        {
            var slot = dialogBoxController.photoSlot[i];

            if (!slot.isFull || slot.quantity == 0)
            {
                // Instead of RecyclableType, just set the slot directly
                slot.AddPhotoItem(photoData);
                Debug.Log($"Photo added to slot {i}: {photoData.speciesName}");
                return;
            }

        }
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

    public void OnButtonPress(string material, int currentObjectiveIndex = 0)
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
                    planetManagerScript.CompleteObjective();
                    //GameManager.Instance.currentScore++;
                    //GameManager.Instance.CheckProgress();
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
                //GameManager.Instance.currentScore++;
                useRecycable(type); // Attempt to use the item, even if incorrect
            }
        }
        else
        {
            Debug.LogError($"Invalid material: {material}");
        }
    }

    public void SeedComptionHolder()
    {
        //GameManager.Instance.currentScore++;
        //GameManager.Instance.CheckProgress();
        planetManagerScript.CompleteObjective();
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

    public void AddSeedToInventory(GameObject seed)
    {
        if (!seedInventory.Contains(seed))
        {
            seedInventory.Add(seed);
            Debug.Log($"Seed '{seed.name}' added to the seed inventory. Total seeds: {seedInventory.Count}");
        }
        else
        {
            Debug.Log($"Seed '{seed.name}' is already in the inventory.");
        }
    }

    public int GetSeedCount()
    {
        return seedInventory.Count;
    }

    public void RemoveSeedFromInventory()
    {
        if (GetSeedCount() > 0)
        {
            // Get the first seed in the inventory
            GameObject seedToRemove = seedInventory[0];

            // Remove it from the inventory
            seedInventory.RemoveAt(0);

            if(GetSeedCount() == 0)
            {
                FindObjectOfType<DialogBoxController>().SetHasSeed(false);
            }

            Debug.Log($"Seed '{seedToRemove.name}' removed from the seed inventory. Remaining seeds: {seedInventory.Count}");
        }
        else
        {
            Debug.Log("No seeds available in the inventory to remove.");
        }
    }

    public List<GameObject> GetAllSeeds()
    {
        return new List<GameObject>(seedInventory); // Return a copy of the inventory
    }

    public void SetDropOffZone(PlanetManagerScript dropZone)
    {
        planetManagerScript = dropZone;
    }


}