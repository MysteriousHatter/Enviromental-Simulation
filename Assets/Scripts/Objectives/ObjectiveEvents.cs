using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public static class ObjectiveEvents
{
    public static event System.Action<ObjectiveDefinition> OnAvailable;
    public static event System.Action<ObjectiveDefinition> OnStarted;
    public static event System.Action<ObjectiveDefinition, float> OnProgress; // 0..1
    public static event System.Action<ObjectiveDefinition> OnPaused;
    public static event System.Action<ObjectiveDefinition> OnCompleted;
    public static event System.Action<ObjectiveDefinition> OnFailed;
    public static event System.Action<ObjectiveDefinition> OnAbandoned;

    // Raiser methods (one per event)
    public static void RaiseAvailable(ObjectiveDefinition def) => OnAvailable?.Invoke(def);
    public static void RaiseStarted(ObjectiveDefinition def) => OnStarted?.Invoke(def);
    public static void RaiseProgress(ObjectiveDefinition def, float p01) => OnProgress?.Invoke(def, p01);
    public static void RaisePaused(ObjectiveDefinition def) => OnPaused?.Invoke(def);
    public static void RaiseCompleted(ObjectiveDefinition def) => OnCompleted?.Invoke(def);
    public static void RaiseFailed(ObjectiveDefinition def) => OnFailed?.Invoke(def);
    public static void RaiseAbandoned(ObjectiveDefinition def) => OnAbandoned?.Invoke(def);
}
