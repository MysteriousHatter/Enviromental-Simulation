using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class Cuttable : MonoBehaviour
{
    [Header("Cut Health & Yield")]
    public float hitPoints = 1f;
    public float biomassYield = 1f;

    [Header("Clear Behavior")]
    public bool destroyOnClear = true;
    public GameObject[] disableOnClear;
    public UnityEvent OnCleared;

    private float _hp;

    void Awake() => _hp = Mathf.Max(0.01f, hitPoints);

    /// <summary>Returns true if cleared; out param gives biomass awarded this strike (on clear).</summary>
    public bool ApplyCut(float power, out float yield)
    {
        yield = 0f;
        _hp -= Mathf.Max(0.01f, power);
        if (_hp > 0f) return false;

        yield = Mathf.Max(0f, biomassYield);
        Clear();
        return true;
    }

    private void Clear()
    {
        OnCleared?.Invoke();
        if (destroyOnClear)
        {
            Destroy(gameObject);
        }
        else
        {
            foreach (var g in disableOnClear) if (g) g.SetActive(false);
            foreach (var c in GetComponentsInChildren<Collider>(true)) c.enabled = false;
            foreach (var r in GetComponentsInChildren<Renderer>(true)) r.enabled = false;
        }
    }
}

