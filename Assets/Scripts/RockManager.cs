using UnityEngine;
using System.Collections;

public class RockManager : MonoBehaviour
{
    public GameObject rockPrefab; // Reference to the rock prefab
    public float respawnTime = 3f; // Time in seconds before the rock respawns
    public ParticleSystem respawnEffect; // Particle effect prefab for respawn

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private GameObject currentRock;

    void Start()
    {
        if (rockPrefab != null)
        {
            // Store the initial position and rotation
            initialPosition = rockPrefab.transform.position;
            initialRotation = rockPrefab.transform.rotation;
            currentRock = rockPrefab;

            // Instantiate the initial rock
            //currentRock = Instantiate(rockPrefab, initialPosition, initialRotation);
        }
        else
        {
            Debug.LogError("Rock prefab is not assigned in RockManager.");
        }
    }

    public void StartRespawnCoroutine()
    {
        StartCoroutine(DestroyAndRespawn());
    }

    private IEnumerator DestroyAndRespawn()
    {
        // Wait for the respawn time
        yield return new WaitForSeconds(respawnTime);

        // Reactivate the rock at the initial position and rotation
        currentRock.transform.position = initialPosition;
        currentRock.transform.rotation = initialRotation;
        currentRock.SetActive(true);

        // Trigger respawn particle effect
        if (respawnEffect != null)
        {
            Instantiate(respawnEffect, initialPosition, Quaternion.identity);
        }
    }
}