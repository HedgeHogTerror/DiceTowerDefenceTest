using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private int rewardValue = 10;
    [SerializeField] private int damage = 1;
    
    [Header("References")]
    [SerializeField] private Transform[] waypoints;
    
    private int currentWaypointIndex = 0;
    private Health health;
    private GameManager gameManager;
    
    public float MoveSpeed => moveSpeed;
    public int RewardValue => rewardValue;
    public int Damage => damage;
    
    private void Start()
    {
        health = GetComponent<Health>();
        gameManager = FindObjectOfType<GameManager>();
        
        if (health != null)
        {
            health.OnDeath.AddListener(OnEnemyDeath);
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
    }
    
    private void Update()
    {
        Move();
    }
    
    private void Move()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        
        if (currentWaypointIndex < waypoints.Length)
        {
            Vector3 targetPosition = waypoints[currentWaypointIndex].position;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            
            // Look at the target
            Vector3 direction = (targetPosition - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
            
            // Check if reached waypoint
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                currentWaypointIndex++;
                
                // If reached the end, damage player and destroy enemy
                if (currentWaypointIndex >= waypoints.Length)
                {
                    ReachEnd();
                }
            }
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
    
    private void OnEnemyDeath()
    {
        if (gameManager != null)
        {
            gameManager.AddMoney(rewardValue);
        }
        
        Destroy(gameObject);
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
}
