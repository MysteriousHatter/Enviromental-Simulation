using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BioTools;

public class BioToolButtonController : MonoBehaviour
{
    [SerializeField] private int ID;
    [SerializeField] private string itemName;
    [SerializeField] private TextMeshProUGUI itemText;
    [SerializeField] private Image selectedItem;
    [SerializeField] private Sprite icon;
    [SerializeField] private GameObject tool;

    private Animator anim;
    private bool selected = false;
    public bool selectedWeapon;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (selected)
        {
            selectedItem.sprite = icon;
            itemText.text = itemName;
        }
    }

    public int getToolID()
    {
        return ID;
    }

    public void Selected()
    {
        selected = true;
    }

    public void Deselected()
    {
        selected = false;
    }
    public void HoverEnter()
    {
        anim.SetBool("Hover", true);
        Debug.Log($"HoverEnter called. itemName: {itemName}, itemText: {itemText}");
        itemText.text = itemName;
    }
    public void HoverExit()
    {
        anim.SetBool("Hover", false);
        itemText.text = "";
    }

    public GameObject getBioTool()
    {
        return tool;
    }

    public bool GetSelected()
    {
        return selectedWeapon;
    }

    public void setSelected(bool value)
    {
        selectedWeapon = value;
    }
}
