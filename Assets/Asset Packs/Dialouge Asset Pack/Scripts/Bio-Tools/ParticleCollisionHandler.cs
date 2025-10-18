using System;
using UnityEngine;

public class ParticleCollisionHandler : MonoBehaviour
{
    [SerializeField] private QuestCountManager _questCountManager;
    [SerializeField] private GameObject partilceVFX1;
    [SerializeField] private GameObject partilceVFX2;
    [SerializeField] private GameObject partilceVFX3;
    [SerializeField] private GameObject partilceVFX4;
    [SerializeField] private GameObject partilceVFX5;

    private bool isExtinguished = false;
    void OnParticleCollision(GameObject other)
    {
        // 'other' refers to the GameObject that the particle collided with.
        Debug.Log("Particle collided with: " + other.name);
        if(other.CompareTag("Water"))
        {
            ExtinguishFire();
        }

        // You can add specific logic here based on the collided object.
        // For example, if you want to detect collision with a specific tag:
        if (other.CompareTag("Player"))
        {
            Debug.Log("Particle hit the player!");
            // Add your desired actions here, e.g., damage player, play sound, etc.
        }
    }

    private void ExtinguishFire()
    {
        if (isExtinguished) return;

        isExtinguished = true;
        Debug.Log($"{gameObject.name} extinguished!");

        _questCountManager.UnregisterCollectiable(this.gameObject);

        ParticleExplosions();

        Destroy(gameObject, 2f); // Optional: Destroy the fire after 2 seconds
    }

    private void ParticleExplosions()
    {
        var vfx1 = Instantiate(partilceVFX1, transform.position, transform.rotation);
        var ps1 = vfx1.GetComponent<ParticleSystem>();

        var vfx2 = Instantiate(partilceVFX2, transform.position, transform.rotation);
        var ps2 = vfx2.GetComponent<ParticleSystem>();

        var vfx3 = Instantiate(partilceVFX3, transform.position, transform.rotation);
        var ps3 = vfx3.GetComponent<ParticleSystem>();

        var vfx4 = Instantiate(partilceVFX4, transform.position, transform.rotation);
        var ps4 = vfx4.GetComponent<ParticleSystem>();

        var vfx5 = Instantiate(partilceVFX5, transform.position, transform.rotation);
        var ps5 = vfx5.GetComponent<ParticleSystem>();
        if (ps1 && ps2 && ps3 && ps4 && ps5)
        {
            ps1.Play();
            ps2.Play();
            ps3.Play();
            ps4.Play();
            ps5.Play();

        }
    }
}
