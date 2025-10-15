using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialModifierChildren : MonoBehaviour
{
    public Color newColor = Color.red;
    [SerializeField] float duration = 0.6f;


    Renderer renderer;
    Material _mat;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ChangeMaterial(newColor);
    }

    public void ChangeMaterial(Color colorChange)
    {
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer) _mat = renderer.material; // unique instance for THIS object

        if (renderer != null)
        {
            // Use the material property to create a unique instance of the material
            _mat.color = colorChange;

            // Now, only this GameObject's material will be modified
            Debug.Log("Material modified for this GameObject only.");
        }
        else
        {
            Debug.LogError("Renderer component not found on this GameObject.");
        }
    }

    public void ChangeMaterialSmooth(Color target)
    {
        StopAllCoroutines();
        StartCoroutine(LerpColor(target, duration));
    }

    IEnumerator LerpColor(Color target, float seconds)
    {
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer) _mat = renderer.material; // unique instance for THIS object

        Color start = _mat.color;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, seconds);
            _mat.color = Color.Lerp(start, target, t);  // smooth step 0?1
            yield return null;
        }
        _mat.color = target; // ensure final value
    }
}
