using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip deforestedMusic; // Music for deforested state
    [SerializeField] private AudioClip forestedMusic;   // Music for forested state

    [Header("Audio Settings")]
    [SerializeField] private float transitionSpeed = 0.1f; // Speed of crossfade between tracks

    private AudioSource audioSource1; // First audio source
    private AudioSource audioSource2; // Second audio source
    private float currentProgress = 0f; // Current progress (0 = deforested, 1 = forested)
    private float targetProgress = 0f;  // Target progress to interpolate towards

    private void Start()
    {
        // Initialize audio sources
        audioSource1 = gameObject.AddComponent<AudioSource>();
        audioSource2 = gameObject.AddComponent<AudioSource>();

        audioSource1.clip = deforestedMusic;
        audioSource2.clip = forestedMusic;

        audioSource1.loop = true;
        audioSource2.loop = true;

        audioSource1.volume = 1f; // Start with deforested music playing
        audioSource2.volume = 0f;

        audioSource1.Play();
        audioSource2.Play();
    }

    private void Update()
    {
        // Gradually move current progress toward the target progress
        currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, transitionSpeed * Time.deltaTime);

        // Update the music crossfade based on the current progress
        UpdateMusic(currentProgress);
    }

    public void SetProgress(float progress)
    {
        // Update the target progress (e.g., called by GameManager or EnvironmentManager)
        targetProgress = Mathf.Clamp01(progress);
    }

    private void UpdateMusic(float progress)
    {
        // Adjust the volume of each audio source based on progress
        audioSource1.volume = Mathf.Lerp(1f, 0f, progress); // Deforested music fades out
        audioSource2.volume = Mathf.Lerp(0f, 1f, progress); // Forested music fades in
    }
}