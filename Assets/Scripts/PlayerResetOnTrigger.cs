// 9/30/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class PlayerResetOnTrigger : MonoBehaviour
{
    [SerializeField] private Transform player; // Reference to the player's Transform
    [SerializeField] private Vector3 _startingPosition;
    [SerializeField] Quaternion _startingRotation;
    [SerializeField] private AudioSource sfx;
    [SerializeField] private AudioClip wrongClip;
    [SerializeField] private GameObject wrongVFX;


    private void OnTriggerEnter(Collider other)
    {
        // Check if the player entered the trigger
        if (other.transform == player)
        {
            ResetPlayerToStart();
        }
    }

    /// <summary>
    /// Resets the player to their starting position and rotation.
    /// </summary>
    private void ResetPlayerToStart()
    {
        if (player != null)
        {

            player.position = _startingPosition;
            player.rotation = _startingRotation;
            if (sfx) { sfx.PlayOneShot(wrongClip); }
            if (wrongVFX)
            {
                wrongVFX.transform.position = _startingPosition;
                wrongVFX.transform.rotation = _startingRotation;
                wrongVFX.SetActive(true);
                StartCoroutine(DisableAfterSeconds(wrongVFX, 2f)); // disables after 2 seconds
            }
            Debug.Log("Player has been reset to the starting position.");
        }
    }


    private IEnumerator DisableAfterSeconds(GameObject vfx, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (vfx != null)
            vfx.SetActive(false);
    }
}
