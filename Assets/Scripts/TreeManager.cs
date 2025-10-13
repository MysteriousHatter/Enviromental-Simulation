using UnityEngine;
using UnityEngine.VFX;

public class TreeManager : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private GameObject[] trees;               // Tree GameObjects with a VisualEffect child
    [SerializeField] private string leafSizeProperty = "Leaves Size";

    [Header("Leaf Size Range")]
    [SerializeField] private float minLeafSize = 0f;           // size when env = 0
    [SerializeField] private float maxLeafSize = 5f;           // size when env = 1

    [Header("Progress (smooth like SkyboxManager)")]
    [SerializeField] private float transitionSpeed = 1f;       // how fast currentProgress chases targetProgress
    private float currentProgress = 0f;                        // 0..1 (what we apply this frame)
    private float targetProgress = 0f;                         // 0..1 (what GameManager sets)

    // cache
    private VisualEffect[] leafVFX;

    private void Awake()
    {
        if (trees == null || trees.Length == 0)
        {
            Debug.LogError("[TreeManager] No trees assigned.");
            return;
        }

        leafVFX = new VisualEffect[trees.Length];
        for (int i = 0; i < trees.Length; i++)
        {
            if (trees[i] == null) continue;
            leafVFX[i] = trees[i].GetComponentInChildren<VisualEffect>(true);
            if (leafVFX[i] == null)
            {
                Debug.LogWarning($"[TreeManager] No VisualEffect found under tree index {i} ({trees[i].name}).");
            }
        }

        // initialize leaf size at the deforested state
        ApplyLeafSize(minLeafSize);
    }

    private void Update()
    {
        // Smoothly move toward the requested progress (same idea as SkyboxManager)
        if (!Mathf.Approximately(currentProgress, targetProgress))
        {
            currentProgress = Mathf.MoveTowards(
                currentProgress,
                targetProgress,
                transitionSpeed * Time.deltaTime
            );

            float leafSize = Mathf.Lerp(minLeafSize, maxLeafSize, currentProgress);
            ApplyLeafSize(leafSize);
        }
    }

    private void ApplyLeafSize(float size)
    {
        if (leafVFX == null) return;
        for (int i = 0; i < leafVFX.Length; i++)
        {
            if (leafVFX[i] == null) continue;
            leafVFX[i].SetFloat(leafSizeProperty, size);
        }
    }

    /// <summary>
    /// Called by GameManager when overall progress changes, just like SkyboxManager.SetProgress.
    /// </summary>
    public void SetProgress(float progress01)
    {
        targetProgress = Mathf.Clamp01(progress01);
    }

    /// <summary>
    /// Optional: force an immediate refresh (e.g., after scene load).
    /// </summary>
    public void ForceApply()
    {
        currentProgress = targetProgress;
        float leafSize = Mathf.Lerp(minLeafSize, maxLeafSize, currentProgress);
        ApplyLeafSize(leafSize);
    }
}
