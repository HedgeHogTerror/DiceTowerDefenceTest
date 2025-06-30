using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Wave
{
    [Header("Wave Settings")]
    public string waveName = "Wave";
    public int enemyCount = 10;
    public GameObject enemyPrefab;
    public float spawnRate = 1f; // Enemies per second
    public float timeBetweenWaves = 5f;
    
    [Header("Enemy Modifications")]
    public float enemySpeedMultiplier = 1f;
    public float enemyHealthMultiplier = 1f;
    public int enemyRewardMultiplier = 1;
}

public class WaveManager : MonoBehaviour
{
    [Header("Wave Configuration")]
    [SerializeField] private Wave[] waves;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform[] waypoints;
    
    [Header("Wave State")]
    [SerializeField] private int currentWaveIndex = -1;
    [SerializeField] private bool waveInProgress = false;
    [SerializeField] private bool autoStartWaves = true;
    [SerializeField] private float waveStartDelay = 3f;
    
    [Header("Spawning")]
    [SerializeField] private int enemiesRemaining = 0;
    [SerializeField] private int enemiesAlive = 0;
    
    private GameManager gameManager;
    private Coroutine currentWaveCoroutine;
    private List<GameObject> activeEnemies = new List<GameObject>();
    
    public int TotalWaves => waves.Length;
    public int CurrentWaveIndex => currentWaveIndex;
    public bool IsWaveInProgress => waveInProgress;
    public int EnemiesRemaining => enemiesRemaining;
    public int EnemiesAlive => enemiesAlive;
    public Wave CurrentWave => currentWaveIndex >= 0 && currentWaveIndex < waves.Length ? waves[currentWaveIndex] : null;
    
    private void Start()
    {
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
        
        // Find spawn point if not assigned
        if (spawnPoint == null)
        {
            GameObject spawnObj = GameObject.Find("SpawnPoint");
            if (spawnObj != null)
            {
                spawnPoint = spawnObj.transform;
            }
        }
        
        // Find waypoints if not assigned
        if (waypoints == null || waypoints.Length == 0)
        {
            GameObject waypointContainer = GameObject.Find("Waypoints");
            if (waypointContainer != null)
            {
                waypoints = new Transform[waypointContainer.transform.childCount];
                for (int i = 0; i < waypoints.Length; i++)
                {
                    waypoints[i] = waypointContainer.transform.GetChild(i);
                }
            }
        }
        
        // Start first wave automatically if enabled
        if (autoStartWaves)
        {
            StartCoroutine(StartFirstWaveDelayed());
        }
    }
    
    private IEnumerator StartFirstWaveDelayed()
    {
        yield return new WaitForSeconds(waveStartDelay);
        StartNextWave();
    }
    
    public void StartNextWave()
    {
        if (waveInProgress) return;
        
        currentWaveIndex++;
        
        if (currentWaveIndex >= waves.Length)
        {
            // All waves completed
            if (gameManager != null)
            {
                gameManager.WaveCompleted();
            }
            return;
        }
        
        StartWave(currentWaveIndex + 1); // +1 because wave numbers are 1-indexed for display
    }
    
    public void StartWave(int waveNumber)
    {
        int waveIndex = waveNumber - 1; // Convert to 0-indexed
        
        if (waveIndex < 0 || waveIndex >= waves.Length || waveInProgress)
        {
            return;
        }
        
        currentWaveIndex = waveIndex;
        Wave wave = waves[currentWaveIndex];
        
        waveInProgress = true;
        enemiesRemaining = wave.enemyCount;
        enemiesAlive = 0;
        
        Debug.Log($"Starting {wave.waveName} - {wave.enemyCount} enemies");
        
        if (currentWaveCoroutine != null)
        {
            StopCoroutine(currentWaveCoroutine);
        }
        
        currentWaveCoroutine = StartCoroutine(SpawnWave(wave));
    }
    
    private IEnumerator SpawnWave(Wave wave)
    {
        float spawnInterval = 1f / wave.spawnRate;
        
        for (int i = 0; i < wave.enemyCount; i++)
        {
            SpawnEnemy(wave);
            enemiesRemaining--;
            
            if (i < wave.enemyCount - 1) // Don't wait after the last enemy
            {
                yield return new WaitForSeconds(spawnInterval);
            }
        }
        
        // Wait for all enemies to be defeated or reach the end
        yield return StartCoroutine(WaitForWaveCompletion());
        
        // Wave completed
        CompleteWave(wave);
    }
    
    private void SpawnEnemy(Wave wave)
    {
        if (wave.enemyPrefab == null || spawnPoint == null) return;
        
        GameObject enemyObj = Instantiate(wave.enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        activeEnemies.Add(enemyObj);
        enemiesAlive++;
        
        // Configure enemy
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy != null)
        {
            // Set waypoints
            if (waypoints != null && waypoints.Length > 0)
            {
                enemy.SetWaypoints(waypoints);
            }
            
            // Apply wave modifiers
            float newSpeed = enemy.MoveSpeed * wave.enemySpeedMultiplier;
            int newReward = enemy.RewardValue * wave.enemyRewardMultiplier;
            enemy.SetStats(newSpeed, newReward, enemy.Damage);
        }
        
        // Configure health
        Health health = enemyObj.GetComponent<Health>();
        if (health != null)
        {
            float newMaxHealth = health.MaxHealth * wave.enemyHealthMultiplier;
            health.SetMaxHealth(newMaxHealth);
            
            // Subscribe to death event
            health.OnDeath.AddListener(() => OnEnemyDeath(enemyObj));
        }
    }
    
    private void OnEnemyDeath(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
            enemiesAlive--;
            
            if (gameManager != null)
            {
                gameManager.EnemyKilled();
            }
        }
    }
    
    private IEnumerator WaitForWaveCompletion()
    {
        while (enemiesAlive > 0)
        {
            // Clean up null references (enemies that were destroyed)
            activeEnemies.RemoveAll(enemy => enemy == null);
            enemiesAlive = activeEnemies.Count;
            
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    private void CompleteWave(Wave wave)
    {
        waveInProgress = false;
        
        Debug.Log($"{wave.waveName} completed!");
        
        if (gameManager != null)
        {
            gameManager.WaveCompleted();
        }
        
        // Auto-start next wave after delay
        if (autoStartWaves && currentWaveIndex < waves.Length - 1)
        {
            StartCoroutine(StartNextWaveDelayed(wave.timeBetweenWaves));
        }
    }
    
    private IEnumerator StartNextWaveDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartNextWave();
    }
    
    public void ForceStartNextWave()
    {
        if (!waveInProgress && currentWaveIndex < waves.Length - 1)
        {
            StartNextWave();
        }
    }
    
    public void StopCurrentWave()
    {
        if (currentWaveCoroutine != null)
        {
            StopCoroutine(currentWaveCoroutine);
            currentWaveCoroutine = null;
        }
        
        waveInProgress = false;
        
        // Destroy all active enemies
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        
        activeEnemies.Clear();
        enemiesAlive = 0;
    }
    
    public float GetWaveProgress()
    {
        if (!waveInProgress || CurrentWave == null) return 0f;
        
        int totalEnemies = CurrentWave.enemyCount;
        int spawnedEnemies = totalEnemies - enemiesRemaining;
        return (float)spawnedEnemies / totalEnemies;
    }
    
    public float GetWaveCompletionProgress()
    {
        if (!waveInProgress || CurrentWave == null) return 0f;
        
        int totalEnemies = CurrentWave.enemyCount;
        int defeatedEnemies = totalEnemies - enemiesAlive;
        return (float)defeatedEnemies / totalEnemies;
    }
    
    // Editor helper methods
    private void OnValidate()
    {
        // Ensure wave names are unique
        for (int i = 0; i < waves.Length; i++)
        {
            if (string.IsNullOrEmpty(waves[i].waveName))
            {
                waves[i].waveName = $"Wave {i + 1}";
            }
        }
    }
}
