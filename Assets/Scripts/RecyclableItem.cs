using UnityEngine;

[System.Serializable]
public class RecyclableItem
{
    public enum RecyclableType 
    { 
        None,
        Paper, 
        Battery, 
        Glass, 
        Plastic, 
        LithiumBattery,
        Placeholder,
        Rings,
        Frog,
        Reed
    }

    public RecyclableType type;
    public int amount;
    public GameObject prefab;
    public string ItemDescription;
    public Sprite sprite;
}