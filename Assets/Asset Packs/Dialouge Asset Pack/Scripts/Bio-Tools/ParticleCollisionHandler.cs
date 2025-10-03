using UnityEngine;

public class ParticleCollisionHandler : MonoBehaviour
{
    void OnParticleCollision(GameObject other)
    {
        // 'other' refers to the GameObject that the particle collided with.
        Debug.Log("Particle collided with: " + other.name);

        // You can add specific logic here based on the collided object.
        // For example, if you want to detect collision with a specific tag:
        if (other.CompareTag("Player"))
        {
            Debug.Log("Particle hit the player!");
            // Add your desired actions here, e.g., damage player, play sound, etc.
        }
    }
}
