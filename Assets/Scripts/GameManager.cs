using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    public int currentScore { get; set; }
    private int goalScore;

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
}
