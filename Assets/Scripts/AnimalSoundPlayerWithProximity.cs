// 10/13/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using UnityEditor;
using UnityEngine;

public class AnimalSoundPlayerWithProximity : MonoBehaviour
{
    public AudioClip animalSound; // Assign the sound clip in the Inspector
    public float interval = 15f; // Time interval in seconds
    public Transform player; // Assign the Player's Transform in the Inspector
    public float detectionRadius = 5f; // Distance threshold for proximity detection

    private AudioSource audioSource;

    private void Start()
    {
        // Add an AudioSource component if not already present
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Assign the audio clip to the AudioSource
        audioSource.clip = animalSound;

        // Start the repeating sound
        InvokeRepeating(nameof(PlayAnimalSound), 0f, interval);
    }

    private void PlayAnimalSound()
    {
        if (animalSound != null && IsPlayerClose())
        {
            audioSource.Play();
        }
        else if (!IsPlayerClose())
        {
            Debug.Log("Player is not close enough to the animal.");
        }
        else
        {
            Debug.LogWarning("No animal sound assigned to the AudioSource.");
        }
    }

    private bool IsPlayerClose()
    {
        if (player == null)
        {
            Debug.LogWarning("Player Transform is not assigned.");
            return false;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        return distance <= detectionRadius;
    }
}
