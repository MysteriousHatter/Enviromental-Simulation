using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace BioTools
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class XRI_ToolBinder : MonoBehaviour
    {
        public BioToolBase tool;                             // your tool (GrassCutter, SeedSpreader, Frog, etc.)
        [Header("Absorb input (drag actions here)")]
        public InputActionReference absorbRight;             // e.g. XRI RightHand → UI Press (Primary)
        public InputActionReference absorbLeft;              // e.g. XRI LeftHand  → UI Press (Primary)
        public bool preventSprayWhileAbsorbing = true;

        XRGrabInteractable grab;
        bool absorbingHeld;

        void Awake()
        {
            grab = GetComponent<XRGrabInteractable>();
            if (!tool) tool = GetComponent<BioToolBase>();

            // Spray via XRI Activate
            grab.activated.AddListener(_ => OnActivateDown());
            grab.deactivated.AddListener(_ => OnActivateUp());

            grab.selectEntered.AddListener(_ => tool?.Equip());
            grab.selectExited.AddListener(_ =>
            {
                tool?.Unequip();
                if (tool is ReedFrogPurifierTool frog) frog.AbsorbUp();
                absorbingHeld = false;
            });
        }

        void OnEnable()
        {
            // Enable/subscribe to both hands' Absorb actions (safe even if one is null)
            Subscribe(absorbRight);
            Subscribe(absorbLeft);
        }

        void OnDisable()
        {
            Unsubscribe(absorbRight);
            Unsubscribe(absorbLeft);
        }

        void Subscribe(InputActionReference actionRef)
        {
            if (actionRef == null) return;
            var a = actionRef.action;
            if (a == null) return;
            // If you have an Input Action Manager in scene enabling the asset, no need to Enable() here.
            a.performed += OnAbsorbDown;
            a.canceled += OnAbsorbUp;
        }

        void Unsubscribe(InputActionReference actionRef)
        {
            if (actionRef == null) return;
            var a = actionRef.action;
            if (a == null) return;
            a.performed -= OnAbsorbDown;
            a.canceled -= OnAbsorbUp;
        }

        // Spray (Secondary mapped to Activate in your actions)
        void OnActivateDown()
        {
            if (preventSprayWhileAbsorbing && absorbingHeld) return;
            tool?.PressPrimary();
        }
        void OnActivateUp() => tool?.ReleasePrimary();

        // Absorb (Primary mapped to UI Press in your actions)
        void OnAbsorbDown(InputAction.CallbackContext _)
        {
            if (!(tool is ReedFrogPurifierTool frog)) return;
            if (!grab.isSelected) return;         // only while held
            frog.AbsorbDown();
            absorbingHeld = true;
        }
        void OnAbsorbUp(InputAction.CallbackContext _)
        {
            if (tool is ReedFrogPurifierTool frog) frog.AbsorbUp();
            absorbingHeld = false;
        }
    }
}
