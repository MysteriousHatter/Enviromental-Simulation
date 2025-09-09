using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static RecyclableItem;
using UnityEngine.EventSystems;
using System;

public class ItemSlot : MonoBehaviour
{
    //=====ITEM DATA======//
    public string itemName;
    public int quantity;
    public Sprite itemSprite;
    public bool isFull;
    public string itemDescription;
    public Sprite emptySprite;

    //======PHOTO DATA====//
    private AnimationClip SpeicesClip;
    private int animalRarity;
    public string animalTag;
    private bool isPhoto;

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
        this.isPhoto = false;

        return 0;

    }
    public void AddPhotoItem(PhotoData data)
    {
        // Mark as filled
        isFull = true;
        quantity = 1;

        // Display in UI
        itemImage.sprite = data.photoTaken.sprite;
        itemImage.enabled = true;
        itemName = data.speciesName;

        // Update description text if you’re showing hover/tooltip
        this.itemDescription = data.Description;
        this.itemDescriptionImage.sprite = data.photoTaken.sprite;

        // Private values for Photos
        this.animalRarity = data.rarityLevel;
        this.animalTag = data.speicesTag;
        this.SpeicesClip = data.animationClip;
        this.isPhoto = true;

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
            itemDescriptionImage.sprite = (this.isPhoto)? itemImage.sprite : itemImage.sprite;
            if(itemDescriptionImage.sprite == null)
            {
                itemDescriptionImage.sprite = emptySprite;
            }
        }
        else if((thisItemSelected && this.quantity > 0))
        {
            inventory.OnButtonPress(itemName);
            this.quantity -= 1;
            quantityText.text = this.quantity.ToString();  
            if(this.quantity <= 0)
            {
                EmptySlot();
            }
        }
        else
        {
            EmptySlot();
        }
    }

    private void EmptySlot()
    {
        quantityText.enabled = false;
        this.itemImage.sprite = emptySprite;

        this.itemSprite = emptySprite;
        this.itemName = "";
        this.itemDescription = "";
        this.itemDescriptionImage.sprite = emptySprite;
        ItemDescriptionNameText.text = itemName;
        ItemDescriptionText.text = itemDescription;

    }
}
