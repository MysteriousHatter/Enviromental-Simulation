using System;
using System.Collections;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PhotoCapture : MonoBehaviour
{

    [Header("Photo Taker")]
    [SerializeField] private Image photoDisplayArea;
    [SerializeField] private GameObject photoFrame;
    [SerializeField] private GameObject cameraUI;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float minFOV = 15f;
    [SerializeField] private float maxFOV = 90f;

    [Header("Flash Effect")]
    [SerializeField] private GameObject cameraFlash;
    [SerializeField] private float flashTime;

    [Header("Photo Fader Effect")]
    [SerializeField] private Animator FadingAnimation;

    [Header("Audio")]
    [SerializeField] private AudioSource cameraAudio;

    private Texture2D screenCapture;
    private bool viewingPhoto;

    [Header("Inventory")]
    [SerializeField] Inventory inventory;

    private void Start()
    {
        screenCapture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            if (!viewingPhoto)
            {
                StartCoroutine(CapturePhoto());
            }
            else
            {
                RemovePhoto();
            }
        }

        // Get mouse scroll wheel input
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        // Adjust FOV based on scroll input
        mainCamera.fieldOfView -= scroll * zoomSpeed;

        // Clamp FOV within min/max values
        mainCamera.fieldOfView = Mathf.Clamp(mainCamera.fieldOfView, minFOV, maxFOV);
    }

    private void RemovePhoto()
    {
        viewingPhoto = false;
        photoFrame.SetActive(false);
        cameraUI.SetActive(true);
    }

    IEnumerator CapturePhoto()
    {
        //Camera UI set false
        Vector3 center = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f);
        Ray ray = mainCamera.ScreenPointToRay(center);

        if(Physics.Raycast(ray, out RaycastHit hit, 500f))
        {
            var target = hit.collider.GetComponent<ScannableTarget>();
            string name = target.speciesData.Name;
            int rarity = target.speciesData.Rarity;
            string Description = target.speciesData.Description;
            string tag = hit.collider.tag;
            AnimatorClipInfo[] m_CurrentClipInfo = hit.collider.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0);
            AnimationClip currentClip = m_CurrentClipInfo[0].clip;

            cameraUI.SetActive(false);
            viewingPhoto = true;

            yield return new WaitForEndOfFrame();

            Rect regionToRead = new Rect(0, 0, Screen.width, Screen.height);

            screenCapture.ReadPixels(regionToRead, 0, 0, false);
            screenCapture.Apply();
            var record = ShowPhoto(name, rarity, Description, tag, currentClip);
            inventory.AddPhoto(record);

        }
        else
        {

            yield return null;
        }

    }

    PhotoData ShowPhoto(string name, int rarity, string description, string tag, AnimationClip currentClip)
    {

        Sprite photoSprite = Sprite.Create(screenCapture, new Rect(0.0f, 0.0f, screenCapture.width, screenCapture.height), new Vector2(0.5f, 0.5f), 100.0f);
        photoDisplayArea.sprite = photoSprite;

        photoFrame.SetActive(true);

        StartCoroutine(CameraFlashEffect());
        FadingAnimation.Play("PhotoFade");
        return new PhotoData { speciesName = name, rarityLevel = rarity, Description = description, photoTaken = photoDisplayArea, animationClip = currentClip, speicesTag = tag  };
    }

    IEnumerator CameraFlashEffect()
    {
        cameraAudio.Play();
        cameraFlash.SetActive(true);
        yield return new WaitForSeconds(flashTime);
        cameraFlash.SetActive(false);
    }
}
