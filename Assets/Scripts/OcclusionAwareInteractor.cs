using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class OcclusionAwareInteractor : XRDirectInteractor
{
    public LayerMask wallLayerMask;

    public override bool CanSelect(IXRSelectInteractable interactable)
    {
        // Perform the default selection check
        if (!base.CanSelect(interactable))
            return false;

        // Perform occlusion check
        Vector3 direction = interactable.transform.position - transform.position;
        float distance = direction.magnitude;

        if (Physics.Raycast(transform.position, direction, distance, wallLayerMask))
        {
            // A wall is blocking the interaction
            return false;
        }

        // No wall detected, allow interaction
        return true;
    }
}
