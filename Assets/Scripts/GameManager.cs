using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int startingLives = 20;
    
    [Header("Current Game State")]
    [SerializeField] private int currentLives;
    [SerializeField] private int currentWave = 0;
    [SerializeField] private int enemiesKilled = 0;
    [SerializeField] private bool gameOver = false;
    [SerializeField] private bool gamePaused = false;
    
    [Header("Events")]
    public UnityEvent<int> OnMoneyChanged;
    public UnityEvent<int> OnLivesChanged;
    public UnityEvent<int> OnWaveChanged;
    public UnityEvent OnGameOver;
    public UnityEvent OnGameWon;
    public UnityEvent<bool> OnGamePaused;
    
    [Header("References")]
    [SerializeField] private WaveManager waveManager;
    
    // Singleton pattern
    public static GameManager Instance { get; private set; }
    
    // Properties
    public int CurrentLives => currentLives;
    public int CurrentWave => currentWave;
    public int EnemiesKilled => enemiesKilled;
    public bool IsGameOver => gameOver;
    public bool IsGamePaused => gamePaused;
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        InitializeGame();
    }
    
    private void Update()
    {
        // Handle pause input
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
    
    private void InitializeGame()
    {
        currentLives = startingLives;
        currentWave = 0;
        enemiesKilled = 0;
        gameOver = false;
        gamePaused = false;
        
        // Find wave manager if not assigned
        if (waveManager == null)
        {
            waveManager = FindObjectOfType<WaveManager>();
        }
        
        // Trigger initial events
        OnLivesChanged?.Invoke(currentLives);
        OnWaveChanged?.Invoke(currentWave);
    }
    
    public void TakeDamage(int damage)
    {
        if (gameOver) return;
        
        currentLives -= damage;
        OnLivesChanged?.Invoke(currentLives);
        
        if (currentLives <= 0)
        {
            GameOver();
        }
    }
    
    public void AddLives(int amount)
    {
        if (gameOver) return;
        
        currentLives += amount;
        OnLivesChanged?.Invoke(currentLives);
    }
    
    public void EnemyKilled()
    {
        if (gameOver) return;
        
        enemiesKilled++;
    }
    
    public void StartNextWave()
    {
        if (gameOver) return;
        
        currentWave++;
        OnWaveChanged?.Invoke(currentWave);
        
        if (waveManager != null)
        {
            waveManager.StartWave(currentWave);
        }
    }
    
    public void WaveCompleted()
    {
        // Check if all waves are completed
        if (waveManager != null && currentWave >= waveManager.TotalWaves)
        {
            GameWon();
        }
    }

    private void GameOver()
    {
        gameOver = true;
        Time.timeScale = 0f; // Pause the game
        OnGameOver?.Invoke();
        StartCoroutine(DelayedRestart(3f));
    }

    private IEnumerator DelayedRestart(float delay)
    {
        yield return new WaitForSeconds(delay);
        RestartGame();
    }
    
    private void GameWon()
    {
        gameOver = true;
        Time.timeScale = 0f; // Pause the game
        OnGameWon?.Invoke();
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f; // Resume time
        
        // Reset all values
        InitializeGame();
        
        // Reload the scene or reset game state
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    
    public void TogglePause()
    {
        if (gameOver) return;
        
        gamePaused = !gamePaused;
        Time.timeScale = gamePaused ? 0f : 1f;
        OnGamePaused?.Invoke(gamePaused);
    }
    
    public void PauseGame()
    {
        if (gameOver) return;
        
        gamePaused = true;
        Time.timeScale = 0f;
        OnGamePaused?.Invoke(gamePaused);
    }
    
    public void ResumeGame()
    {
        if (gameOver) return;
        
        gamePaused = false;
        Time.timeScale = 1f;
        OnGamePaused?.Invoke(gamePaused);
    }
    
    // Save/Load functionality (basic implementation)
    [System.Serializable]
    public class GameData
    {
        public int lives;
        public int wave;
        public int kills;
    }
    
    public GameData GetGameData()
    {
        return new GameData
        {
            lives = currentLives,
            wave = currentWave,
            kills = enemiesKilled
        };
    }
    
    public void LoadGameData(GameData data)
    {
        currentLives = data.lives;
        currentWave = data.wave;
        enemiesKilled = data.kills;
        
        OnLivesChanged?.Invoke(currentLives);
        OnWaveChanged?.Invoke(currentWave);
    }
}
