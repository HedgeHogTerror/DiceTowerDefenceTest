using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Game UI References")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI enemiesText;
    
    [Header("Wave Control")]
    [SerializeField] private Button startWaveButton;
    [SerializeField] private Slider waveProgressSlider;
    [SerializeField] private TextMeshProUGUI waveProgressText;
    
    [Header("Tower Placement UI")]
    [SerializeField] private Transform towerButtonContainer;
    [SerializeField] private Button towerButtonPrefab;
    
    [Header("Game State UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject gameWonPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    
    [Header("Tower Info Panel")]
    [SerializeField] private GameObject towerInfoPanel;
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private TextMeshProUGUI towerStatsText;
    [SerializeField] private Button sellTowerButton;
    [SerializeField] private Button upgradeTowerButton;
    
    private GameManager gameManager;
    private WaveManager waveManager;
    private TowerPlacer towerPlacer;
    private Tower selectedTower;
    
    private void Start()
    {
        // Get references
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
        
        waveManager = FindFirstObjectByType<WaveManager>();
        towerPlacer = FindFirstObjectByType<TowerPlacer>();
        
        // Subscribe to events
        if (gameManager != null)
        {
            gameManager.OnMoneyChanged.AddListener(UpdateMoneyDisplay);
            gameManager.OnLivesChanged.AddListener(UpdateLivesDisplay);
            gameManager.OnWaveChanged.AddListener(UpdateWaveDisplay);
            gameManager.OnGameOver.AddListener(ShowGameOverPanel);
            gameManager.OnGameWon.AddListener(ShowGameWonPanel);
            gameManager.OnGamePaused.AddListener(UpdatePauseState);
        }
        
        // Setup buttons
        SetupButtons();
        
        // Create tower placement buttons
        CreateTowerButtons();
        
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
        
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(TogglePause);
        }
        
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(TogglePause);
        }
        
        if (sellTowerButton != null)
        {
            sellTowerButton.onClick.AddListener(SellSelectedTower);
        }
        
        if (upgradeTowerButton != null)
        {
            upgradeTowerButton.onClick.AddListener(UpgradeSelectedTower);
        }
    }
    
    private void CreateTowerButtons()
    {
        if (towerPlacer == null || towerButtonContainer == null || towerButtonPrefab == null)
        {
            return;
        }
        
        for (int i = 0; i < towerPlacer.GetTowerCount(); i++)
        {
            int towerIndex = i; // Capture for closure
            GameObject towerPrefab = towerPlacer.GetTowerPrefab(i);
            
            if (towerPrefab != null)
            {
                Button towerButton = Instantiate(towerButtonPrefab, towerButtonContainer);
                Tower towerComponent = towerPrefab.GetComponent<Tower>();
                
                // Setup button text
                TextMeshProUGUI buttonText = towerButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null && towerComponent != null)
                {
                    buttonText.text = $"Tower {i + 1}\n${towerComponent.Cost}";
                }
                
                // Setup button click
                towerButton.onClick.AddListener(() => SelectTowerForPlacement(towerIndex));
                
                // Store reference for updating affordability
                TowerButtonUI buttonUI = towerButton.gameObject.AddComponent<TowerButtonUI>();
                buttonUI.towerIndex = towerIndex;
                buttonUI.button = towerButton;
                buttonUI.originalColor = towerButton.colors.normalColor;
            }
        }
    }
    
    private void InitializeUI()
    {
        // Hide panels initially
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameWonPanel != null) gameWonPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (towerInfoPanel != null) towerInfoPanel.SetActive(false);
        
        // Initialize displays
        if (gameManager != null)
        {
            UpdateMoneyDisplay(gameManager.CurrentMoney);
            UpdateLivesDisplay(gameManager.CurrentLives);
            UpdateWaveDisplay(gameManager.CurrentWave);
        }
    }
    
    private void UpdateMoneyDisplay(int money)
    {
        if (moneyText != null)
        {
            moneyText.text = $"Money: ${money}";
        }
        
        // Update tower button affordability
        UpdateTowerButtonAffordability();
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
    
    private void UpdateTowerButtonAffordability()
    {
        if (towerPlacer == null || gameManager == null) return;
        
        TowerButtonUI[] towerButtons = FindObjectsOfType<TowerButtonUI>();
        foreach (TowerButtonUI buttonUI in towerButtons)
        {
            bool canAfford = towerPlacer.CanAffordTower(buttonUI.towerIndex);
            
            ColorBlock colors = buttonUI.button.colors;
            colors.normalColor = canAfford ? buttonUI.originalColor : Color.gray;
            buttonUI.button.colors = colors;
            buttonUI.button.interactable = canAfford;
        }
    }
    
    private void HandleTowerSelection()
    {
        // Handle tower selection with mouse clicks
        if (Input.GetMouseButtonDown(0) && !towerPlacer.IsPlacingTower)
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
    
    private void SelectTowerForPlacement(int towerIndex)
    {
        if (towerPlacer != null)
        {
            towerPlacer.SelectTower(towerIndex);
        }
    }
    
    private void StartNextWave()
    {
        Debug.Log("Starting next wave...");
        if (waveManager != null)
        {
            waveManager.ForceStartNextWave();
        }
    }
    
    private void RestartGame()
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
    
    private void SellSelectedTower()
    {
        if (selectedTower != null && gameManager != null)
        {
            // Give back half the tower cost
            int sellValue = selectedTower.Cost / 2;
            gameManager.AddMoney(sellValue);
            
            Destroy(selectedTower.gameObject);
            DeselectTower();
        }
    }
    
    private void UpgradeSelectedTower()
    {
        if (selectedTower != null && gameManager != null)
        {
            int upgradeCost = selectedTower.Cost / 2;
            
            if (gameManager.SpendMoney(upgradeCost))
            {
                selectedTower.UpgradeDamage(selectedTower.Damage * 0.5f);
                selectedTower.UpgradeRange(selectedTower.Range * 0.2f);
                selectedTower.UpgradeFireRate(selectedTower.FireRate * 0.3f);
                
                ShowTowerInfo(selectedTower); // Refresh display
            }
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
            gameManager.OnMoneyChanged.RemoveListener(UpdateMoneyDisplay);
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
