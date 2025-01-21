using UnityEngine;

[System.Serializable]
public class RecyclableItem
{
    public enum RecyclableType { Paper, Battery, Glass, Plastic, LithiumBattery }
    public RecyclableType type;
    public GameObject prefab;
}