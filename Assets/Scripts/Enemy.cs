using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.UI; // Added for UI components

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private int rewardValue = 10;
    [SerializeField] private int damage = 1;
    [SerializeField] private int corpseDissapearTime = 3;
    
    [Header("References")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private Animator goblinAnimation;
    [SerializeField] private GameObject healthBarPrefab; // Health bar prefab reference
    
    private int currentWaypointIndex = 0;
    private Health health;
    private GameManager gameManager;
    private NavMeshAgent agent;
    public bool isDead = false;
    
    public float MoveSpeed => moveSpeed;
    public int RewardValue => rewardValue;
    public int Damage => damage;
    
    private GameObject healthBarInstance;
    private Slider healthSlider; // or Image if using Image fill

    private void Start()
    {
        health = GetComponent<Health>();
        gameManager = FindFirstObjectByType<GameManager>();
        agent = GetComponent<NavMeshAgent>();
        goblinAnimation = GetComponentInChildren<Animator>();
        goblinAnimation.SetTrigger("Alive");
        
        if (health != null)
        {
            health.OnDeath.AddListener(OnEnemyDeath);
            health.OnHealthChanged.AddListener(UpdateHealthBar);
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

        if (waypoints != null && waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[0].position);
        }

        // Instantiate health bar
        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform.position, Quaternion.identity);
            healthSlider = healthBarInstance.GetComponent<Slider>();
            healthSlider.maxValue = health.MaxHealth;
            healthSlider.value = health.CurrentHealth;
        }
    }
    
    private void Update()
    {
        if (isDead)
        {
            agent.isStopped = true;
            return;
        }
        if (waypoints != null && currentWaypointIndex < waypoints.Length)
        {
            if (!agent.pathPending && agent.remainingDistance < 0.1f)
            {
                currentWaypointIndex++;
                if (currentWaypointIndex < waypoints.Length)
                    agent.SetDestination(waypoints[currentWaypointIndex].position);
                else
                    ReachEnd();
            }
        }

        // Update health bar position and value
        if (healthBarInstance != null)
        {
            healthBarInstance.transform.position = transform.position + Vector3.up; // Adjust as necessary
            healthSlider.value = health.CurrentHealth;
        }
    }
    
    private void ReachEnd()
    {
        if (gameManager != null)
        {
            gameManager.TakeDamage(damage);
        }
        
        Destroy(gameObject);
    }
    

    private IEnumerator CallAfterDelay(GameObject enemy)
    {
        yield return new WaitForSeconds(corpseDissapearTime); // Use the serialized field
        Destroy(enemy);
    }
    
    private void OnEnemyDeath()
    {
        isDead = true;
        agent.isStopped = true;
        DisableCollisions();

        goblinAnimation.SetTrigger("Died");
        
        // Destroy health bar on death
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }
        
        StartCoroutine(CallAfterDelay(gameObject)); ;
    }
    private void DisableCollisions()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;
    }
    
    public void SetWaypoints(Transform[] newWaypoints)
    {
        waypoints = newWaypoints;
        currentWaypointIndex = 0;
    }
    
    public void SetStats(float speed, int reward, int damageAmount)
    {
        moveSpeed = speed;
        rewardValue = reward;
        damage = damageAmount;
    }

    private void UpdateHealthBar(float normalizedHealth)
    {
        if (healthSlider != null)
            healthSlider.value = normalizedHealth;
    }
}
