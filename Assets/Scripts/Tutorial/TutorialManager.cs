using TMPro;
using UnityEngine;

[System.Serializable]
public class TutorialStep
{
    [TextArea]
    public string tutorialText;

    public GameObject tutorialObject;   // Can be null
    public bool enableLocomotion;
}
public class TutorialManager : MonoBehaviour
{
    [SerializeField] private TutorialStep[] steps;
    [SerializeField] private GameObject locomotionObject;

    private int currentStep = -1;

    public void ShowNextTutorial()
    {
        currentStep++;

        if (currentStep >= steps.Length)
            return;

        TutorialStep step = steps[currentStep];

        // Enable locomotion if needed
        if (step.enableLocomotion && locomotionObject != null)
        {
            locomotionObject.SetActive(true);
        }

        // Enable tutorial object (if any)
        if (step.tutorialObject != null)
        {
            step.tutorialObject.SetActive(true);
        }

        // Update UI
        ShowTutorialText(step.tutorialText);
    }

    private void ShowTutorialText(string text)
    {
        var dialog = DialogBoxController.Instance;

        if (!dialog.liveTutorialText.activeSelf) { dialog.liveTutorialText.SetActive(true); }

        dialog.liveTutorialText.GetComponentInChildren<TextMeshProUGUI>().text = text;
    }

}
