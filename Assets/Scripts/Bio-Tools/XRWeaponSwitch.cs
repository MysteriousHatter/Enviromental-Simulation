using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class XRWeaponSwitch : MonoBehaviour
{
    public XRGrabInteractable targetTool; // Assign your tool's XRGrabInteractable in the Inspector
    public XRDirectInteractor handInteractor; // Assign your hand's XRDirectInteractor in the Inspector

    private bool hasGrabbed = false;


}
