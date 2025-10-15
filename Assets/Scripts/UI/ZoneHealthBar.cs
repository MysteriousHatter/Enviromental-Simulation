using UnityEngine;
using UnityEngine.UI;

public class ZoneHealthBar : MonoBehaviour
{
    [Header("References")]
    public Image fillImage;                 // UI Image set to Filled
    public CanvasGroup canvasGroup;         // optional (for fade); add on the bar root

    [Header("Fill Animation")]
    public bool smooth = true;
    public float smoothSpeed = 6f;

    [Header("Auto Hide (KeepAlive)")]
    [Tooltip("Seconds the bar stays visible since the last KeepAlive() call.")]
    public float keepAliveSeconds = 1.25f;
    public float fadeSpeed = 5f;            // only used if canvasGroup is assigned

    float _targetFill;
    float _visibleUntil = -1f;
    bool _hasResetFill;

    // --- Public API ---

    /// <summary>Set normalized progress (0..1). Call with immediate=true to snap.</summary>
    public void SetProgress01(float p, bool immediate = false)
    {
        _targetFill = Mathf.Clamp01(p);
        if (fillImage && immediate) fillImage.fillAmount = _targetFill;
        // NOTE: showing is controlled by KeepAlive(); SetProgress01 doesn't show by itself.
    }

    /// <summary>Show now and extend the auto-hide timer.</summary>
    public void KeepAlive()
    {
        _visibleUntil = Time.time + keepAliveSeconds; // Time.time is Unity's running clock. 
        // Make visible now
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        if (canvasGroup)
        {
            canvasGroup.alpha = 1f;                 // render
            canvasGroup.interactable = true;        // receive input if needed
            canvasGroup.blocksRaycasts = true;      // block clicks if needed
        }
        _hasResetFill = false;
    }

    /// <summary>Hide immediately (and reset the fill to 0%).</summary>
    public void HideImmediate()
    {
        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        ResetFill();
        gameObject.SetActive(false);
    }

    // --- Legacy helpers (if you still want explicit show/hide) ---
    public void Show() => KeepAlive();
    public void Hide() => HideImmediate();

    // --- Internals ---

    void Update()
    {
        if (!fillImage) return;

        // fill animation
        if (smooth)
            fillImage.fillAmount = Mathf.MoveTowards(  // smooth, frame-rate independent ramp
                fillImage.fillAmount, _targetFill, smoothSpeed * Time.deltaTime);
        else
            fillImage.fillAmount = _targetFill;

        // auto-fade/auto-hide after keepAliveSeconds of inactivity
        if (canvasGroup)
        {
            if (Time.time > _visibleUntil)
            {
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 0f, fadeSpeed * Time.deltaTime);
                if (canvasGroup.alpha <= 0.01f)
                {
                    canvasGroup.alpha = 0f;
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                    if (gameObject.activeSelf) gameObject.SetActive(false);
                    if (!_hasResetFill) ResetFill();
                }
            }
        }
        else
        {
            // no CanvasGroup → just toggle off once the timer has elapsed
            if (Time.time > _visibleUntil && gameObject.activeSelf)
            {
                if (!_hasResetFill) ResetFill();
                gameObject.SetActive(false);
            }
        }
    }

    void ResetFill()
    {
        if (fillImage) fillImage.fillAmount = 0f;
        _targetFill = 0f;
        _hasResetFill = true;
    }
}
