using UnityEngine;

[System.Serializable]
public class RecyclableItem
{
    public enum RecyclableType 
    { 
        Paper, 
        Battery, 
        Glass, 
        Plastic, 
        LithiumBattery,
        Rings,
        Reed,
        Photo
    }

    public RecyclableType type;
    public int amount;
    public GameObject prefab;
    public string ItemDescription;
    public Sprite sprite;
}