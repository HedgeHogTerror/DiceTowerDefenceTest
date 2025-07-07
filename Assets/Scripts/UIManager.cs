using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Game UI References")]
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI enemiesText;
    
    [Header("Wave Control")]
    [SerializeField] private Button startWaveButton;
    [SerializeField] private Slider waveProgressSlider;
    [SerializeField] private TextMeshProUGUI waveProgressText;
    
    [Header("Game State UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject gameWonPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button chaosButton;
    [SerializeField] private Button safetyButton;
    
    [Header("Tower Info Panel")]
    [SerializeField] private GameObject towerInfoPanel;
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private TextMeshProUGUI towerStatsText;
    [SerializeField] private Button sellTowerButton;
    [SerializeField] private Button upgradeTowerButton;
    
    private GameManager gameManager;
    private WaveManager waveManager;
    private Tower selectedTower;
    
    enum GameState
    {
        Initial,
        Paused,
        Playing,
        GameOver,
        GameWon
    }
    
    private void Start()
    {
        // Get references
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        waveManager = FindFirstObjectByType<WaveManager>();

        // Subscribe to events
        if (gameManager != null)
        {
            gameManager.OnLivesChanged.AddListener(UpdateLivesDisplay);
            gameManager.OnWaveChanged.AddListener(UpdateWaveDisplay);
            gameManager.OnGameOver.AddListener(ShowGameOverPanel);
            gameManager.OnGameWon.AddListener(ShowGameWonPanel);
            gameManager.OnGamePaused.AddListener(UpdatePauseState);
        }

        // Setup buttons
        SetupButtons();

        // Initialize UI
        InitializeUI();
    }
    
    private void Update()
    {
        UpdateWaveProgress();
        UpdateEnemiesDisplay();
        HandleTowerSelection();
    }
    
    private void SetupButtons()
    {
        if (startWaveButton != null)
        {
            startWaveButton.onClick.AddListener(StartNextWave);
        }
        
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        
        if (safetyButton != null)
        {
            safetyButton.onClick.AddListener(PlaySafe);
        }
        
        if (chaosButton != null)
        {
            chaosButton.onClick.AddListener(PlayChaos);
        }
    
    }
    
    private void PlaySafe()
    {
        Debug.Log("Playing safe mode...");
        // spawn dice
    }

    private void PlayChaos()
    {
        Debug.Log("Playing chaos mode...");
        // spawn dice
    }
    
    private void InitializeUI()
    {
        // Hide panels initially
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameWonPanel != null) gameWonPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (towerInfoPanel != null) towerInfoPanel.SetActive(false);

        chaosButton.gameObject.SetActive(false);
        safetyButton.gameObject.SetActive(false);

        // Initialize displays
        if (gameManager != null)
        {
            UpdateLivesDisplay(gameManager.CurrentLives);
            UpdateWaveDisplay(gameManager.CurrentWave);
        }
    }
    
    private void UpdateLivesDisplay(int lives)
    {
        if (livesText != null)
        {
            livesText.text = $"Lives: {lives}";
        }
    }
    
    private void UpdateWaveDisplay(int wave)
    {
        if (waveText != null)
        {
            Debug.Log($"Updating wave display: {wave}");
            waveText.text = $"Wave: {wave}";
        }
    }
    
    private void UpdateEnemiesDisplay()
    {
        if (enemiesText != null && waveManager != null)
        {
            enemiesText.text = $"Enemies: {waveManager.EnemiesAlive}";
        }
    }
    
    private void UpdateWaveProgress()
    {
        if (waveManager == null) return;
        
        if (waveProgressSlider != null)
        {
            float progress = waveManager.GetWaveCompletionProgress();
            waveProgressSlider.value = progress;
        }
        
        if (waveProgressText != null)
        {
            float progress = waveManager.GetWaveCompletionProgress() * 100f;
            waveProgressText.text = $"Progress: {progress:F0}%";
        }
        
        // Update start wave button
        if (startWaveButton != null)
        {
            startWaveButton.interactable = !waveManager.IsWaveInProgress;
            
            TextMeshProUGUI buttonText = startWaveButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                if (waveManager.IsWaveInProgress)
                {
                    buttonText.text = "Wave in Progress";
                }
                else if (waveManager.CurrentWaveIndex >= waveManager.TotalWaves - 1)
                {
                    buttonText.text = "All Waves Complete";
                }
                else
                {
                    buttonText.text = "Start Next Wave";
                }
            }
        }
    }
    
    private void HandleTowerSelection()
    {
        // Handle tower selection with mouse clicks
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Tower tower = hit.collider.GetComponent<Tower>();
                if (tower != null)
                {
                    SelectTower(tower);
                }
                else
                {
                    DeselectTower();
                }
            }
        }
    }
    
    private void SelectTower(Tower tower)
    {
        // Deselect previous tower
        if (selectedTower != null)
        {
            selectedTower.HideRange();
        }
        
        selectedTower = tower;
        selectedTower.ShowRange();
        
        ShowTowerInfo(tower);
    }
    
    private void DeselectTower()
    {
        if (selectedTower != null)
        {
            selectedTower.HideRange();
            selectedTower = null;
        }
        
        HideTowerInfo();
    }
    
    private void ShowTowerInfo(Tower tower)
    {
        if (towerInfoPanel == null) return;
        
        towerInfoPanel.SetActive(true);
        
        if (towerNameText != null)
        {
            towerNameText.text = tower.name.Replace("(Clone)", "");
        }
        
        if (towerStatsText != null)
        {
            towerStatsText.text = $"Damage: {tower.Damage:F1}\n" +
                                 $"Range: {tower.Range:F1}\n" +
                                 $"Fire Rate: {tower.FireRate:F1}";
        }
    }
    
    private void HideTowerInfo()
    {
        if (towerInfoPanel != null)
        {
            towerInfoPanel.SetActive(false);
        }
    }
    
    private void StartNextWave()
    {
        Debug.Log("Starting next wave...");
        if (gameManager != null)
        {
            gameManager.StartNextWave();
        }
    }
    
    public void RestartGame()
    {
        Debug.Log("Restarting game...");
        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
    }
    
    private void TogglePause()
    {
        Debug.Log("Toggling pause...");
        if (gameManager != null)
        {
            gameManager.TogglePause();
        }
    }

    private void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
    
    private void ShowGameWonPanel()
    {
        if (gameWonPanel != null)
        {
            gameWonPanel.SetActive(true);
        }
    }
    
    private void UpdatePauseState(bool isPaused)
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (gameManager != null)
        {
            gameManager.OnLivesChanged.RemoveListener(UpdateLivesDisplay);
            gameManager.OnWaveChanged.RemoveListener(UpdateWaveDisplay);
            gameManager.OnGameOver.RemoveListener(ShowGameOverPanel);
            gameManager.OnGameWon.RemoveListener(ShowGameWonPanel);
            gameManager.OnGamePaused.RemoveListener(UpdatePauseState);
        }
    }
}

// Helper class for tower buttons
public class TowerButtonUI : MonoBehaviour
{
    public int towerIndex;
    public Button button;
    public Color originalColor;
}
