using System.Collections;
using System.Collections.Generic;
using Unity.AppUI.UI;
using UnityEngine;

public class SeedMiniGameManager : MonoBehaviour
{
    [Header("Spawning")]
    [SerializeField] private Transform spawnPoint;           // Where pots appear (upstream)
    [SerializeField] private List<GameObject> potPrefabs;    // Prefabs must include PotTarget + PotMover
    [SerializeField] private float spawnInterval = 1.75f;    // Seconds between spawns
    [SerializeField] private int maxActivePots = 6;

    [Header("River Motion")]
    [SerializeField] private Vector3 riverDirection = new Vector3(0, 0, 1); // world direction
    [SerializeField] private float potSpeed = 2.5f;

    [Header("Systems")]
    [SerializeField] private SeedMiniGameScoreManager scoreManager;

    private List<GameObject> _active = new();
    private Coroutine _loop;
    private bool _running;

    private void OnEnable()
    {
        StartMiniGame();
    }

    private void OnDisable()
    {
        StopMiniGame();
        CleanupAll();
    }

    public void StartMiniGame()
    {
        Debug.Log("Start Mini Game");
        if (_running) return;
        _running = true;
        _loop = StartCoroutine(SpawnLoop());
        if (scoreManager) scoreManager.ResetScore();
    }

    public void StopMiniGame()
    {
        if (!_running) return;
        _running = false;
        if (_loop != null) StopCoroutine(_loop);
        _loop = null;
    }

    private IEnumerator SpawnLoop()
    {
        while (_running)
        {
            if (_active.Count < maxActivePots)
            {

                InstantiatePot();
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    /// <summary>
    /// Public method you asked for: spawns a pot coming from the river.
    /// </summary>
    public void InstantiatePot()
    {
        if (potPrefabs == null || potPrefabs.Count == 0 || !spawnPoint) return;

        Debug.Log("Instantiate the pots");
        GameObject prefab = Instantiate(potPrefabs[Random.Range(0, potPrefabs.Count)], spawnPoint.position, spawnPoint.localRotation, spawnPoint.transform);
        _active.Add(prefab);

        // Hook mover
        var mover = prefab.GetComponent<PotMover>();
        if (mover)
        {
           mover.Initialize(riverDirection.normalized, potSpeed, OnPotDespawned);
        }

        // Hook target > score manager
        var target = prefab.GetComponent<PotTarget>();
        if (target && scoreManager)
        {
            target.Initialize(scoreManager);
        }
    }

    private void OnPotDespawned(GameObject pot)
    {
        _active.Remove(pot);
        if (pot) Destroy(pot);
    }

    private void CleanupAll()
    {
        foreach (var go in _active) if (go) Destroy(go);
        _active.Clear();
    }
}
