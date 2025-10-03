using System;
using BioTools;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class BagEmptyingController : MonoBehaviour
{
    [SerializeField] private GrassCutter grassCutter; // Reference to the GrassCutter component
    [SerializeField] private InputActionReference emptyBagAction; // Reference to the Input Action
    [SerializeField] private Transform player; // Reference to the player's Transform
    [SerializeField] private Transform grassBagTransform; // Reference to the Grass Bag's Transform
    [SerializeField] private float proximityThreshold = 5f; // Distance threshold for proximity

    private void OnEnable()
    {
        // Enable the input action
        emptyBagAction.action.Enable();
        emptyBagAction.action.performed += OnEmptyBagPerformed;
    }

    private void OnDisable()
    {
        // Disable the input action
        emptyBagAction.action.performed -= OnEmptyBagPerformed;
        emptyBagAction.action.Disable();
    }

    private void OnEmptyBagPerformed(InputAction.CallbackContext context)
    {
        // Check if the player is near the Grass Bag and call TryEmptyBag
        if (IsPlayerNear() && grassCutter != null)
        {
            grassCutter.TryEmptyBag();
        }
    }

    private bool IsPlayerNear()
    {
        // Calculate the distance between the player and the Grass Bag
        float distance = Vector3.Distance(player.position, grassBagTransform.position);
        Debug.Log("Player's Distance: " + distance);
        return distance <= proximityThreshold;
    }
}
