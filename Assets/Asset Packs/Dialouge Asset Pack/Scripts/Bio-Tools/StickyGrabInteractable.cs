using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

#if UNITY_6000_0_OR_NEWER
// XRI 3.x naming used here
#endif

/// <summary>
/// Per-item “sticky” behavior for XR Grab Interactables.
/// If stickyHold = true, releasing the grab input will immediately re-grab the item
/// so it stays attached to the same interactor (hand). Call ForceDrop() to really drop it.
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
public class StickyGrabInteractable : XRGrabInteractable
{
    [Header("Sticky Settings")]
    [Tooltip("If true, the object will remain in-hand even if the player releases the grab input.")]
    public bool stickyHold = false;

    [Tooltip("If true, apply throw velocity when a real drop happens (ForceDrop or being taken by socket, etc.).")]
    public bool allowThrowOnRealDrop = true;

    // Tracks the last hand that selected us
    private IXRSelectInteractor _lastInteractor;

    // When true, the next select exit is allowed to complete (no auto re-grab)
    private bool _forceDropRequested;

    protected override void OnEnable()
    {
        base.OnEnable();
        selectEntered.AddListener(OnSelected);
        selectExited.AddListener(OnDeselected);
    }

    protected override void OnDisable()
    {
        selectEntered.RemoveListener(OnSelected);
        selectExited.RemoveListener(OnDeselected);
        base.OnDisable();
    }

    private void OnSelected(SelectEnterEventArgs args)
    {
        _lastInteractor = args.interactorObject;

        // Optional: ensure we snap correctly if you use an Attach Transform.
        // XRI will handle this if you've set Attach Transform in the inspector.
        // Movement/Track types still controlled by the base XRGrabInteractable.
    }

    private void OnDeselected(SelectExitEventArgs args)
    {
        // If not sticky, do nothing special
        if (!stickyHold)
        {
            // Normal throw behavior controlled by XRGrabInteractable settings
            return;
        }

        // If a real drop was requested, allow it and reset flag
        if (_forceDropRequested)
        {
            _forceDropRequested = false;
            return;
        }

        // Auto re-grab next frame to “stick” in the same hand
        if (_lastInteractor != null && interactionManager != null && isActiveAndEnabled)
            StartCoroutine(RegrabNextFrame(_lastInteractor));
    }

    /// <summary>
    /// Call this from your own code (menu/holster/etc.) to actually drop a sticky item.
    /// </summary>
    public void ForceDrop()
    {
        if (!isSelected) return;

        _forceDropRequested = true;

        // Optionally disable throw on this specific drop
        var originalThrowOnDetach = throwOnDetach;
        if (!allowThrowOnRealDrop) throwOnDetach = false;

        // Deselect from whoever holds us
        // (XRGrabInteractable has helpers; use InteractionManager for clarity)
        foreach (var interactor in interactorsSelecting)
        {
            interactionManager.SelectExit(interactor, this);
            break; // single select expected
        }

        // Restore throw flag
        throwOnDetach = originalThrowOnDetach;
    }

    private IEnumerator RegrabNextFrame(IXRSelectInteractor interactor)
    {
        // Wait one frame so XRI finishes its internal deselect
        yield return null;

        // Ensure we're still around and the interactor exists
        if (interactor == null || interactionManager == null || !isActiveAndEnabled)
            yield break;

        // Attempt a programmatic re-select. This does not require the player still holding the button.
        // If CanSelect is false (e.g., blocked by layers), this will no-op.
        interactionManager.SelectEnter(interactor, this);
    }
}
