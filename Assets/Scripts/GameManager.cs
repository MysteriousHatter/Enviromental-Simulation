using RPG.Quests;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static QuestSystem;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; set; }

    // ---- Progress model (0..1) ----
    [Header("Progress Targets")]
    [Tooltip("Minimum progress to clear the area (excluding Extra Objective). 0.80 = 80%")]
    [Range(0f, 1f)] public float goalTarget01 = 0.80f;

    [Tooltip("Each main section/seed is worth 12% (5 sections ? 60% total).")]
    [Range(0f, 1f)] public float mainSectionValue01 = 0.12f; // 12%

    [Tooltip("Default side objective value when not specified (use 0.03–0.05).")]
    [Range(0f, 1f)] public float defaultSideValue01 = 0.03f; // 3%

    [Header("Live Progress (read-only)")]
    [SerializeField, Range(0f, 1f)] private float currentProgress01 = 0f;
    [SerializeField] private int mainCompletedCount = 0;       // out of 5
    [SerializeField] private int sideCompletedCount = 0;       // ~6–8 total recommended

    [Header("Debug/Test")]
    [SerializeField] private bool debugHotkeys = true;
    [SerializeField] private KeyCode addMainKey = KeyCode.M;
    [SerializeField] private KeyCode addSideKey = KeyCode.N;
    [SerializeField] private KeyCode addPercentKey = KeyCode.P;
    [SerializeField, Range(0f, 0.25f)] private float addPercentStep01 = 0.01f; // 1%

    [Header("Game Completion")]
    [Range(0f, 1f)] public float completionThreshold01 = 0.65f; // enable “Complete Game” once all quests done AND progress >= this
    [SerializeField] private GameObject seedMiniGamePrefab;

    // Assign this in the Inspector or fetch at runtime
    [SerializeField] private QuestList _playerQuestList;
    private QuestList PlayerQuestList => _playerQuestList;


    // Optional: small helper to generate unique IDs in tests
    private int _testMainIdx = 0;
    private int _testSideIdx = 0;

    // Track what’s been awarded so we don’t double count
    private readonly HashSet<string> completedMainIds = new HashSet<string>();
    private readonly HashSet<string> completedSideIds = new HashSet<string>();
    private SkyboxManager enviroment => FindAnyObjectByType<SkyboxManager>();
    //private GrassComponent grass => FindAnyObjectByType<GrassComponent>();
    private GrassGrowthManager grass => FindAnyObjectByType<GrassGrowthManager>();
    private TreeManager tree => FindAnyObjectByType<TreeManager>();
    private WaterManager water => FindAnyObjectByType<WaterManager>();

    private MusicManager music => FindAnyObjectByType<MusicManager>();
    public QuestManager QuestManager => FindAnyObjectByType<QuestManager>();
    public bool gameIsWon { get;  set; }
    private bool alreadyHit = false;

    //[SerializeField] RecyclableSpawner spawner;
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

        //currentScore = 0;
        gameIsWon = false;
        QuestManager.GiveFirstQuest();
      

    }

    private void Start()
    {
       // goalScore = spawner.getRecycableCount();
    }

    void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (debugHotkeys)
        {
            if (Input.GetKeyDown(addMainKey))
            {
                RegisterMainObjectiveCompleted($"TEST_Main_{++_testMainIdx}");
            }
            if (Input.GetKeyDown(addSideKey))
            {
                RegisterSideObjectiveCompleted($"TEST_Side_{++_testSideIdx}");
            }
            if (Input.GetKeyDown(addPercentKey))
            {
                // direct % bump (useful to eyeball transitions)
                var before = GetProgress01();
                // Use internal AddProgress via a tiny proxy to keep it private outside.
                AddProgress_Editor(addPercentStep01);
                Debug.Log($"[TEST] Progress bump {before:P1} ? {GetProgress01():P1}");
            }
        }
#endif


        //if(currentScore == 2)
        //{
        //    QuestList questList = FindAnyObjectByType<QuestList>();
        //    questList.CompleteObjectiveByReference("Naturlist Away");
        //}
    }

    /// Call this from the UI “Next Quest” button
    public void TryAdvanceQuest()
    {
        var qm = QuestManager;
        if (qm == null) { Debug.LogWarning("No QuestManager found."); return; }

        var current = qm.GetCurrentQuest();
        if (current == null)
        {
            // Start the questline if needed
            qm.GiveFirstQuest();
            return;
        }

        if (IsCurrentQuestComplete())
        {
            if (HasNextQuest()) qm.GiveNextQuest();
            else Debug.Log("All quests are complete.");
        }
        else
        {
            Debug.Log("Current quest not complete—cannot advance.");
        }
    }

    /// True if the player has completed all objectives of the current quest
    public bool IsCurrentQuestComplete()
    {
        var qm = QuestManager;
        var ql = PlayerQuestList;
        if (qm == null || ql == null) return false;

        var current = qm.GetCurrentQuest();
        if (current == null) return false;

        // Uses QuestList/QuestStatus to evaluate completion
        var statuses = ql.GetStatuses();                   // assumes your QuestList exposes statuses
        var status = statuses.FirstOrDefault(s => s.GetQuest() == current);
        return status != null && status.IsComplete();
    }

    /// True if there’s another quest after the current one
    public bool HasNextQuest()
    {
        // If your QuestManager exposes HasNextQuest, call it directly.
        // Otherwise, mirror its logic here as needed.
        return QuestManager != null && QuestManager.HasNextQuest;
    }

    /// True if every quest in the questline is complete
    public bool AreAllQuestsComplete()
    {
        var qm = QuestManager;
        var ql = PlayerQuestList;
        if (qm == null || ql == null) return false;

        var statuses = ql.GetStatuses();
        return qm.AllQuests.All(q =>
        {
            var s = statuses.FirstOrDefault(st => st.GetQuest() == q);
            return s != null && s.IsComplete();
        });
    }

    /// Enable/disable “Next Quest” button
    public bool CanAdvanceQuest() => IsCurrentQuestComplete() && HasNextQuest();

    /// Enable/disable “Complete Game” button
    public bool CanCompleteGame()
    {
        // Require: all quests done AND restoration >= completionThreshold01
        return AreAllQuestsComplete() && GetProgress01() >= completionThreshold01;
    }

    /// Call this from the UI “Complete Game” button
    public void OnCompleteGamePressed()
    {
        if (!CanCompleteGame())
        {
            Debug.Log("Not ready to complete the game.");
            return;
        }

        // End-game logic (save, cutscene, load scene, unlock, etc.)
        Debug.Log("GAME COMPLETE! 🎉");
        CheckWinCondition();


        // e.g., SceneManager.LoadScene("Credits");
    }


    /// <summary>
    /// Register a main objective/section completion (worth 12% by default).
    /// Use stable IDs per section, e.g., "Seed_Conservatory", "Seed_Fountain", etc.
    /// </summary>
    public void RegisterMainObjectiveCompleted(string objectiveId)
    {

        completedMainIds.Add(objectiveId);
        mainCompletedCount++;
        AddProgress(mainSectionValue01);
        Debug.Log($"[Progress] Main complete: {objectiveId} (+{mainSectionValue01:P0})");
        //CheckWinCondition();
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    // Editor-only proxy wrapper around private AddProgress
    private void AddProgress_Editor(float delta01) => AddProgress(delta01);
#endif

    /// <summary>
    /// Register a side objective completion.
    /// Pass a value in the 0.03f..0.05f range per your design (3–5%).
    /// If omitted or <= 0, defaultSideValue01 is used (3%).
    /// Examples:
    /// - Mowing small patch: 0.03f
    /// - Purifying a pond: 0.05f
    /// </summary>
    public void RegisterSideObjectiveCompleted(string objectiveId, float sideValue01 = -1f)
    {

        if (string.IsNullOrEmpty(objectiveId) || completedMainIds.Contains(objectiveId)) return;

        float award = sideValue01 > 0f ? Mathf.Clamp01(sideValue01) : defaultSideValue01;

        completedSideIds.Add(objectiveId);
        sideCompletedCount++;
        AddProgress(award);
        Debug.Log($"[Progress] Side complete: {objectiveId} (+{award:P0})");
        //CheckWinCondition();
    }

    /// <summary>
    /// Returns overall progress (0..1).
    /// </summary>
    public float GetProgress01() => currentProgress01;

    /// <summary>
    /// Returns counts for UI or logic.
    /// </summary>
    public void GetObjectiveCounts(out int mainDone, out int sideDone)
    {
        mainDone = mainCompletedCount;
        sideDone = sideCompletedCount;
    }

    /// <summary>
    /// Did we meet the area’s minimum requirement (80%)?
    /// </summary>
    public bool HasReachedGoal() =>  currentProgress01 >= goalTarget01;

    // ------------------------------------------------------------
    // INTERNAL
    // ------------------------------------------------------------

    private void AddProgress(float delta01)
    {
        float before = currentProgress01;
        currentProgress01 = Mathf.Clamp01(currentProgress01 + delta01);

        // Drive world feedback with normalized progress
        PushProgressToWorld();

        // Optional: step-based hooks, e.g., unlocks at certain percentages
        HandleThresholds(before, currentProgress01);
    }

    private void PushProgressToWorld()
    {
        // Update environment systems safely
        if (enviroment) enviroment.SetProgress(currentProgress01);
        if (music) music.SetProgress(currentProgress01);
        if (tree) tree.SetProgress(currentProgress01);
        // If your grass/water managers expect incremental calls, keep as-is:
        if (grass) grass.AddProgress();
        if (water) water.SetProgress(); // keep/comment as needed

        Debug.Log($"[Progress] {currentProgress01:P1} | Main:{mainCompletedCount} | Side:{sideCompletedCount}");
    }

    private void HandleThresholds(float before, float after)
    {
        if (mainCompletedCount >= 2 && (after >= 0.50f) && !alreadyHit)
        {
            Debug.Log("Complete side quests " + QuestManager.GetCurrentQuest().GetTitle());
            RPG.Quests.Quest currentQuest = QuestManager.GetCurrentQuest();
            QuestList questList = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
            questList.CompleteObjective(currentQuest, "Naturlist Away");
            seedMiniGamePrefab.SetActive(true);
            alreadyHit = true;
        }
        else if (after >= 0.70f && before < 0.80f){ RenderSettings.fog = false;}
    }

    private void CheckWinCondition()
    {
        if (!gameIsWon && HasReachedGoal())
        {
            gameIsWon = true;
            Debug.Log("[Progress] Minimum requirement met (>= 80%). Area can be cleared.");
            // Trigger end-of-area beats, cutscenes, or open exit, etc.
        }
    }

    public float getCurrentProgress()
    {
        return currentProgress01;
    }


    //public void CheckProgress()
    //{
    //   // goalScore = spawner.getRecycableCount();
    //    float percentage = (float)currentScore / goalScore * 100f;

    //    Debug.Log($"Current Count: {currentScore}, Percentage: {percentage}%");

    //    // Update Terrain and Skybox based on progress
    //    enviroment.SetProgress(percentage / 100f); // Normalize percentage (0-1 range)
    //    music.SetProgress(percentage / 100f);
    //    grass.AddProgress(); // Updates grass growth
    //    //water.SetProgress();
    //}
}
