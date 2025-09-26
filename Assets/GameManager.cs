using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public bool isGameActive = false;
    public float gameStartTime;
    public float currentGameTime;

    [Header("Systems")]
    public WaveManager waveManager;
    public MetaProgression metaProgression;
    public LootSystem lootSystem;

    [Header("UI References")]
    public GameObject gameOverPanel;
    public GameObject victoryPanel;
    public TMPro.TextMeshProUGUI gameTimeText;
    public TMPro.TextMeshProUGUI statsText;

    private bool gameEnded = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeSystems();

        // EXPANDED DEBUG - Archie | [26/09/25]
        Debug.Log("=== GAMEMANAGER START DEBUG ===");
        Debug.Log($"TutorialManager exists: {TutorialManager.Instance != null}");

        if (TutorialManager.Instance != null)
        {
            Debug.Log($"HasCompletedTutorial: {TutorialManager.Instance.HasCompletedTutorial()}");
            Debug.Log(
                $"Tutorial steps count: {TutorialManager.Instance.tutorialSteps?.Length ?? 0}"
            );

            // TEMPORARY FIX: Reset tutorial for testing - Archie | [26/09/25]
            Debug.Log("RESETTING TUTORIAL FOR TESTING");
            TutorialManager.Instance.ResetTutorialProgress();
            Debug.Log(
                $"After reset - HasCompletedTutorial: {TutorialManager.Instance.HasCompletedTutorial()}"
            );
        }

        // === TUTORIAL INTEGRATION ===
        if (TutorialManager.Instance != null && !TutorialManager.Instance.HasCompletedTutorial())
        {
            Debug.Log("STARTING TUTORIAL NOW");
            TutorialManager.Instance.StartTutorial();
            return;
        }

        Debug.Log("BYPASSING TUTORIAL - starting actual game");
        StartActualGame();
    }

    void Update()
    {
        if (isGameActive && !gameEnded)
        {
            currentGameTime = Time.time - gameStartTime;
            UpdateGameTimeUI();
        }
    }

    // --- Finding or creating systems. ---
    void InitializeSystems()
    {
        // Subscribing to tutorial completion event - Archie | [25/09/25].
        // Need to know when tutorial is completed so game can start.
        // TODO: Consider checking for tutorial completion in other places.
        if (TutorialManager.Instance != null)
        {
            TutorialManager.OnTutorialCompleted += OnTutorialCompleted;
        }

        waveManager = FindObjectOfType<WaveManager>();
        if (waveManager == null)
        {
            GameObject waveManagerObj = new GameObject("WaveManager");
            waveManager = waveManagerObj.AddComponent<WaveManager>();
        }

        metaProgression = FindObjectOfType<MetaProgression>();
        if (metaProgression == null)
        {
            GameObject metaProgObj = new GameObject("MetaProgression");
            metaProgression = metaProgObj.AddComponent<MetaProgression>();
        }

        lootSystem = FindObjectOfType<LootSystem>();
        if (lootSystem == null)
        {
            GameObject lootSystemObj = new GameObject("LootSystem");
            lootSystem = lootSystemObj.AddComponent<LootSystem>();
        }

        // Subscribe to events
        if (waveManager != null)
        {
            waveManager.OnAllWavesComplete += OnAllWavesComplete;
        }

        if (metaProgression != null)
        {
            metaProgression.OnFeatureUnlocked += OnFeatureUnlocked;
        }
    }

    // Tutorial Completion Handler - Archie | [25/09/25].
    void OnTutorialCompleted()
    {
        Debug.Log("Tutorial completed, starting actual game.");
        // Now starting real game and ending tutorial.
        StartActualGame(); // Renamed 'StartNewGame' to 'StartActualGame' to avoid confusion.
    }

    void StartActualGame()
    {
        isGameActive = true;
        gameEnded = false;
        gameStartTime = Time.time;
        currentGameTime = 0f;

        // Notify meta progression
        if (metaProgression != null)
        {
            metaProgression.StartActualGame(); // Changed to StartActualGame so it's more descriptive - Archie | [25/09/25].
        }

        // Hide UI panels
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        Debug.Log("New game started!");
    }

    void UpdateGameTimeUI()
    {
        if (gameTimeText != null)
        {
            int minutes = Mathf.FloorToInt(currentGameTime / 60f);
            int seconds = Mathf.FloorToInt(currentGameTime % 60f);
            gameTimeText.text = string.Format("Time: {0:00}:{1:00}", minutes, seconds);
        }
    }

    void OnAllWavesComplete()
    {
        GameVictory();
    }

    void GameVictory()
    {
        if (gameEnded)
            return;

        gameEnded = true;
        isGameActive = false;

        // Record game completion
        if (metaProgression != null)
        {
            metaProgression.EndGame(currentGameTime);
        }

        // Show victory UI
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            UpdateVictoryStats();
        }

        Debug.Log("Victory! All waves completed!");
    }

    public void GameOver()
    {
        if (gameEnded)
            return;

        gameEnded = true;
        isGameActive = false;

        // Record game completion
        if (metaProgression != null)
        {
            metaProgression.EndGame(currentGameTime);
        }

        // Show game over UI
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            UpdateGameOverStats();
        }

        Debug.Log("Game Over!");
    }

    void UpdateVictoryStats()
    {
        if (statsText != null && metaProgression != null)
        {
            MetaProgressionData data = metaProgression.GetData();
            statsText.text =
                $"Victory!\n"
                + $"Time: {FormatTime(currentGameTime)}\n"
                + $"Total Games: {data.totalGamesPlayed}\n"
                + $"Best Time: {FormatTime(data.bestGameTime)}\n"
                + $"Meta Currency: {data.metaCurrency}";
        }
    }

    void UpdateGameOverStats()
    {
        if (statsText != null && metaProgression != null)
        {
            MetaProgressionData data = metaProgression.GetData();
            statsText.text =
                $"Game Over!\n"
                + $"Time: {FormatTime(currentGameTime)}\n"
                + $"Waves Completed: {data.totalWavesCompleted}\n"
                + $"Enemies Killed: {data.totalEnemiesKilled}\n"
                + $"Meta Currency: {data.metaCurrency}";
        }
    }

    string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void OnFeatureUnlocked(string featureName)
    {
        Debug.Log($"New feature unlocked: {featureName}");

        // You can add UI notifications here
        // For example, show a popup or play a sound
    }

    // UI Button Methods
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // Make sure you have a MainMenu scene
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // Debug methods
    [ContextMenu("Add Test Meta Currency")]
    void AddTestMetaCurrency()
    {
        if (metaProgression != null)
        {
            metaProgression.AddMetaCurrency(500);
        }
    }

    [ContextMenu("Complete All Waves")]
    void CompleteAllWaves()
    {
        if (waveManager != null)
        {
            waveManager.currentWave = waveManager.maxWaves;
            waveManager.StopCurrentWave();
            OnAllWavesComplete();
        }
    }

    [ContextMenu("Trigger Game Over")]
    void TriggerGameOver()
    {
        GameOver();
    }
}
