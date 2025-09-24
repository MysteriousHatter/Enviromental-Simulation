public enum ObjectiveState { Inactive, Available, Active, Paused, Completed, Failed, Abandoned }

public interface IObjectiveCondition
{
    bool IsMet();                    // e.g., player in zone, item count, timer, etc.
    float Progress01 { get; }        // for UI bars
}

[System.Serializable]
public class ObjectiveProgress
{
    public string objectiveId;
    public ObjectiveState state;
    public float startedAt, completedAt;
    public float progress01;
}