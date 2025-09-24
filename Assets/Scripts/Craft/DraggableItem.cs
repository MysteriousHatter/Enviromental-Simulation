using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image image;
    public string itemName;
    public int quantity = 1;
    public string itemDescription;

    public int maxNumberOfItems;
    public bool isFull;

    public TMP_Text quantityText;
    [HideInInspector] public Transform parentAfterDrag;
    private RectTransform m_DraggingPlane;

    public void AddItem(string name, int amount, Image itemSprite, string description)
    {
        this.image = itemSprite;
        this.itemName = name;

        if (this.itemName == "empty")
            itemName = "";

        this.itemDescription = description;

        RefreshCount();
    }

    public void RefreshCount()
    {
        quantityText.text = quantity.ToString();
    }

    private void Awake()
    {
        m_DraggingPlane = GameObject.Find("New Inventory Canvas").GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Begin drag");
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.parent.parent.parent);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        SetDraggedPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("end drag");
        transform.SetParent(parentAfterDrag);
        image.raycastTarget = true;
    }

    private void SetDraggedPosition(PointerEventData data)
    {
        if (data.pointerEnter != null && data.pointerEnter.transform as RectTransform != null)
            m_DraggingPlane = data.pointerEnter.transform as RectTransform;
        var rt = gameObject.GetComponent<RectTransform>();
        Vector3 globalMousePos;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_DraggingPlane, data.position, data.pressEventCamera, out globalMousePos))
        {
            rt.position = new Vector3(globalMousePos.x, globalMousePos.y, transform.position.z);
            rt.rotation = m_DraggingPlane.rotation;
        }
    }
}
