using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.UI;
using static RecyclableItem;

public class DialogBoxController : MonoBehaviour
{
    [SerializeField] private GameObject ReadyToRecycle; // Assign your World Space Canvas here
    [SerializeField] private GameObject RecycleShopCanvas;
    [SerializeField] private float distanceFromCamera = 2.0f; // Distance in meters
    [SerializeField] private float horizontalSpacing = 1.5f; // Space between canvases
    [SerializeField] private InputActionReference InventoryButton;
    [SerializeField] private InputActionManager inputActionManager; // Drag your Input Action Manager here
    [SerializeField] private GameObject leftController;
    [SerializeField] private GameObject rightController;


    private Transform cameraTransform;
    private string defaultLayerMask = "Default";
    private string UILayerMask = "UI";
    
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

        if (isDialogVisible)
        {
            
            DisableAllExceptAllowed();
        }
        else
        {
            EnableAllActionMaps();
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
        RecycleShopCanvas.transform.position = basePosition + cameraTransform.right * horizontalSpacing;
        RecycleShopCanvas.transform.rotation = Quaternion.LookRotation(RecycleShopCanvas.transform.position - cameraTransform.position);
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

        // Align the canvas to face the camera
        ReadyToRecycle.transform.rotation = Quaternion.LookRotation(ReadyToRecycle.transform.position - cameraTransform.position);

        // Show the dialog
        ReadyToRecycle.gameObject.SetActive(true);

        leftController.GetComponentInChildren<NearFarInteractor>().interactionLayers = UIlayerMask;
        rightController.GetComponentInChildren<NearFarInteractor>().interactionLayers = UIlayerMask;

        leftController.GetComponentInChildren<NearFarInteractor>().interactionLayers &= ~defaultlayerMask;
        rightController.GetComponentInChildren<NearFarInteractor>().interactionLayers &= ~defaultlayerMask;
       raycaster.blockingMask = 1 << 5;
    }

    public void HideDialog()
    {
        // Hide the dialog
        ReadyToRecycle.gameObject.SetActive(false);

        InteractionLayerMask UIlayerMask = InteractionLayerMask.GetMask(UILayerMask);
        InteractionLayerMask defaultlayerMask = InteractionLayerMask.GetMask(defaultLayerMask);

        leftController.GetComponentInChildren<NearFarInteractor>().interactionLayers = defaultlayerMask;
        rightController.GetComponentInChildren<NearFarInteractor>().interactionLayers = defaultlayerMask;

        leftController.GetComponentInChildren<NearFarInteractor>().interactionLayers &= ~UIlayerMask;
        rightController.GetComponentInChildren<NearFarInteractor>().interactionLayers &= ~UIlayerMask;
        raycaster.blockingMask = 0;
        raycaster.blockingMask = 1;
    }

    /// <summary>
    /// Handle Input Toggle Logic for Menus
    /// </summary>
    private void OnToggleAction(InputAction.CallbackContext context)
    {
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
        else
        {
            // If no menus are open, open Ready to Recycle
            OpenReadyToRecycle();
        }

        Debug.Log($"Toggle Action: isRecycleShopOpen={isRecycleShopOpen}, isDialogVisible={isDialogVisible}");
    }

    /// <summary>
    /// Open the Recycle Shop UI and disable the Ready to Recycle UI
    /// </summary>
    public void OpenRecycleShop()
    {
        ReadyToRecycle.gameObject.SetActive(true);
        RecycleShopCanvas.gameObject.SetActive(true);
        isRecycleShopOpen = true;
        isDialogVisible = true;
        Debug.Log("Recycle Shop Opened");
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
    public void OnButtonPress(string material)
    {
        Inventory inventory = FindAnyObjectByType<Inventory>();

        if (System.Enum.TryParse(material, out RecyclableType type))
        {
            if (inventory.useRecycable(type))
            {
                GameManager.Instance.currentScore++;
                GameManager.Instance.CheckProgress();
                Debug.Log($"Correct Item: {type}. Score Updated!");
            }
            else
            {
                Debug.Log($"Item {type} not available in inventory.");
            }
        }
        else
        {
            Debug.LogError($"Invalid material: {material}");
        }
    }
}
