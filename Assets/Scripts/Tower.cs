using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Tower : MonoBehaviour
{
    [Header("Tower Stats")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float range = 5f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private int cost = 50;

    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;
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
            if (enemy != null)
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

    private void LookAtTarget()
    {
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    private void Fire()
    {
        if (target == null || projectilePrefab == null) return;

        Transform spawnPoint = firePoint != null ? firePoint : transform;
        GameObject projectileObj = Instantiate(projectilePrefab, spawnPoint.position, spawnPoint.rotation);

        Projectile projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.SetTarget(target);
            projectile.SetDamage(damage);
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
