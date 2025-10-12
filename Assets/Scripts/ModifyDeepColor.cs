// 10/6/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using UnityEditor;
using UnityEngine;

public class ModifyDeepColor : MonoBehaviour
{
    [SerializeField] private Material waterMaterial; // Assign Water_mat_01 in the Inspector
    [SerializeField] private Color newDeepColor = new Color(0f, 0.5f, 0.4f, 0.2f); // Example color

    void Start()
    {
        if (waterMaterial != null)
        {
            // Modify the "Deep Color" property
            waterMaterial.SetColor("Color_36218622185947c6a5ae36366d8e21d8", newDeepColor);
            Debug.Log("Deep Color updated to: " + newDeepColor);
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = waterMaterial; // Reassign the material to force an update
            }
        }
        else
        {
            Debug.LogError("Water material is not assigned!");
        }
    }
}
