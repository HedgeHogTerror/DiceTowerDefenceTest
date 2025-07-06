using UnityEngine;
using System.Collections;

public class ProjectileD14 : ProjectileBase
{
    [Header("D14 Laser Beam Settings")]
    [SerializeField] private float beamDuration = 3f;
    [SerializeField] private float damagePerSecond = 15f;
    [SerializeField] private float homingStrength = 8000f;
    [SerializeField] private float maxTurnRate = 360f; // degrees per second
    [SerializeField] private float beamWidth = 0.2f;
    [SerializeField] private LayerMask enemyLayerMask = -1;
    [SerializeField] private int particleCount = 1;
    
    [Header("Visual Effects")]
    [SerializeField] private LineRenderer laserLine;
    [SerializeField] private GameObject beamStartEffect;
    [SerializeField] private GameObject beamMiddleEffect;
    [SerializeField] private GameObject beamEndEffect;
    [SerializeField] private Material laserMaterial;
    
    private bool isBeamActive = false;
    private float beamTimer = 0f;
    private Vector3 velocity;
    private Transform currentTarget;
    private Coroutine damageCoroutine;
    
    protected override void Start()
    {
        base.Start();
        
        // Initialize velocity in forward direction
        velocity = transform.forward * speed;
        
        // Setup laser line renderer
        SetupLaserLine();
    
    }
    
    protected override void UpdateProjectile()
    {
        if (!isBeamActive) return;
                
        // Update target tracking
        UpdateTargetTracking();
        
        // Move the laser beam
        UpdateMovement();
        
        // Update laser visual
        UpdateLaserVisual();
    }
    
    private void SetupLaserLine()
    {
        if (laserLine == null)
        {
            laserLine = gameObject.AddComponent<LineRenderer>();
        }
        
        laserLine.positionCount = 2;
        laserLine.startWidth = beamWidth;
        laserLine.endWidth = beamWidth;
        laserLine.useWorldSpace = true;

        if (laserMaterial != null)
        {
            laserLine.material = laserMaterial;
        }
        else
        {
            // Create a simple glowing material
            laserLine.material = new Material(Shader.Find("Sprites/Default"));
            laserLine.material.color = Color.red;
        }
    }
    
    private void StartLaserBeam()
    {
        isBeamActive = true;
        beamTimer = 0f;
        
        // Spawn start effect
        if (beamStartEffect != null)
        {
            Instantiate(beamStartEffect, transform.position, transform.rotation);
        }
         if (beamMiddleEffect != null)
        {
            SpawnParticlesAlongLine();
        }
        
        // Start damage over time
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
        }
        damageCoroutine = StartCoroutine(DamageOverTime());
    }
    
    private void SpawnParticlesAlongLine()
    {
        if (laserLine == null || beamMiddleEffect == null) return;

        Vector3 startPos = laserLine.GetPosition(0);
        Vector3 endPos = laserLine.GetPosition(1);

        for (int i = 0; i <= particleCount; i++)
        {
            float t = (float)i / particleCount;
            Vector3 pos = Vector3.Lerp(startPos, endPos, t);
            Instantiate(beamMiddleEffect, pos, Quaternion.identity);
        }
    }
    
    private void UpdateTargetTracking()
    {
        // Find the best target to track
        if (currentTarget == null || !IsTargetStillValid(currentTarget))
        {
            FindNewTarget();
        }

        if (currentTarget != null)
        {
            target = currentTarget;
            targetPosition = currentTarget.position;
            hasTarget = true;
        }
    }

    private void FindNewTarget()
    {
        // Look for enemies in a wider radius for laser beam
        Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, 15f, enemyLayerMask);

        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (Collider col in nearbyEnemies)
        {
            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy != null && !enemy.isDead)
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = col.transform;
                }
            }
        }

        currentTarget = closestEnemy;
        StartLaserBeam();
    }
    
    private bool IsTargetStillValid(Transform target)
    {
        if (target == null) return false;
        
        Enemy enemy = target.GetComponent<Enemy>();
        return enemy != null && !enemy.isDead;
    }
    
    private void UpdateMovement()
    {
        if (hasTarget && currentTarget != null)
        {
            // Calculate desired direction to target
            Vector3 desiredDirection = (targetPosition - transform.position).normalized;
            
            // Calculate how much we can turn this frame
            float maxTurnAngle = maxTurnRate * Time.deltaTime;
            
            // Smoothly turn towards target
            Vector3 currentDirection = velocity.normalized;
            Vector3 newDirection = Vector3.RotateTowards(currentDirection, desiredDirection, 
                maxTurnAngle * Mathf.Deg2Rad, 0f);
            
            // Apply homing strength
            newDirection = Vector3.Slerp(currentDirection, newDirection, homingStrength * Time.deltaTime);
            
            // Update velocity
            velocity = newDirection * speed;
        }
        
        // Move the laser beam
        transform.position += velocity * Time.deltaTime;
        
        // Rotate to face movement direction
        if (velocity != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(velocity);
        }
    }

    private void UpdateLaserVisual()
    {
        if (laserLine == null) return;

        Vector3 startPos = transform.position;
        Vector3 endPos;

        if (hasTarget && currentTarget != null)
        {
            endPos = currentTarget.position;
            laserLine.SetPosition(0, startPos);
            laserLine.SetPosition(1, endPos);
        }
        else
        {
            EndLaserBeam();
        }
    }
    
    private IEnumerator DamageOverTime()
    {
        while (isBeamActive && beamTimer < beamDuration)
        {
            // Deal damage to all enemies the laser is touching
            DealLaserDamage();
            
            yield return new WaitForSeconds(0.1f); // Damage 10 times per second
        }
    }
    
    private void DealLaserDamage()
    {
        if (laserLine == null) return;
        
        Vector3 startPos = laserLine.GetPosition(0);
        Vector3 endPos = laserLine.GetPosition(1);
        Vector3 direction = (endPos - startPos).normalized;
        float distance = Vector3.Distance(startPos, endPos);
        
        // Raycast along the laser beam
        RaycastHit[] hits = Physics.RaycastAll(startPos, direction, distance, enemyLayerMask);
        
        foreach (RaycastHit hit in hits)
        {
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null && !enemy.isDead)
            {
                Health enemyHealth = hit.collider.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    float damageThisFrame = damagePerSecond * 0.1f; // 0.1 second intervals
                    enemyHealth.TakeDamage(damageThisFrame);
                }
            } else {
                EndLaserBeam();
            }
        }
    }
    
    private void EndLaserBeam()
    {
        isBeamActive = false;
        
        // Stop damage coroutine
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
        }
        
        // Spawn end effect
        if (beamEndEffect != null)
        {
            Instantiate(beamEndEffect, transform.position, transform.rotation);
        }
        
        // Destroy the projectile
        DestroyProjectile();
    }
    
    protected override void HitTarget(Transform hitTarget = null)
    {
        // Laser beam doesn't use traditional hit detection
        // Damage is handled by DamageOverTime coroutine
    }
    
    protected override void OnTriggerEnter(Collider other)
    {
        // Laser beam doesn't use trigger collision
        // Damage is handled by raycast in DealLaserDamage
    }
    
    public override void SetDamage(float newDamage)
    {
        base.SetDamage(newDamage);
        // Convert total damage to damage per second
        damagePerSecond = newDamage / beamDuration;
    }
    
    protected override void DestroyProjectile()
    {
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
        }
        
        base.DestroyProjectile();
    }
}
