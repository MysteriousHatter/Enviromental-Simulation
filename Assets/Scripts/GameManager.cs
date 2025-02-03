using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; set; }

    public int currentScore { get; set; }
    public int goalScore = 10;
    private SkyboxManager enviroment => FindAnyObjectByType<SkyboxManager>();
    //private GrassComponent grass => FindAnyObjectByType<GrassComponent>();
    private GrassGrowthManager grass => FindAnyObjectByType<GrassGrowthManager>();
    private TreeManager tree => FindAnyObjectByType<TreeManager>();
    private WaterManager water => FindAnyObjectByType<WaterManager>();

    private MusicManager music => FindAnyObjectByType<MusicManager>();
    public bool gameIsWon { get;  set; }

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
        gameIsWon = false;
      

    }

    private void Start()
    {
        goalScore = spawner.getRecycableCount();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // Simulate count increase
        {
            currentScore += 1; // Increase count
            spawner.placeholderRecyacableCount--;
            CheckProgress();
        }

        if (currentScore == 6)
        {
            DetachChild detachChild = FindAnyObjectByType<DetachChild>();
            if (detachChild != null)
            {
                detachChild.DropItem();
            }
        }
        if(currentScore == 7) { RenderSettings.fog = false; }
    }
    public bool CompleteGame()
    {
        if(spawner.getCurrentRecycableCount() <= 0)
        {
            gameIsWon = true;
            if (currentScore >= goalScore)
            {
                Debug.Log("We Won");
                return true;
            }
            else if (currentScore < goalScore)
            {
                Debug.Log("We lost");
                return false;
            }
        }

        return false;
    }

    public void CheckProgress()
    {
        goalScore = spawner.getRecycableCount();
        float percentage = (float)currentScore / goalScore * 100f;

        Debug.Log($"Current Count: {currentScore}, Percentage: {percentage}%");

        // Update Terrain and Skybox based on progress
        enviroment.SetProgress(percentage / 100f); // Normalize percentage (0-1 range)
        music.SetProgress(percentage / 100f);
        grass.AddProgress(); // Updates grass growth
        water.SetProgress();
    }
}
