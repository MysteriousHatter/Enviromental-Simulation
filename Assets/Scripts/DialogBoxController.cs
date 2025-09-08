using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.SceneManagement;
using static RecyclableItem;

public class DialogBoxController : MonoBehaviour
{
    [SerializeField] private GameObject ReadyToRecycle; // Assign your World Space Canvas here
    [SerializeField] private GameObject RecycleShopCanvas;
    [SerializeField] private GameObject cameraInventory;
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
    [SerializeField] private GameObject LoseUI;
    [SerializeField] private RecyclableSpawner recyclableSpawner;

    private Transform cameraTransform;
    private string defaultLayerMask = "Default";
    private string recycaleLayerMask = "Recycable";
    private string UILayerMask = "UI";
    private RecyclableType type;

    [SerializeField] private Transform player;
    [SerializeField] private Transform recycleShop; // Assign the item's Transform
    [SerializeField] private float proximityThreshold = 3.0f; // Distance threshold for being "near

    public ItemSlot[] itemSlot;
    public ItemSlot[] photoSlot;

    TrackedDeviceGraphicRaycaster raycaster => GetComponentInChildren<TrackedDeviceGraphicRaycaster>();
    private bool isDialogVisible = true;
    private bool isRecycleShopOpen = false; // Tracks if Recycle Shop UI is active
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
        ReadyToRecycle.gameObject.SetActive(false);
        RecycleShopCanvas.gameObject.SetActive(false);
        cameraMode.gameObject.SetActive(false);
        cameraInventory.gameObject.SetActive(false);
        WinUI.gameObject.SetActive(false);
        LoseUI.gameObject.SetActive(false);
        isDialogVisible = false;

        InventoryButton.action.performed += OnToggleAction;
    }

    void OnDestroy()
    {
        // Unsubscribe from the input action event to prevent memory leaks
        InventoryButton.action.performed -= OnToggleAction;
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
    }

    private void PositionCanvas()
    {
        // Position ReadyToRecycle UI directly in front of the camera
        Vector3 basePosition = cameraTransform.position + cameraTransform.forward * distanceFromCamera;

        // Position Ready to Recycle
        ReadyToRecycle.transform.position = basePosition;
        ReadyToRecycle.transform.rotation = Quaternion.LookRotation(ReadyToRecycle.transform.position - cameraTransform.position);

        // Position Recycle Shop
        RecycleShopCanvas.transform.position = basePosition;
        RecycleShopCanvas.transform.rotation = Quaternion.LookRotation(RecycleShopCanvas.transform.position - cameraTransform.position);

        cameraInventory.transform.position = basePosition;
        cameraInventory.transform.rotation = Quaternion.LookRotation(cameraInventory.transform.position - cameraTransform.position);

        //Postion Camera
        cameraMode.transform.position = basePosition;
        cameraMode.transform.rotation = Quaternion.LookRotation(cameraMode.transform.position - cameraTransform.position);

        ShowPhotoUI.transform.position = basePosition;
        ShowPhotoUI.transform.rotation = Quaternion.LookRotation(ShowPhotoUI.transform.position - cameraTransform.position);

        WinUI.transform.position = basePosition;
        WinUI.transform.rotation = Quaternion.LookRotation(WinUI.transform.position - cameraTransform.position);

        LoseUI.transform.position = basePosition;
        LoseUI.transform.rotation = Quaternion.LookRotation(LoseUI.transform.position - cameraTransform.position);

        timer.transform.position = basePosition;
        timer.transform.rotation = Quaternion.LookRotation(timer.transform.position - cameraTransform.position);
    }

    public bool IsPlayerNear()
    {
        float distance = Vector3.Distance(player.position, recycleShop.position);
        return distance <= proximityThreshold;
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
        ReadyToRecycle.transform.position = newPosition;
        InteractionLayerMask UIlayerMask = InteractionLayerMask.GetMask(UILayerMask);
        InteractionLayerMask defaultlayerMask = InteractionLayerMask.GetMask(defaultLayerMask);
        InteractionLayerMask recycalelayerMask = InteractionLayerMask.GetMask(recycaleLayerMask);

        // Align the canvas to face the camera
        ReadyToRecycle.transform.rotation = Quaternion.LookRotation(ReadyToRecycle.transform.position - cameraTransform.position);

        // Show the dialog
        ReadyToRecycle.gameObject.SetActive(true);

        leftController.GetComponentInChildren<NearFarInteractor>().interactionLayers = UIlayerMask;
        rightController.GetComponentInChildren<NearFarInteractor>().interactionLayers = UIlayerMask;

        leftController.GetComponentInChildren<NearFarInteractor>().interactionLayers &= ~defaultlayerMask;
        rightController.GetComponentInChildren<NearFarInteractor>().interactionLayers &= ~defaultlayerMask;

        leftController.GetComponentInChildren<NearFarInteractor>().interactionLayers &= ~recycalelayerMask;
        rightController.GetComponentInChildren<NearFarInteractor>().interactionLayers &= ~recycalelayerMask;
        //raycaster.blockingMask = 1 << 5;
    }

    public void HideDialog()
    {
        // Hide the dialog
        ReadyToRecycle.gameObject.SetActive(false);

        InteractionLayerMask UIlayerMask = InteractionLayerMask.GetMask(UILayerMask);
        InteractionLayerMask defaultlayerMask = InteractionLayerMask.GetMask(defaultLayerMask);
        InteractionLayerMask recycalelayerMask = InteractionLayerMask.GetMask(recycaleLayerMask);

        leftController.GetComponentInChildren<NearFarInteractor>().interactionLayers = defaultlayerMask;
        rightController.GetComponentInChildren<NearFarInteractor>().interactionLayers = defaultlayerMask;

        leftController.GetComponentInChildren<NearFarInteractor>().interactionLayers = recycalelayerMask;
        rightController.GetComponentInChildren<NearFarInteractor>().interactionLayers = recycalelayerMask;

        leftController.GetComponentInChildren<NearFarInteractor>().interactionLayers &= ~UIlayerMask;
        rightController.GetComponentInChildren<NearFarInteractor>().interactionLayers &= ~UIlayerMask;
        //raycaster.blockingMask = 0;
        //raycaster.blockingMask = 1;
    }

    /// <summary>
    /// Handle Input Toggle Logic for Menus
    /// </summary>
    private void OnToggleAction(InputAction.CallbackContext context)
    {
        bool isNear = IsPlayerNear();

        if (isRecycleShopOpen)
        {
            // If Recycle Shop is open, close it and return to Ready to Recycle
            CloseRecycleShop();
        }
        else if (isDialogVisible)
        {
            // If Ready to Recycle is open, close it
            CloseReadyToRecycle();
        }
        else if (!GameManager.Instance.gameIsWon && isNear)
        {
            // If no menus are open, open Ready to Recycle
            OpenReadyToRecycle();
        }

        Debug.Log($"Toggle Action: isRecycleShopOpen={isRecycleShopOpen}, isDialogVisible={isDialogVisible}");
    }

    /// <summary>
    /// Open the Recycle Shop UI and disable the Ready to Recycle UI
    /// </summary>
    public void OpenRecycleShop(string material)
    {

        // Attempt to parse the material into a valid RecyclableType
        if (System.Enum.TryParse(material, out RecyclableType recyclableType))
        {
            // Update the current type based on the parsed recyclableType
            type = recyclableType;
            recyclableSpawner.currentRecyclableType = recyclableType; // Update the spawner's current type

            // Activate UI components
            ReadyToRecycle.gameObject.SetActive(false);
            RecycleShopCanvas.gameObject.SetActive(true);
            isRecycleShopOpen = true;
            isDialogVisible = true;

            Debug.Log("Recycle Shop Opened");
            Debug.Log($"Current recyclable set to: {type}");
        }
        else
        {
            // Log an error if the material is invalid
            Debug.LogError($"Invalid recyclable type: {material}");
        }
    }

    public void CheckGameIsComplete()
    {

        isDialogVisible = true;
        bool condition = GameManager.Instance.CompleteGame();
        if (GameManager.Instance.gameIsWon)
        {
           // ReadyToRecycle.SetActive(false);
            if (condition)
            {
                WinUI.SetActive(true);

            }
            else
            {
                LoseUI.SetActive(true);
            }
        }
    }
    
    public void TimeIsOut()
    {
        isDialogVisible = true;
        GameManager.Instance.gameIsWon = true;
        LoseUI.SetActive(true);
        WinUI.SetActive(false);

    }

    /// <summary>
    /// Close Recycle Shop UI and return to Ready to Recycle UI
    /// </summary>
    private void CloseRecycleShop()
    {
        RecycleShopCanvas.gameObject.SetActive(false);
        ReadyToRecycle.gameObject.SetActive(true);
        isRecycleShopOpen = false;
        isDialogVisible = true;
        Debug.Log("Recycle Shop Closed, Back to Ready to Recycle");
    }

    /// <summary>
    /// Open the Ready to Recycle UI
    /// </summary>
    private void OpenReadyToRecycle()
    {
        ReadyToRecycle.gameObject.SetActive(true);
        RecycleShopCanvas.gameObject.SetActive(false);
        isRecycleShopOpen = false;
        isDialogVisible = true;
        ShowDialog();
        Debug.Log("Ready to Recycle UI Opened");
    }

    /// <summary>
    /// Close the Ready to Recycle UI
    /// </summary>
    private void CloseReadyToRecycle()
    {
        ReadyToRecycle.gameObject.SetActive(false);
        isDialogVisible = false;
        HideDialog();
        Debug.Log("Ready to Recycle UI Closed");
    }

    // Handle Button Presses
    //public void OnButtonPress(string material)
    //{
    //    Inventory inventory = FindAnyObjectByType<Inventory>();
    //    RecyclableSpawner.placeholderRecyacableCount--;

    //    if (System.Enum.TryParse(material, out RecyclableType type))
    //    {
    //        // Check if the selected recyclable type matches the current recyclable type
    //        if (RecyclableSpawner.currentRecyclableType == type)
    //        {
    //            // Correct selection
    //            if (inventory.useRecycable(type)) // Check if the item is available in inventory
    //            {
    //                GameManager.Instance.currentScore++;
    //                GameManager.Instance.CheckProgress();
    //                Debug.Log($"Correct Item: {type}. Score Updated!");
    //            }
    //            else
    //            {
    //                Debug.Log($"Item {type} not available in inventory.");
    //            }
    //        }
    //        else
    //        {
    //            // Incorrect selection
    //            Debug.Log($"Incorrect selection! Expected: {RecyclableSpawner.currentRecyclableType}, but selected: {type}");

    //            inventory.useRecycable(type); // Attempt to use the item, even if incorrect
    //        }
    //    }
    //    else
    //    {
    //        Debug.LogError($"Invalid material: {material}");
    //    }
    //}


    public void ResetGame()
    {
        // Get the current active scene
        Scene currentScene = SceneManager.GetActiveScene();

        // Reload the current scene
        SceneManager.LoadScene(currentScene.name);
    }

    public void OnApplicationQuit()
    {
        Application.Quit();
    }
}