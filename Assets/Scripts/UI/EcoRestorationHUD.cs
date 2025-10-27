using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EcoRestorationHUD : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Image fillImage;                 // child "Fill"
    [SerializeField] private TextMeshProUGUI percentLabel;    // optional % text
    [SerializeField] private Button nextQuestButton;          // optional
    [SerializeField] private Button completeGameButton;       // optional

    [Header("Smoothing")]
    [SerializeField] private float lerpSpeed = 6f;            // 0 = no smoothing

    float _shown; // what the UI currently shows

    void Start()
    {
        _shown = Mathf.Clamp01(GameManager.Instance.GetProgress01());
        if (fillImage) fillImage.fillAmount = _shown;
        RefreshButtons();
        RefreshLabel();
    }

    void Update()
    {
        float target = Mathf.Clamp01(GameManager.Instance.GetProgress01());

        // Smoothly animate fill:
        _shown = (lerpSpeed <= 0f) ? target : Mathf.Lerp(_shown, target, Time.deltaTime * lerpSpeed);

        if (fillImage) fillImage.fillAmount = _shown;

        // Update label occasionally (or every frame if you prefer)
        RefreshLabel();
        RefreshButtons(); // cheap checks; fine to run each frame
        UpdateButtonStates();
    }

    void RefreshLabel()
    {
        if (!percentLabel) return;
        percentLabel.text = $"{(_shown * 100f):0}%";
    }

    void RefreshButtons()
    {
        var gm = GameManager.Instance;
        if (nextQuestButton) nextQuestButton.interactable = gm.CanAdvanceQuest();
        if (completeGameButton) completeGameButton.interactable = gm.CanCompleteGame();
    }

    private void UpdateButtonStates()
    {
        var gm = GameManager.Instance;

        // === Next Quest button ===
        if (nextQuestButton)
        {
            bool canAdvance = gm.CanAdvanceQuest();
            nextQuestButton.interactable = canAdvance;

            ColorBlock colors = nextQuestButton.colors;
            colors.normalColor = canAdvance ? Color.white : Color.gray;
            nextQuestButton.colors = colors;
        }

        // === Complete Game button ===
        if (completeGameButton)
        {
            bool canComplete = gm.CanCompleteGame();
            Debug.Log("Completed the game? " + canComplete);
            completeGameButton.interactable = canComplete;

            ColorBlock colors = completeGameButton.colors;
            colors.normalColor = canComplete ? Color.white : Color.gray;
            completeGameButton.colors = colors;
        }
    }


    // Hook these to your buttons if you like:
    public void OnClickNextQuest() => GameManager.Instance.TryAdvanceQuest();
    public void OnClickCompleteGame() => GameManager.Instance.OnCompleteGamePressed();
}
