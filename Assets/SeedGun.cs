using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeThrow : MonoBehaviour
{
    //Objects
    [Header("Objects")]
    public Transform cam;
    public Transform attackPoint;
    public GameObject seed;
    public Transform player;

    //Limits
    [Header("Limits")]
    public int totalThrows;
    public float throwCooldown;

    //Throw Variables
    [Header("Throw Variables")]
    public KeyCode throwKey = KeyCode.Mouse0;
    public float throwForce;
    public float throwUpwardForce;

    bool readyToThrow;

    /*//Audio
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip pop;

    //Cam Shake
    [Header("Cam Shake")]
    public CameraShake shakingScript;
    public float shakeDuration;
    public float shakeIntensity;

    [Header("Particles")]
    ParticleSystem fire;*/

    // Start is called before the first frame update
    void Start()
    {
        readyToThrow = true;
        //fire = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(throwKey) && readyToThrow && totalThrows > 0 && transform.parent != null)
        {
            Throw();
        }
    }

    private void Throw()
    {
        readyToThrow = false;

        //spawn object
        GameObject cube = Instantiate(seed, attackPoint.position, cam.rotation);

        //get Rigidbody
        Rigidbody projectileRb = cube.GetComponent<Rigidbody>();

        //calculate direction
        Vector3 forceDirection = attackPoint.transform.forward;

        /*RaycastHit hit;

        if (Physics.Raycast(player.position, cam.forward, out hit, 500f))
        {
            forceDirection = (hit.point - attackPoint.position).normalized;
        }*/

        //shakingScript.ShakeCamera(shakeDuration, shakeIntensity);
        //audioSource.PlayOneShot(pop);
        //fire.Play();

        //force to add
        Vector3 forceToAdd = forceDirection * throwForce + transform.up * throwUpwardForce;

        projectileRb.AddForce(forceToAdd, ForceMode.Impulse);

        totalThrows--;

        Invoke(nameof(ResetThrow), throwCooldown);
    }

    private void ResetThrow()
    {
        readyToThrow = true;
    }
}
