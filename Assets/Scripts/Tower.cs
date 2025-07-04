using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Tower : MonoBehaviour
{
    public enum TowerType
    {
        d4,
        d6,
        d8,
        d12,
        d14
}

    [Header("Tower Stats")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float range = 5f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private int cost = 50;
    [SerializeField] private TowerType towerType; 

    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject[] projectilePrefabs; // Array of projectile prefabs for each tower type
    [SerializeField] private LineRenderer rangeIndicator;

    [Header("Targeting")]
    [SerializeField] private LayerMask enemyLayerMask = -1;

    private float nextFireTime = 0f;
    private Transform target;
    private List<Transform> enemiesInRange = new List<Transform>();

    public float Damage => damage;
    public float Range => range;
    public float FireRate => fireRate;
    public int Cost => cost;
    public Transform Target => target;

    private void Start()
    {
        // Create range indicator if not assigned
        if (rangeIndicator == null)
        {
            CreateRangeIndicator();
        }

        // Hide range indicator by default
        if (rangeIndicator != null)
        {
            rangeIndicator.enabled = false;
        }

        // Start targeting coroutine
        StartCoroutine(UpdateTargeting());
    }

    private void Update()
    {
        if (target == null)
        {
            return;
        }

        // Check if target is still in range
        if (Vector3.Distance(transform.position, target.position) > range)
        {
            target = null;
            return;
        }

        // Rotate towards target
        // comment for now, causes issues with stability
        // LookAtTarget(); 



        // Fire at target
        if (Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    private IEnumerator UpdateTargeting()
    {
        while (true)
        {
            UpdateEnemiesInRange();
            SelectTarget();
            yield return new WaitForSeconds(0.1f); // Update targeting 10 times per second
        }
    }

    private void UpdateEnemiesInRange()
    {
        enemiesInRange.Clear();

        Collider[] enemiesFound = Physics.OverlapSphere(transform.position, range, enemyLayerMask);

        foreach (Collider enemyCollider in enemiesFound)
        {
            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null && !enemy.isDead)
            {
                enemiesInRange.Add(enemyCollider.transform);
            }
        }
    }

    private void SelectTarget()
    {
        if (enemiesInRange.Count == 0)
        {
            target = null;
            return;
        }

        // Target the closest enemy (you can implement different targeting strategies)
        Transform closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (Transform enemy in enemiesInRange)
        {
            if (enemy == null) continue;

            float distance = Vector3.Distance(transform.position, enemy.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        target = closestEnemy;
    }

    // private void LookAtTarget()
    // {
    //     if (target == null) return;

    //     Vector3 direction = (target.position - transform.position).normalized;
    //     Quaternion lookRotation = Quaternion.LookRotation(direction);
    //     transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    // }

    private void Fire()
    {
        if (target == null) return;

        // Get the appropriate projectile prefab for this tower type
        GameObject projectilePrefab = GetProjectilePrefab();
        if (projectilePrefab == null) return;

        Transform spawnPoint = firePoint != null ? firePoint : transform;
        GameObject projectileObj = Instantiate(projectilePrefab, spawnPoint.position, spawnPoint.rotation);
        
        // Try to set target and damage using reflection to work with any projectile type
        MonoBehaviour[] projectileComponents = projectileObj.GetComponents<MonoBehaviour>();
        bool projectileConfigured = false;
        
        foreach (MonoBehaviour component in projectileComponents)
        {
            // Check if this component has SetTarget and SetDamage methods
            var setTargetMethod = component.GetType().GetMethod("SetTarget");
            var setDamageMethod = component.GetType().GetMethod("SetDamage");
            
            if (setTargetMethod != null && setDamageMethod != null)
            {
                setTargetMethod.Invoke(component, new object[] { target });
                setDamageMethod.Invoke(component, new object[] { damage });
                projectileConfigured = true;
                break;
            }
        }
        
        if (!projectileConfigured)
        {
            Debug.LogWarning($"Tower {gameObject.name}: Could not configure projectile {projectileObj.name}. Make sure it has SetTarget and SetDamage methods.");
        }
    }

    private GameObject GetProjectilePrefab()
    {
        // Return the appropriate projectile prefab based on tower type
        if (projectilePrefabs == null || projectilePrefabs.Length == 0)
        {
            Debug.LogWarning($"Tower {gameObject.name}: No projectile prefabs assigned!");
            return null;
        }

        int towerTypeIndex = (int)towerType;
        
        // Make sure the index is valid
        if (towerTypeIndex >= 0 && towerTypeIndex < projectilePrefabs.Length)
        {
            return projectilePrefabs[towerTypeIndex];
        }
        else
        {
            Debug.LogWarning($"Tower {gameObject.name}: Invalid tower type index {towerTypeIndex} for projectile array of length {projectilePrefabs.Length}");
            // Return first available projectile as fallback
            return projectilePrefabs[0];
        }
    }

    private void CreateRangeIndicator()
    {
        GameObject rangeObj = new GameObject("RangeIndicator");
        rangeObj.transform.SetParent(transform);
        rangeObj.transform.localPosition = Vector3.zero;

        rangeIndicator = rangeObj.AddComponent<LineRenderer>();
        rangeIndicator.material = new Material(Shader.Find("Sprites/Default"));
        rangeIndicator.startColor = Color.red;
        rangeIndicator.startWidth = 0.1f;
        rangeIndicator.endWidth = 0.1f;
        rangeIndicator.useWorldSpace = false;

        // Create circle points
        int segments = 64;
        rangeIndicator.positionCount = segments + 1;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * range, 0, Mathf.Sin(angle) * range);
            rangeIndicator.SetPosition(i, pos);
        }
    }

    public void ShowRange()
    {
        if (rangeIndicator != null)
        {
            rangeIndicator.enabled = true;
        }
    }

    public void HideRange()
    {
        if (rangeIndicator != null)
        {
            rangeIndicator.enabled = false;
        }
    }

    public void UpgradeDamage(float damageIncrease)
    {
        damage += damageIncrease;
    }

    public void UpgradeRange(float rangeIncrease)
    {
        range += rangeIncrease;

        // Update range indicator
        if (rangeIndicator != null)
        {
            CreateRangeIndicator();
        }
    }

    public void UpgradeFireRate(float fireRateIncrease)
    {
        fireRate += fireRateIncrease;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw range in scene view
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
    
    public Tower GetTowerOnTop(float maxDistance = 1f)
    {
        // Start from the top of this tower's collider (or just above its position)
        Vector3 origin = transform.position + Vector3.up * 0.6f; // Adjust 0.6f to your tower's height/offset
        Ray ray = new Ray(origin, Vector3.up);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            Tower towerAbove = hit.collider.GetComponent<Tower>();
            if (towerAbove != null && towerAbove != this)
            {
                return towerAbove;
            }
        }
        return null;
    }
}
