using UnityEngine;

[System.Serializable]
public class RecyclableItem
{
    public enum RecyclableType { Paper, Battery, Glass, Plastic, Reed, Weed, LithiumBattery, Seed, None }
    public RecyclableType type;
    public GameObject prefab;
    [TextArea]public string ItemDescription;
    public Sprite sprite;
    [Header("For Item Objective")]
    [SerializeField] private int ObjectiveIndex = 0;

    public int GetObjectiveIndex() { return ObjectiveIndex; }
}