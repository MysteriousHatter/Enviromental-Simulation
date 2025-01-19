using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    public int currentScore { get; set; }
    public int goalScore = 10;
    private SkyboxManager enviroment => FindAnyObjectByType<SkyboxManager>();
    //private GrassComponent grass => FindAnyObjectByType<GrassComponent>();
    private GrassGrowthManager grass => FindAnyObjectByType<GrassGrowthManager>();
    private TreeManager tree => FindAnyObjectByType<TreeManager>();
    private WaterManager water => FindAnyObjectByType<WaterManager>();

    [SerializeField] RecyclableSpawner spawner;
    void Awake()
    {
        // Singleton Pattern: Ensure only one GameManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        currentScore = 0;
        goalScore = spawner.getRecycableCount();

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // Simulate count increase
        {
            currentScore += 1; // Increase count
            CheckProgress();
        }
    }
    public void CompleteGame()
    {
        if(spawner.getRecycableCount() <= 0)
        {
            if (currentScore >= goalScore)
            {
                Debug.Log("We Won");
            }
            else if (currentScore < goalScore)
            {
                Debug.Log("We lost");
            }
        }
    }

    public void CheckProgress()
    {
        goalScore = spawner.getRecycableCount();
        float percentage = (float)currentScore / goalScore * 100f;

        Debug.Log($"Current Count: {currentScore}, Percentage: {percentage}%");

        // Update Terrain and Skybox based on progress
        enviroment.SetProgress(percentage / 100f); // Normalize percentage (0-1 range)
        grass.AddProgress(); // Updates grass growth
        tree.SetProgress(percentage / 100f);
        water.SetProgress();
    }
}
