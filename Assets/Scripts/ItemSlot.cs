using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static RecyclableItem;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour
{
    //=====ITEM DATA======//
    public string itemName;
    public int quantity;
    public Sprite itemSprite;
    public bool isFull;
    public string itemDescription;
    public Sprite emptySprite;

    [SerializeField] private int maxNumberOfItems;

    //====ITEM SLOT======//
    [SerializeField] TMP_Text quantityText;

    [SerializeField] Image itemImage;

    //====ITEM DESCRIPTION====///
    public Image itemDescriptionImage;
    public TMP_Text ItemDescriptionNameText;
    public TMP_Text ItemDescriptionText;

    public GameObject selectedShader;
    public bool thisItemSelected;
    private Inventory inventory => FindAnyObjectByType<Inventory>();

    public int AddItem(RecyclableType type, int amount, Sprite itemSprite, string description)
    {
        //Check to see if the slot is already full
        if (isFull) { return quantity; }

        this.itemSprite = itemSprite;
        this.itemName = type.ToString();

        this.itemDescription = description;

        itemImage.sprite = itemSprite;

        this.quantity += amount;
        if(this.quantity >= maxNumberOfItems)
        {
            quantityText.text = quantity.ToString();
            quantityText.enabled = true;
            isFull = true;

            //Return the LeftOvers
            int extraItems = this.quantity - maxNumberOfItems;
            this.quantity = maxNumberOfItems;
            return extraItems;
        }

        //Update Quantity Text
        quantityText.text = this.quantity.ToString();
        quantityText.enabled = true;

        return 0;

    }


    public void OnItemClicked()
    {
        Debug.Log("An item is selecteed");
        if(!thisItemSelected)
        {
            inventory.DeselectAllSlots();
            selectedShader.SetActive(true);
            thisItemSelected = true;
            ItemDescriptionNameText.text = itemName;
            ItemDescriptionText.text = itemDescription;
            itemDescriptionImage.sprite = itemSprite;
            if(itemDescriptionImage.sprite == null)
            {
                itemDescriptionImage.sprite = emptySprite;
            }
        }
    }
}
