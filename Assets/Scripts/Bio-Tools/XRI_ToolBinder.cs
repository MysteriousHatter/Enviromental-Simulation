using BioTools;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class XRI_ToolBinder : MonoBehaviour
{
    [SerializeField] private BioToolBase tool;
    XRGrabInteractable grab;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        if (!tool) tool = GetComponent<BioToolBase>();
        grab.selectEntered.AddListener(_ => tool?.Equip());
        grab.selectExited.AddListener(_ => tool?.Unequip());
        grab.activated.AddListener(_ => tool?.PressPrimary());     // Primary pressed
        grab.deactivated.AddListener(_ => tool?.ReleasePrimary());  // Primary released
    }
}