using UnityEngine;

public class TutorialTriggers : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            this.gameObject.SetActive(false);
            GameManager.Instance.tutorialManager.ShowNextTutorial();

        }
    }
}
