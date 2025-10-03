using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class RecyclableItemComponent : MonoBehaviour
{
    public RecyclableItem recyclableItem;
    [SerializeField] private AudioClip[] proximitySounds = new AudioClip[3]; // 0: Green, 1: Yellow, 2: Red
    [SerializeField] private float greenThreshold = 10f;
    [SerializeField] private float yellowThreshold = 5f;
    [SerializeField] private float redThreshold = 2f;
    [SerializeField] private int quantity;

    public int questIndex; // The index of the quest in the QuestSystem
    public int objectiveIndex; // The index of the objective in the quest

    private bool itemPickedUp = false;
    private AudioSource itemAudioSource => GetComponent<AudioSource>();
    private Transform playerTransform;


    void Awake()
    {
        if (itemAudioSource == null)
        {
            Debug.LogError("AudioSource component missing on CollectibleItem.");
        }

        // Assuming the player has the tag "Player"
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player object not found in the scene.");
        }

        // Subscribe to XRGrabInteractable events if present
        XRGrabInteractable grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    void Update()
    {
        if (!itemPickedUp && playerTransform != null)
        {
            AdjustAudioBasedOnProximity();
        }
    }

    void AdjustAudioBasedOnProximity()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= redThreshold)
        {
            PlayProximitySound(2); // Red
        }
        else if (distanceToPlayer <= yellowThreshold)
        {
            PlayProximitySound(1); // Yellow
        }
        else if (distanceToPlayer <= greenThreshold)
        {
            PlayProximitySound(0); // Green
        }
        else
        {
            StopAudio();
        }
    }


    void PlayProximitySound(int index)
    {
        if (itemAudioSource.clip != proximitySounds[index])
        {
            itemAudioSource.clip = proximitySounds[index];
            itemAudioSource.Play();
        }
    }

    void StopAudio()
    {
        if (itemAudioSource.isPlaying)
        {
            itemAudioSource.Stop();
        }
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        SetItemPicked(true);
        int leftOverItems = playerTransform.GetComponent<Inventory>().AddRecyclable(recyclableItem.type, quantity, recyclableItem.sprite, recyclableItem.ItemDescription, recyclableItem.GetObjectiveIndex());
        Debug.Log("We have these numbers left:" +  leftOverItems);
        if(leftOverItems <= 0) //If there are no leftovers, destory the item
        {
            Debug.Log("Destory Items");
            QuestSystem questSystem = FindObjectOfType<QuestSystem>();
            if (questSystem != null)
            {
                Debug.Log($"Player collected item for Quest {questIndex}, Objective {objectiveIndex}");
                questSystem.CompleteObjective(objectiveIndex);
                if(this.recyclableItem.type == RecyclableItem.RecyclableType.Seed) { FindObjectOfType<DialogBoxController>().SetHasSeed(true); }
                this.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.Log("Stick to limit number");
            quantity = leftOverItems;
        }

    }

    void OnReleased(SelectExitEventArgs args)
    {
        SetItemPicked(false);
    }

    public void SetItemPicked(bool picked)
    {
        itemPickedUp = picked;
        itemAudioSource.enabled = !picked;
        if (picked)
        {
            StopAudio();
        }
    }
}
