using RPG.Quests;
using System.Collections.Generic;
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
    // Cooldown timer to ensure sounds are played every n seconds
    private float soundCooldownTimer = 0f;
    private const float soundCooldownDuration = 15f;
    [SerializeField] private int quantity;

    public int questIndex; // The index of the quest in the QuestSystem
    public int objectiveIndex; // The index of the objective in the quest

    private bool itemPickedUp = false;
    private AudioSource itemAudioSource => GetComponent<AudioSource>();
    private Transform playerTransform;
    [SerializeField] private Inventory inventory;
    [SerializeField] private QuestCountManager _questCountManager;

    [Header("Quest System")]
    [SerializeField] Quest quest;
    [SerializeField] string objective;
    [SerializeField] private bool isQuest;


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
            if (this.gameObject.GetComponent<AudioSource>() != null) { AdjustAudioBasedOnProximity(); }
        }
    }

    void AdjustAudioBasedOnProximity()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= redThreshold)
        {
            PlayProximitySoundWithCooldown(0);
        }
        else
        {
            StopAudio();
        }
    }


    void PlayProximitySoundWithCooldown(int index)
    {
        // Check if the cooldown timer has elapsed
        if (Time.time >= soundCooldownTimer)
        {
            if (itemAudioSource.clip != proximitySounds[index])
            {
                itemAudioSource.clip = proximitySounds[index];
                itemAudioSource.Play();
            }

            // Reset the cooldown timer
            soundCooldownTimer = Time.time + soundCooldownDuration;
        }
    }

    void StopAudio()
    {
        if (itemAudioSource.isPlaying)
        {
            itemAudioSource.Stop();
        }
    }
    public void CompleteObjective()
    {
        QuestList questList = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
        questList.CompleteObjective(quest, objective);
    }


    void OnGrabbed(SelectEnterEventArgs args)
    {
        SetItemPicked(true);
        if (this.recyclableItem.type == RecyclableItem.RecyclableType.Seed) 
        {
            if (objectiveIndex == 1)
            {
                Debug.Log("Grow Bushes");

                if (isQuest) { GameManager.Instance.QuestManager.GiveNextQuest();}
                else { CompleteObjective(); }

                FindFirstObjectByType<SubmergeRocks>().StartSubmergeAll();
                FindFirstObjectByType<WeedManager>().ClearAllWeeds();
            }
            // Add the seed to the seed inventory
            FindFirstObjectByType<DialogBoxController>().SetHasSeed(true);
            inventory.AddSeedToInventory(this.gameObject);
            this.gameObject.SetActive(false);
        }
        else 
        { 
            int leftOverItems = playerTransform.GetComponent<Inventory>().AddRecyclable(recyclableItem.type, quantity, recyclableItem.sprite, recyclableItem.ItemDescription, recyclableItem.GetObjectiveIndex()); 
            Debug.Log("We have these numbers left:" + leftOverItems);
            if (leftOverItems <= 0) //If there are no leftovers, destory the item
            {
                Debug.Log("Destory Items");
                if(_questCountManager != null) { _questCountManager.UnregisterCollectiable(this.gameObject); }
                this.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("Stick to limit number");
                quantity = leftOverItems;
            }
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
