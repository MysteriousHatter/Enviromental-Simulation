using BioTools;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class WeaponEquipManager : MonoBehaviour
{
    [SerializeField] private Transform handAttachPoint; // Assign the hand attach point in the inspector
    private GameObject currentTool;
    private StickyGrabInteractable currentStickyTool { get; set; }
    [SerializeField] private GameObject WeaponSelectorUI;
    [SerializeField] private GameObject cameraProps;
    [SerializeField] private GameObject cameraUI;
    [SerializeField] private DialogBoxController dialog;
    [SerializeField] private GameObject[] ammoUI;
    /// <summary>
    /// Equip a tool by instantiating it from the BioToolButtonController and attaching it to the player's hand.
    /// </summary>
    /// <param name="toolButton">The BioToolButtonController associated with the selected tool.</param>
    public void EquipTool(BioToolButtonController toolButton)
    {
        if (toolButton.getToolID() == 4)
        {
            currentTool = cameraUI;
            currentTool.gameObject.SetActive(true);
            cameraProps.SetActive(true);
            WeaponSelectorUI.SetActive(false);
        }
        else
        {
            if (toolButton == null)
            {
                Debug.LogWarning("No tool button provided for equipping.");
                return;
            }

            // Instantiate the tool from the BioToolButtonController
            GameObject toolInstance = toolButton.getBioTool();

            // Get the StickyGrabInteractable component from the instantiated tool
            var stickyGrab = toolInstance.GetComponent<StickyGrabInteractable>();
            if (stickyGrab == null)
            {
                Debug.LogWarning("The instantiated tool does not have a StickyGrabInteractable component.");
                toolInstance.SetActive(false); // Clean up the instantiated object if it's invalid
                return;
            }
            else { currentStickyTool = stickyGrab; }

            // Drop the current tool if any
            if (currentTool != null)
            {
                var currentStickyGrab = currentTool.GetComponent<StickyGrabInteractable>();
                if (currentStickyGrab != null)
                {
                    currentStickyGrab.ForceDrop();
                }
                currentTool.SetActive(false);       
                cameraProps.SetActive(false);
            }

            AttachToHand(stickyGrab);
            WeaponSelectorUI.SetActive(false);
        }

    }

    public void UnequipTool()
    {
        // Check if there is a currently equipped tool
        if (currentTool != null)
        {
            // If the current tool has a StickyGrabInteractable component, force it to drop
            var currentStickyGrab = currentTool.GetComponent<StickyGrabInteractable>();
            if (currentStickyGrab != null)
            {
                currentStickyGrab.ForceDrop();
            }

            // Deactivate the current tool
            currentTool.SetActive(false);

            // Reset camera properties if applicable
            cameraProps.SetActive(false);

            // Clear the reference to the current tool
            currentTool = null;

            // Clear the reference to the current sticky tool
            currentStickyTool = null;

            // Optionally, update the UI to reflect no tool is equipped
            WeaponSelectorUI.SetActive(false);

            dialog.DisbaleAmmoUI();

            Debug.Log("Tool unequipped successfully.");
        }
        else
        {
            Debug.LogWarning("No tool is currently equipped to unequip.");
        }
    }

    private void AttachToHand(StickyGrabInteractable stickyGrab)
    {
        // Equip the new tool
        currentTool = stickyGrab.gameObject;
        currentTool.SetActive(true);

        // Make sure sticky hold is enabled
        stickyGrab.stickyHold = true;

        // Snap to hand
        stickyGrab.transform.SetPositionAndRotation(handAttachPoint.position, handAttachPoint.rotation);

        // Programmatic grab
        var handInteractor = handAttachPoint.GetComponentInParent<XRBaseInteractor>();
        var selectInteractor = handInteractor as IXRSelectInteractor;
        var selectInteractable = stickyGrab as IXRSelectInteractable;

        if (selectInteractor != null && selectInteractable != null && stickyGrab.interactionManager != null)
        {
            Debug.Log("Select Interactor");

            // --- FIX: clear existing selectors safely ---
            // Option A: snapshot (uncomment to use)
            var selectingSnapshot = new System.Collections.Generic.List<IXRInteractor>(stickyGrab.interactorsSelecting);
            foreach (var inter in selectingSnapshot)
            {
                var interAsSelect = inter as IXRSelectInteractor;
                if (interAsSelect != null)
                    stickyGrab.interactionManager.SelectExit(interAsSelect, selectInteractable);
            }

            // Option B: backwards-by-index (recommended to avoid allocs)
            //  for (int i = stickyGrab.interactorsSelecting.Count - 1; i >= 0; i--)
            //  {
            //       var inter = stickyGrab.interactorsSelecting[i];
            //       var interAsSelect = inter as IXRSelectInteractor;
            //       if (interAsSelect != null)
            //           stickyGrab.interactionManager.SelectExit(interAsSelect, selectInteractable);
            //   }
            // --- end FIX ---

            stickyGrab.interactionManager.SelectEnter(selectInteractor, selectInteractable);
        }
        else
        {
            // Fallback parenting if no proper interactor
            stickyGrab.transform.SetParent(handAttachPoint, worldPositionStays: true);
        }

    }

    public void SetToolActive(bool isActive)
    {
        foreach (GameObject ui in ammoUI)
        {
            ui.SetActive(false);
        }

        if (currentTool != null)
        {
            currentTool.SetActive(isActive);
            cameraProps.SetActive(isActive);
            if(isActive == true)
            {
                if (currentTool.name == "CameraUI")
                {
                    currentTool.SetActive(true);
                    cameraProps.SetActive(true);
                }
                else
                {
                    Debug.Log("Attach the hand");
                    AttachToHand(currentStickyTool);
                }
            }
        }
    }
}
