using System;
using UnityEditor;
using UnityEngine;

public class MaterialModifier : MonoBehaviour
{
    public Color newColor = Color.red;

    void Start()
    {
        // Get the Renderer component of the GameObject
        Renderer renderer = GetComponent<Renderer>();

        if (renderer != null)
        {
            // Use the material property to create a unique instance of the material
            renderer.material.color = newColor;

            // Now, only this GameObject's material will be modified
            Debug.Log("Material modified for this GameObject only.");
        }
        else
        {
            Debug.LogError("Renderer component not found on this GameObject.");
        }
    }
}
