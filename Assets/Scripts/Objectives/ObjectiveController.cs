using UnityEngine;

public class ObjectiveController : MonoBehaviour
{
    public ObjectiveDefinition def;
    public ObjectiveState State { get; private set; } = ObjectiveState.Inactive;
    int stepIndex; float startedAt;

    public void MakeAvailable()
    {
        if (State == ObjectiveState.Inactive)
        {
            State = ObjectiveState.Available;
            ObjectiveEvents.RaiseAvailable(def);  
        }
    }

    public void Begin()
    {
        if (State == ObjectiveState.Available)
        {
            State = ObjectiveState.Active;
            startedAt = Time.time;
            ObjectiveEvents.RaiseStarted(def);
        }
    }

    public void Pause() { if (State == ObjectiveState.Active) { State = ObjectiveState.Paused; ObjectiveEvents.RaisePaused(def); } }
    public void Resume() { if (State == ObjectiveState.Paused) { State = ObjectiveState.Active; ObjectiveEvents.RaiseStarted(def); } }
    public void Abandon() { if (State == ObjectiveState.Active || State == ObjectiveState.Paused) { State = ObjectiveState.Abandoned; ObjectiveEvents.RaiseAbandoned(def); } }

    void Update()
    {
        if (State != ObjectiveState.Active) return;

        var step = def.steps[stepIndex];
        // Aggregate step progress
        float p = 0f; int n = 0; bool allMet = true;
        foreach (var mb in step.conditionProviders)
        {
            if (mb is IObjectiveCondition c)
            {
                p += c.Progress01; n++;
                if (!c.IsMet()) allMet = false;
            }
        }
        float progress01 = n > 0 ? p / n : 1f;
        ObjectiveEvents.RaiseProgress(def, progress01); 

        if (allMet)
        {
            stepIndex++;
            if (stepIndex >= def.steps.Count) Complete();
        }
    }

    void Complete()
    {
        State = ObjectiveState.Completed;
        ObjectiveEvents.RaiseCompleted(def);
        // inform a QuestManager to unlock next objectives, add restoration %
    }
}
