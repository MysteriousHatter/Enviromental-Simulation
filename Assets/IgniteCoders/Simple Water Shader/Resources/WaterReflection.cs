using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class WaterReflection : MonoBehaviour
{
    [System.Serializable]
    public class ReflectionTarget
    {
        [Tooltip("The plane for this reflection (e.g., water surface).")]
        public Transform reflectionPlane;

        [Tooltip("The RenderTexture the water shader will sample for this plane.")]
        public RenderTexture outputTexture;

        [Tooltip("Optional vertical offset (y). If 0, uses the global offset.")]
        public float verticalOffsetOverride = 0f;
    }

    // references
    private Camera mainCamera;
    private Camera reflectionCamera;

    [Header("Targets")]
    [Tooltip("Add all water planes you want to render reflections for, each with its own output texture.")]
    public List<ReflectionTarget> targets = new List<ReflectionTarget>();

    [Header("Parameters")]
    [Tooltip("Copy main camera parameters before rendering (FOV, clip planes, etc.).")]
    public bool copyCameraParameters = true;

    [Tooltip("Global vertical offset added to the mirrored camera (used when per-target override is 0).")]
    public float verticalOffset = 0f;

    [Tooltip("If true, copies main camera parameters every frame; if false, copies once on Awake/Validate.")]
    public bool syncParamsEveryFrame = true;

    // cache
    private Transform mainCamTransform;
    private Transform reflectionCamTransform;
    private bool isReady;

    private void Awake()
    {
        mainCamera = Camera.main;
        reflectionCamera = GetComponent<Camera>();

        // Render manually; don't let this camera draw to screen
        if (reflectionCamera != null) reflectionCamera.enabled = false;

        Validate();
    }

    private void OnEnable() => Validate();
    private void OnValidate() => Validate();

    private void Update()
    {
        if (!isReady || targets == null || targets.Count == 0) return;

        if (syncParamsEveryFrame && copyCameraParameters && mainCamera && reflectionCamera)
        {
            CopyParamsFromMain();
        }

        // Render each target in sequence
        for (int i = 0; i < targets.Count; i++)
        {
            var t = targets[i];
            if (t == null || t.reflectionPlane == null || t.outputTexture == null)
                continue;

            RenderReflectionFor(t);
        }
    }

    private void RenderReflectionFor(ReflectionTarget t)
    {
        // 1) Main camera basis (world space)
        Vector3 camDirWS = mainCamTransform.forward;
        Vector3 camUpWS = mainCamTransform.up;
        Vector3 camPosWS = mainCamTransform.position;

        // per-target vertical offset (fallback to global)
        float yOffset = t.verticalOffsetOverride != 0f ? t.verticalOffsetOverride : verticalOffset;
        camPosWS.y += yOffset;

        // 2) Into plane local space
        Transform plane = t.reflectionPlane;
        Vector3 camDirPS = plane.InverseTransformDirection(camDirWS);
        Vector3 camUpPS = plane.InverseTransformDirection(camUpWS);
        Vector3 camPosPS = plane.InverseTransformPoint(camPosWS);

        // 3) Mirror across plane (invert Y in plane space)
        camDirPS.y *= -1f;
        camUpPS.y *= -1f;
        camPosPS.y *= -1f;

        // 4) Back to world space
        camDirWS = plane.TransformDirection(camDirPS);
        camUpWS = plane.TransformDirection(camUpPS);
        camPosWS = plane.TransformPoint(camPosPS);

        // 5) Apply to reflection camera and render
        reflectionCamTransform.position = camPosWS;
        reflectionCamTransform.rotation = Quaternion.LookRotation(camDirWS, camUpWS);

        reflectionCamera.targetTexture = t.outputTexture;
        reflectionCamera.Render();
    }

    private void Validate()
    {
        isReady = false;

        if (mainCamera != null)
        {
            mainCamTransform = mainCamera.transform;
        }
        else
        {
            mainCamera = Camera.main;
            mainCamTransform = mainCamera ? mainCamera.transform : null;
        }

        if (reflectionCamera == null)
        {
            reflectionCamera = GetComponent<Camera>();
            if (reflectionCamera == null)
            {
                Debug.LogError("[WaterReflection] Missing Camera component on this GameObject.");
                return;
            }
        }

        reflectionCamTransform = reflectionCamera.transform;

        // Copy once if not syncing every frame
        if (!syncParamsEveryFrame && copyCameraParameters && mainCamera && reflectionCamera)
        {
            CopyParamsFromMain();
        }

        // Safety: turn off on-screen rendering, we render manually per target
        reflectionCamera.enabled = false;

        isReady = (mainCamTransform != null) && (reflectionCamTransform != null);
    }

    private void CopyParamsFromMain()
    {
        // CopyFrom can also copy targetTexture, so set it AFTER each render
        reflectionCamera.CopyFrom(mainCamera);
        // Ensure it's off-screen
        reflectionCamera.enabled = false;
    }
}
