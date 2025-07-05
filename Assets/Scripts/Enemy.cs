using UnityEngine;
using UnityEngine.AI;
using System.Collections;

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
    
    private int currentWaypointIndex = 0;
    private Health health;
    private GameManager gameManager;
    private NavMeshAgent agent;
    public bool isDead = false;
    
    public float MoveSpeed => moveSpeed;
    public int RewardValue => rewardValue;
    public int Damage => damage;
    
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
        if (gameManager != null)
        {
            gameManager.AddMoney(rewardValue);
        }
        goblinAnimation.SetTrigger("Died");
        
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
}
