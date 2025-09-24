using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Objective")]
public class ObjectiveDefinition : ScriptableObject
{
    public string id;
    public string title;
    [TextArea] public string description;
    public float restorationPercent;          // contributes to your 100%
    public ObjectiveDefinition[] prerequisites;
    public List<ObjectiveStep> steps;         // each step has conditions
}

[System.Serializable]
public class ObjectiveStep
{
    public string label;
    public List<MonoBehaviour> conditionProviders; // components implementing IObjectiveCondition
}
