using UnityEngine;

[CreateAssetMenu(menuName = "Game/Area", fileName = "Area_Conservatory")]
public class AreaDefinition : ScriptableObject
{
    [Header("Identity")]
    public string areaId = "AREA_Conservatory";

    [Header("Completion")]
    [Range(0, 100)] public float clearThreshold = 80f;

    [Header("Objectives in this Area")]
    public ObjectiveDefinition[] objectives; // Main, Side, Extra
}
