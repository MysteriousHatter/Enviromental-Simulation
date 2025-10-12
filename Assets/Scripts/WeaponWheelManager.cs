using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponWheelManager : MonoBehaviour
{
    [SerializeField] private GameObject weaponWheelUI; // Assign the weapon wheel UI in the inspector
    [SerializeField] private InputActionReference toggleWeaponWheelAction; // Assign the input action for toggling
    [SerializeField] private WeaponEquipManager weaponEquipManager;

    public GameObject[] totalToolsAvailable;

    private bool isWeaponWheelActive = false;

    private void OnEnable()
    {
        toggleWeaponWheelAction.action.performed += ToggleWeaponWheel;
    }

    private void OnDisable()
    {
        toggleWeaponWheelAction.action.performed -= ToggleWeaponWheel;
    }

    private void ToggleWeaponWheel(InputAction.CallbackContext context)
    {
        isWeaponWheelActive = !isWeaponWheelActive;
        weaponWheelUI.SetActive(isWeaponWheelActive);

        if (weaponEquipManager != null)
        {
            weaponEquipManager.SetToolActive(!isWeaponWheelActive);
        }
    }
}
