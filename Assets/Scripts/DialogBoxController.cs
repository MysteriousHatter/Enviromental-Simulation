using NUnit.Framework;
using RPG.Quests;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.UI;
using static RecyclableItem;

public class DialogBoxController : MonoBehaviour
{
    [SerializeField] private GameObject PlayerLogUI; // Assign your World Space Canvas here
    [SerializeField] private GameObject InventoryMenuUI;
    [SerializeField] private GameObject SeedUI;
    [SerializeField] private GameObject QuestUI;
    [SerializeField] private GameObject cameraInventory;
    [SerializeField] private GameObject ToolWheel;
    [SerializeField] private GameObject ShowPhotoUI;
    [SerializeField] private GameObject cameraMode;
    [SerializeField] private GameObject timer;
    [SerializeField] private float distanceFromCamera = 2.0f; // Distance in meters
    [SerializeField] private float horizontalSpacing = 1.5f; // Space between canvases
    [SerializeField] private InputActionReference InventoryButton;
    [SerializeField] private InputActionManager inputActionManager; // Drag your Input Action Manager here
    [SerializeField] private GameObject leftController;
    [SerializeField] private GameObject rightController;
    [SerializeField] private GameObject WinUI;
    [SerializeField] private GameObject WinUI2;
    [SerializeField] private RecyclableSpawner recyclableSpawner;
    [SerializeField] private WeaponEquipManager weaponEquipManager;
    [SerializeField] private GameObject AmmoUI;
    [SerializeField] private GameObject EcoRestorationHUD;
    [SerializeField] private GameObject seedCongratulationsUI;
    public ZoneHealthBar healthUI;

    [Header("UI For Seed Controller")]
    [SerializeField] private Button yesButton; // "Yes" button
    [SerializeField] private InputActionReference toggleSeedUIAction; // Input action for toggling SeedUI

    [Header("UI For Restoration Controller")]
    [SerializeField] private InputActionReference toggleQuestUI;

    private bool isSeedUIVisible = false; // Tracks if the SeedUI is currently visible
    private bool hasSeed = false; // Tracks if the player has a seed
    private bool isResorationUIVisible = false;
    private bool isTuorialOpen = false;
    private bool isSeedCongratsOpen = false;

    [Header("Tutorial Board UI")]
    [SerializeField] private GameObject tutorialPanel;        // Panel to show the tutorial text
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private string tutorialBoardTag = "TutorialBoard";
    [SerializeField] private InputActionReference toggleTutorialUIAction;


    private Transform cameraTransform;
    private string defaultLayerMask = "Default";
    private string recycaleLayerMask = "Recycable";
    private string UILayerMask = "UI";
    private string ToolsLayerMask = "Tools";
    private RecyclableType type;

    [SerializeField] private Transform player;
    public Transform[] recycleShopTransforms;
    [SerializeField] private Transform DesktopGiver;
    [SerializeField] private float proximityThreshold = 3.0f; // Distance threshold for being "near

    public ItemSlot[] itemSlot;
    public ItemSlot[] photoSlot;

    TrackedDeviceGraphicRaycaster raycaster => GetComponentInChildren<TrackedDeviceGraphicRaycaster>();
    private bool isDialogVisible = true;
    private bool isInventoryMenuOpen = false; // Tracks if Recycle Shop UI is active
    private bool isPhotoAlbumOpen = false;
    private bool isQuestsWindowOpen = false;
    private bool isMapOpen = false;
    private readonly string[] allowedActionMaps =
    {
        "XRI Head",
        "XRI Left Interaction",
        "XRI Left",
        "XRI Right Interaction",
        "XRI Right",
        "XRI UI"
    };


    private void Awake()
    {


    }
    void Start()
    {
        // Reference to the main camera's transform
        cameraTransform = Camera.main.transform;

        // Initially hide the dialog
        PlayerLogUI.gameObject.SetActive(false);
        InventoryMenuUI.gameObject.SetActive(false);
        cameraMode.gameObject.SetActive(false);
        cameraInventory.gameObject.SetActive(false);
        WinUI.gameObject.SetActive(false);
        WinUI2.gameObject.SetActive(false);
        QuestUI.gameObject.SetActive(false);
        SeedUI.gameObject.SetActive(false);
        ToolWheel.gameObject.SetActive(false);
        AmmoUI.gameObject.SetActive(false);
        EcoRestorationHUD.gameObject.SetActive(false);
        tutorialPanel.gameObject.SetActive(false);
        seedCongratulationsUI.gameObject.SetActive(false);
        //healthUI.gameObject.SetActive(false);   
        isDialogVisible = false;

        InventoryButton.action.performed += OnToggleAction;

        UpdateYesButtonState();

        // Subscribe to the input action
        toggleSeedUIAction.action.performed += OnToggleSeedUI;
        toggleQuestUI.action.performed += OnToggleQuestUI;
        toggleTutorialUIAction.action.performed += CheckNearbyTutorialBoard;
    }

    void OnDestroy()
    {
        // Unsubscribe from the input action event to prevent memory leaks
        InventoryButton.action.performed -= OnToggleAction;
        toggleSeedUIAction.action.performed -= OnToggleSeedUI;
        toggleQuestUI.action.performed -= OnToggleQuestUI;
        toggleTutorialUIAction.action.performed -= CheckNearbyTutorialBoard;
    }

    public bool GetDialogIsVisible()
    {
        return isDialogVisible;
    }

    // Update is called once per frame
    void Update()
    {
        PositionCanvas();
        IsPlayerNear();

        if (isDialogVisible)
        {

            DisableAllExceptAllowed();
        }
        else
        {
            EnableAllActionMaps();
        }

        //REDO Will refactor later with with Inventory panel fro panel switching.
        if (Input.GetKeyDown(KeyCode.P))
        {
            cameraInventory.SetActive(true);
        }

        // Automatically hide the SeedUI if the player moves away
        if (!IsPlayerNear() && isSeedUIVisible)
        {
            ToggleSeedUI(false);
        }

        if (!FindClosestTutorialBoard() && isTuorialOpen)
        {
            OpenTutorialPanel(false);
        }
    }

    private void PositionCanvas()
    {
        // Position ReadyToRecycle UI directly in front of the camera
        Vector3 basePosition = cameraTransform.position + cameraTransform.forward * distanceFromCamera;

        // Position Ready to Recycle
        PlayerLogUI.transform.position = basePosition;
        PlayerLogUI.transform.rotation = Quaternion.LookRotation(PlayerLogUI.transform.position - cameraTransform.position);

        // Position Recycle Shop
        InventoryMenuUI.transform.position = basePosition;
        InventoryMenuUI.transform.rotation = Quaternion.LookRotation(InventoryMenuUI.transform.position - cameraTransform.position);

        cameraInventory.transform.position = basePosition;
        cameraInventory.transform.rotation = Quaternion.LookRotation(cameraInventory.transform.position - cameraTransform.position);

        ToolWheel.transform.position = basePosition;
        ToolWheel.transform.rotation = Quaternion.LookRotation(ToolWheel.transform.position - cameraTransform.position);

        //Postion Camera
        cameraMode.transform.position = basePosition;
        cameraMode.transform.rotation = Quaternion.LookRotation(cameraMode.transform.position - cameraTransform.position);

        ShowPhotoUI.transform.position = basePosition;
        ShowPhotoUI.transform.rotation = Quaternion.LookRotation(ShowPhotoUI.transform.position - cameraTransform.position);

        WinUI.transform.position = basePosition;
        WinUI.transform.rotation = Quaternion.LookRotation(WinUI.transform.position - cameraTransform.position);

        WinUI2.transform.position = basePosition;
        WinUI2.transform.rotation = Quaternion.LookRotation(WinUI2.transform.position - cameraTransform.position);

        SeedUI.transform.position = basePosition;
        SeedUI.transform.rotation = Quaternion.LookRotation(SeedUI.transform.position - cameraTransform.position);

        timer.transform.position = basePosition;
        timer.transform.rotation = Quaternion.LookRotation(timer.transform.position - cameraTransform.position);

        QuestUI.transform.position = basePosition;
        QuestUI.transform.rotation = Quaternion.LookRotation(QuestUI.transform.position - cameraTransform.position);

        AmmoUI.transform.position = basePosition;
        AmmoUI.transform.rotation = Quaternion.LookRotation(AmmoUI.transform.position - cameraTransform.position);

        healthUI.transform.position = basePosition;
        healthUI.transform.rotation = Quaternion.LookRotation(healthUI.transform.position - cameraTransform.position);

        EcoRestorationHUD.transform.position = basePosition;
        EcoRestorationHUD.transform.rotation = Quaternion.LookRotation(EcoRestorationHUD.transform.position - cameraTransform.position);

        tutorialPanel.transform.position = basePosition;
        tutorialPanel.transform.rotation = Quaternion.LookRotation(tutorialPanel.transform.position - cameraTransform.position);

        seedCongratulationsUI.transform.position = basePosition;
        seedCongratulationsUI.transform.rotation = Quaternion.LookRotation(seedCongratulationsUI.transform.position - cameraTransform.position);
    }

    public bool IsPlayerNear()
    {
        foreach (Transform shop in recycleShopTransforms)
        {
            float distance = Vector3.Distance(player.position, shop.position);
            Debug.Log($"Distance to {shop.name}: {distance}");

            if (distance <= proximityThreshold)
            {
                player.gameObject.GetComponent<Inventory>().SetDropOffZone(shop.GetComponent<PlanetManagerScript>());
                return true; // Player is near at least one shop
            }
        }

        return false; // Player is not near any shop
    }

    private bool FindClosestTutorialBoard()
    {
        // Find all tutorial boards with the correct tag
        GameObject[] tutorialBoards = GameObject.FindGameObjectsWithTag("TutorialBoard");

        foreach (GameObject board in tutorialBoards)
        {
            float distance = Vector3.Distance(player.position, board.transform.position);
            Debug.Log($"Distance to {board.name}: {distance}");

            // If the player is within proximity range
            if (distance <= proximityThreshold)
            {
                player.gameObject.GetComponent<Inventory>().SetTuroitalBoard(board.GetComponent<TutorialBoard>());
                return true; // Player is near at least one tutorial board
            }
        }
        return false; // Player is not near any tutorial boards
    }

    /// <summary>
    /// Checks whether the player is close enough to the DesktopGiver terminal.
    /// </summary>
    public bool IsPlayerNearTerminal()
    {
        if (DesktopGiver == null || player == null)
        {
            Debug.LogWarning("DesktopGiver or Player not assigned.");
            return false;
        }

        float distance = Vector3.Distance(player.position, DesktopGiver.position);
        bool isNear = distance <= proximityThreshold;

        // Optional: Debug visualization
        Debug.DrawLine(player.position, DesktopGiver.position, isNear ? Color.green : Color.red);

        return isNear;
    }

    public void DisableAllExceptAllowed()
    {
        foreach (var asset in inputActionManager.actionAssets)
        {
            foreach (var actionMap in asset.actionMaps)
            {
                if (IsActionMapAllowed(actionMap.name))
                {
                    actionMap.Enable();
                }
                else
                {
                    actionMap.Disable();
                }
            }
        }

    }

    public void EnableAllActionMaps()
    {
        foreach (var asset in inputActionManager.actionAssets)
        {
            foreach (var actionMap in asset.actionMaps)
            {
                actionMap.Enable();
                Debug.Log($"Enabled Action Map: {actionMap.name}");
            }
        }
        Debug.Log("All action maps have been enabled.");
    }

    private bool IsActionMapAllowed(string actionMapName)
    {
        foreach (var allowedMap in allowedActionMaps)
        {
            if (actionMapName == allowedMap)
                return true;
        }
        return false;
    }

    public void ShowDialog()
    {
        // Position the canvas in front of the camera
        Vector3 newPosition = cameraTransform.position + cameraTransform.forward * distanceFromCamera;
        PlayerLogUI.transform.position = newPosition;
        InteractionLayerMask UIlayerMask = InteractionLayerMask.GetMask(UILayerMask);
        InteractionLayerMask defaultlayerMask = InteractionLayerMask.GetMask(defaultLayerMask);
        InteractionLayerMask recycalelayerMask = InteractionLayerMask.GetMask(recycaleLayerMask);

        // Align the canvas to face the camera
        PlayerLogUI.transform.rotation = Quaternion.LookRotation(PlayerLogUI.transform.position - cameraTransform.position);

        // Show the dialog
        PlayerLogUI.gameObject.SetActive(true);

        leftController.GetComponentInChildren<NearFarInteractor>().interactionLayers = UIlayerMask;
        rightController.GetComponentInChildren<NearFarInteractor>().interactionLayers = UIlayerMask;

        leftController.GetComponentInChildren<NearFarInteractor>().interactionLayers &= ~defaultlayerMask;
        rightController.GetComponentInChildren<NearFarInteractor>().interactionLayers &= ~defaultlayerMask;

        leftController.GetComponentInChildren<NearFarInteractor>().interactionLayers &= ~recycalelayerMask;
        rightController.GetComponentInChildren<NearFarInteractor>().interactionLayers &= ~recycalelayerMask;
        weaponEquipManager.SetToolActive(false);
        //raycaster.blockingMask = 1 << 5;
    }

    public void HideDialog()
    {
        Debug.Log("Add LayerMasks back");
        // Hide the dialog
        PlayerLogUI.gameObject.SetActive(false);
        InteractionLayerMask UIlayerMask = InteractionLayerMask.GetMask(UILayerMask);

        leftController.GetComponentInChildren<NearFarInteractor>().interactionLayers = InteractionLayerMask.GetMask(defaultLayerMask, recycaleLayerMask, ToolsLayerMask);
        rightController.GetComponentInChildren<NearFarInteractor>().interactionLayers = InteractionLayerMask.GetMask(defaultLayerMask, recycaleLayerMask, ToolsLayerMask);

        leftController.GetComponentInChildren<NearFarInteractor>().interactionLayers &= ~UIlayerMask;
        rightController.GetComponentInChildren<NearFarInteractor>().interactionLayers &= ~UIlayerMask;
        weaponEquipManager.SetToolActive(true);
        //raycaster.blockingMask = 0;
        //raycaster.blockingMask = 1;
    }

    /// <summary>
    /// Handle Input Toggle Logic for Menus
    /// </summary>
    private void OnToggleAction(InputAction.CallbackContext context)
    {
        if (isInventoryMenuOpen || isPhotoAlbumOpen || isQuestsWindowOpen || isMapOpen || isSeedCongratsOpen)
        {
            // If Recycle Shop is open, close it and return to Ready to Recycle
            CloseInventoryMenu();
            ClosePhotoAlbum();
            CloseQuestsWindow();
            CloseMapWindow();
            CloseQuestsWindow();


        }
        else if (isDialogVisible)
        {
            // If Ready to Recycle is open, close it
            ClosePlayerLog();
        }
        else if (!GameManager.Instance.gameIsWon)
        {
            // If no menus are open, open Ready to Recycle
            OpenPlayerLog();
        }

        Debug.Log($"Toggle Action: isRecycleShopOpen={isInventoryMenuOpen}, isDialogVisible={isDialogVisible}");
    }

    /// <summary>
    /// Handles the input action for toggling the SeedUI.
    /// </summary>
    /// <param name="context">The input action context.</param>
    private void OnToggleSeedUI(InputAction.CallbackContext context)
    {
        Debug.Log("Exit Button");
        if (isDialogVisible && isInventoryMenuOpen)
        {
            Debug.Log("Seed toogle off");
            SeedUI.SetActive(false);
            isSeedUIVisible = false;
            tutorialPanel.SetActive(false);
            isTuorialOpen = false;
            isDialogVisible = false;
            isInventoryMenuOpen = false;
        }
        else if (IsPlayerNear() && !GameManager.Instance.gameIsWon)
        {
            ToggleSeedUI(true);
        }
    }

    /// <summary>
    /// Detects the nearest TutorialBoard within proximityThreshold. 
    /// If found, opens the tutorial panel and displays its tutorialInformation.
    /// If not found, hides the panel.
    /// </summary>
    public void CheckNearbyTutorialBoard(InputAction.CallbackContext context)
    {
        if (isDialogVisible && isInventoryMenuOpen)
        {
            Debug.Log("Seed toogle off");
            tutorialPanel.SetActive(false);
            isTuorialOpen = false;
            isSeedUIVisible = false;
            isDialogVisible = false;
            isInventoryMenuOpen = false;
        }
        else if (FindClosestTutorialBoard() && !GameManager.Instance.gameIsWon)
        {

            OpenTutorialPanel(true);

        }
    }


    private void OnToggleQuestUI(InputAction.CallbackContext context)
    {
            if (IsPlayerNearTerminal() && !GameManager.Instance.gameIsWon)
            {
            // Open UI Panel
            ToogleTerminalUI(true);
                //terminalUI.SetActive(true);
            }
    }



    /// <summary>
    /// Open the Recycle Shop UI and disable the Ready to Recycle UI
    /// </summary>
    public void OpenInventoryMenu()
    {
            PlayerLogUI.gameObject.SetActive(false);
            SeedUI.SetActive(false);
            tutorialPanel.SetActive(false);
            cameraInventory.gameObject.SetActive(false);
            AmmoUI.gameObject.SetActive(false);
            InventoryMenuUI.gameObject.SetActive(true);
            EcoRestorationHUD.gameObject.SetActive(false);
            isInventoryMenuOpen = true;
            isPhotoAlbumOpen = false;
            isDialogVisible = true;
            isQuestsWindowOpen = false;
            isMapOpen = false;
    }

    public void OpenPhotoAlbum()
    {
        PlayerLogUI.gameObject.SetActive(false);
        InventoryMenuUI.gameObject.SetActive(false);
        AmmoUI.gameObject.SetActive(false);
        EcoRestorationHUD.gameObject.SetActive(false);
        cameraInventory.gameObject.SetActive(true);
        isInventoryMenuOpen = false;
        isPhotoAlbumOpen = true;
        isQuestsWindowOpen = false;
        isDialogVisible = true;
        isMapOpen = false;
    }

    public void OpenPotSuccessWindow()
    {
        seedCongratulationsUI.gameObject.SetActive(true);
        PlayerLogUI.gameObject.SetActive(false);
        SeedUI.SetActive(false);
        tutorialPanel.SetActive(false);
        cameraInventory.gameObject.SetActive(false);
        AmmoUI.gameObject.SetActive(false);
        InventoryMenuUI.gameObject.SetActive(false);
        EcoRestorationHUD.gameObject.SetActive(false);
        weaponEquipManager.SetToolActive(false);
        isInventoryMenuOpen = false;
        isPhotoAlbumOpen = false;
        isDialogVisible = true;
        isQuestsWindowOpen = false;
        isSeedCongratsOpen = true;

    }

    public void ClosePotSuccessWindow()
    {
        seedCongratulationsUI.gameObject.SetActive(false);
        PlayerLogUI.gameObject.SetActive(false);
        SeedUI.SetActive(false);
        tutorialPanel.SetActive(false);
        cameraInventory.gameObject.SetActive(false);
        AmmoUI.gameObject.SetActive(false);
        InventoryMenuUI.gameObject.SetActive(false);
        EcoRestorationHUD.gameObject.SetActive(false);
        weaponEquipManager.SetToolActive(true);
        isInventoryMenuOpen = false;
        isPhotoAlbumOpen = false;
        isDialogVisible = false;
        isQuestsWindowOpen = false;
        isSeedCongratsOpen = true;
    }

    public void OpenQuestsWindow()
    {
        PlayerLogUI.gameObject.SetActive(false);
        InventoryMenuUI.gameObject.SetActive(false);
        EcoRestorationHUD.gameObject.SetActive(false);
        QuestUI.gameObject.SetActive(true);
        AmmoUI.gameObject.SetActive(false);
        cameraInventory.gameObject.SetActive(false);
        isInventoryMenuOpen = false;
        isPhotoAlbumOpen = false;
        isDialogVisible = true;
        isQuestsWindowOpen = true;
        isMapOpen = false;
    }

    public void OpenMapWindow()
    {
        PlayerLogUI.gameObject.SetActive(false);
        InventoryMenuUI.gameObject.SetActive(false);
        EcoRestorationHUD.gameObject.SetActive(true);
        cameraInventory.gameObject.SetActive(false);
        QuestUI.gameObject.SetActive(false);
        AmmoUI.gameObject.SetActive(false);
        isInventoryMenuOpen = false;
        isPhotoAlbumOpen = false;
        isDialogVisible = true;
        isQuestsWindowOpen = false;
        isMapOpen = true;

    }

    public void CheckGameIsComplete()
    {

        isDialogVisible = true;
        bool condition = GameManager.Instance.CanCompleteGame();
        if (GameManager.Instance.gameIsWon)
        {
            if(condition && GameManager.Instance.getCurrentProgress() >= 0.1)
            {
                WinUI2.SetActive(true);
                WinUI.SetActive(false);
            }
            else if (condition && GameManager.Instance.getCurrentProgress() < 0.1)
            {
                WinUI.SetActive(true);
                WinUI2.SetActive(false);

            }
        }
    }
    
    public void TimeIsOut()
    {
        isDialogVisible = true;
        GameManager.Instance.gameIsWon = true;
        WinUI.SetActive(false);
        WinUI2.SetActive(false);

    }

    /// <summary>
    /// Close Recycle Shop UI and return to Ready to Recycle UI
    /// </summary>
    private void CloseInventoryMenu()
    {
        InventoryMenuUI.gameObject.SetActive(false);
        PlayerLogUI.gameObject.SetActive(true);
        EcoRestorationHUD.gameObject.SetActive(false);
        isInventoryMenuOpen = false;
        isPhotoAlbumOpen = false;
        isDialogVisible = true;
        isSeedUIVisible = false;
        isTuorialOpen = false;
        isMapOpen = false;
        Debug.Log("Recycle Shop Closed, Back to Ready to Recycle");
    }

    private void ClosePhotoAlbum()
    {
        PlayerLogUI.gameObject.SetActive(true);
        InventoryMenuUI.gameObject.SetActive(false);
        cameraInventory.gameObject .SetActive(false);
        EcoRestorationHUD.gameObject.SetActive(false);
        isInventoryMenuOpen = false;
        isPhotoAlbumOpen = false;
        isMapOpen = false;
        isDialogVisible = true;
    }

    private void CloseQuestsWindow()
    {
        PlayerLogUI.gameObject.SetActive(true);
        InventoryMenuUI.gameObject.SetActive(false);
        cameraInventory.gameObject.SetActive(false);
        QuestUI.gameObject .SetActive(false);
        EcoRestorationHUD.gameObject.SetActive(false);
        isInventoryMenuOpen = false;
        isPhotoAlbumOpen = false;
        isQuestsWindowOpen = false;
        isMapOpen = false;
        isDialogVisible = true;
    }

    private void CloseMapWindow()
    {
        PlayerLogUI.gameObject.SetActive(true);
        InventoryMenuUI.gameObject.SetActive(false);
        cameraInventory.gameObject.SetActive(false);
        QuestUI.gameObject.SetActive(false);
        EcoRestorationHUD.gameObject.SetActive(true);
        isInventoryMenuOpen = false;
        isPhotoAlbumOpen = false;
        isQuestsWindowOpen = false;
        isMapOpen = true;
        isDialogVisible = true;
    }

    /// <summary>
    /// Open the Ready to Recycle UI
    /// </summary>
    private void OpenPlayerLog()
    {
        PlayerLogUI.gameObject.SetActive(true);
        InventoryMenuUI.gameObject.SetActive(false);
        cameraInventory.gameObject.SetActive(false);
        EcoRestorationHUD.gameObject.SetActive(false);
        AmmoUI.gameObject.SetActive(false);
        isInventoryMenuOpen = false;
        isMapOpen = false;
        isPhotoAlbumOpen = false;
        isDialogVisible = true;
        ShowDialog();
        Debug.Log("Ready to Recycle UI Opened");
    }

    /// <summary>
    /// Close the Ready to Recycle UI
    /// </summary>
    public void ClosePlayerLog()
    {
        PlayerLogUI.gameObject.SetActive(false);
        isDialogVisible = false;
        HideDialog();
        Debug.Log("Ready to Recycle UI Closed");
    }


    private void OpenTutorialPanel(bool v)
    {
        isTuorialOpen = v;
        tutorialPanel.SetActive(v);
        tutorialText.text = player.GetComponent<Inventory>().GetTutorialText();
        isSeedUIVisible = false;
        InventoryMenuUI.SetActive(false);
        AmmoUI.gameObject.SetActive(false);
        weaponEquipManager.SetToolActive(!v);
        if (true)
        {
            isDialogVisible = v;
        }
    }


    /// <summary>
    /// Toggles the visibility of the SeedUI.
    /// </summary>
    /// <param name="visible">True to show the UI, false to hide it.</param>
    private void ToggleSeedUI(bool visible)
    {
        isSeedUIVisible = visible;
        isTuorialOpen = false;
        SeedUI.SetActive(visible);
        tutorialPanel.SetActive(false);
        InventoryMenuUI.SetActive(false);
        AmmoUI.gameObject.SetActive(false);
        weaponEquipManager.SetToolActive(!visible);
        if (true)
        {
            isDialogVisible = visible;
            UpdateYesButtonState();
        }
    }
    private void ToogleTerminalUI(bool v)
    {
       isResorationUIVisible = v;
       EcoRestorationHUD.SetActive(v);
       InventoryMenuUI.SetActive(false);
       AmmoUI.gameObject.SetActive(false);
       weaponEquipManager.SetToolActive(!v);
       if(true)
        {
            isDialogVisible = v;

        }
    }
    public void CloseSeedShop()
    {
        SeedUI.gameObject.SetActive(false);
        isDialogVisible = false;
        isSeedUIVisible = false;
        isTuorialOpen = false;
        HideDialog();
        Debug.Log("Ready to Recycle UI Closed");
    }

    public void CloseTutorialBox()
    {
        tutorialPanel.SetActive(false);
        isDialogVisible = false;
    }

    public void CloseTerminalUI()
    {
        EcoRestorationHUD.SetActive(false);
        isDialogVisible = false;
        isResorationUIVisible = false;
        HideDialog();
    }

    /// <summary>
    /// Updates the state of the "Yes" button based on whether the player has a seed.
    /// </summary>
    private void UpdateYesButtonState()
    {
        if (hasSeed)
        {
            // Make the "Yes" button interactable and change its color to indicate availability
            yesButton.interactable = true;
            ColorBlock colors = yesButton.colors;
            colors.normalColor = Color.white; // Default color when interactable
            yesButton.colors = colors;
        }
        else
        {
            // Make the "Yes" button non-interactable and change its color to indicate unavailability
            yesButton.interactable = false;
            ColorBlock colors = yesButton.colors;
            colors.normalColor = Color.gray; // Gray out the button when not interactable
            yesButton.colors = colors;
        }
    }

    /// <summary>
    /// Call this method to update whether the player has a seed.
    /// </summary>
    /// <param name="hasSeed">True if the player has a seed, false otherwise.</param>
    public void SetHasSeed(bool hasSeed)
    {
        this.hasSeed = hasSeed;

        // Update the "Yes" button state immediately
        UpdateYesButtonState();
    }

    public void ResetGame()
    {
        // Get the current active scene
        Scene currentScene = SceneManager.GetActiveScene();

        // Reload the current scene
        SceneManager.LoadScene(currentScene.name);
    }

    public void DisbaleAmmoUI()
    {
        AmmoUI.gameObject.SetActive(false);
    }    

    public void OnApplicationQuit()
    {
        Application.Quit();
    }
}