using UnityEngine;

[DefaultExecutionOrder(-1000)] // Run early
public class FOVWatcher : MonoBehaviour
{
    private Camera cam;
    private float lastFOV;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
        lastFOV = cam.fieldOfView;
    }

    void LateUpdate()
    {
        if (Mathf.Abs(cam.fieldOfView - lastFOV) > 0.01f)
        {
            Debug.LogWarning($"FOV changed from {lastFOV} to {cam.fieldOfView} by:", this);
            Debug.Log(StackTraceUtility.ExtractStackTrace());
            lastFOV = cam.fieldOfView;
        }
    }
}