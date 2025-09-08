using System.Collections;
using UnityEngine;

public class ScannableTarget : MonoBehaviour
{
    [SerializeField] public SpeicesData speciesData;
    private Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            StartCoroutine(PlayRandomAnimation());
        }
    }

    // Call this method to play a random animation
    IEnumerator PlayRandomAnimation()
    {

        while (true)
        {
            yield return new WaitForSeconds(2.0f);
            int randomIndex = Random.Range(0, speciesData.animationClips.Length); // Generates a random index
            animator.Play(speciesData.animationClips[randomIndex].name);
        }


    }

}
