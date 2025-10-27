using UnityEngine;

public class SubmergeRocks : MonoBehaviour
{
    [Tooltip("Objects that will submerge")]
    public GameObject[] objectsToSubmerge;
    public GameObject[] resetTriggers;

    public float targetY = -5f;
    public float speed = 2f;

    // Keep track of which ones are currently submerging
    private bool[] _isSubmerging;

    void Awake()
    {
        // Initialize the tracking array
        if (objectsToSubmerge != null)
        {
            _isSubmerging = new bool[objectsToSubmerge.Length];
            for (int i = 0; i < _isSubmerging.Length; i++)
            {
                _isSubmerging[i] = false;
            }
        }
        else
        {
            _isSubmerging = new bool[0];
        }
    }

    void Update()
    {
        if (objectsToSubmerge == null) return;

        for (int i = 0; i < objectsToSubmerge.Length; i++)
        {
            GameObject go = objectsToSubmerge[i];
            if (go == null) continue;

            if (_isSubmerging[i])
            {
                Vector3 pos = go.transform.position;
                pos.y = Mathf.MoveTowards(pos.y, targetY, speed * Time.deltaTime);
                go.transform.position = pos;

                if (Mathf.Approximately(pos.y, targetY))
                {
                    _isSubmerging[i] = false;
                    // Optionally do something when it finishes, e.g. disable object or trigger event
                }
            }
        }
    }

    /// <summary>
    /// Starts the submerge process for the object at the given index.
    /// </summary>
    public void StartSubmerge(int index)
    {
        if (objectsToSubmerge == null) return;
        if (index < 0 || index >= objectsToSubmerge.Length) return;

        _isSubmerging[index] = true;
    }

    /// <summary>
    /// Starts submerge for **all** objects in the array.
    /// </summary>
    public void StartSubmergeAll()
    {
        for (int i = 0; i < _isSubmerging.Length; i++)
        {
            _isSubmerging[i] = true;
        }

        foreach(GameObject go in resetTriggers)
        {
            go.SetActive(false);
        }
    }
}
