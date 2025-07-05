using UnityEngine;

public class ProjectileD4 : ProjectileBase
{
    [Header("D4 Homing Settings")]
    [SerializeField] private float homingStrength = 5f;
    [SerializeField] private float maxTurnRate = 180f; // degrees per second
    [SerializeField] private float homingDelay = 0.1f; // delay before homing starts
    
    private float homingTimer = 0f;
    private Vector3 velocity;
    
    protected override void Start()
    {
        base.Start();
        
        // Initialize velocity in forward direction
        velocity = transform.forward * speed;
    }
    
    protected override void UpdateProjectile()
    {
        homingTimer += Time.deltaTime;
        
        if (hasTarget && IsTargetValid() && homingTimer >= homingDelay)
        {
            // Update target position for moving targets
            targetPosition = target.position;
            
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
        else if (hasTarget && !IsTargetValid())
        {
            // Target is dead/invalid, find new target nearby
            FindNewTarget();
        }
        
        // Move projectile
        transform.position += velocity * Time.deltaTime;
        
        // Rotate to face movement direction
        if (velocity != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(velocity);
        }
        
        // Check if we've reached the target position (for non-homing movement)
        if (hasTarget && Vector3.Distance(transform.position, targetPosition) < 0.2f)
        {
            HitTarget();
        }
    }
    
    private void FindNewTarget()
    {
        // Look for nearby enemies to retarget
        Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, 10f);
        
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
        
        if (closestEnemy != null)
        {
            SetTarget(closestEnemy);
        }
        else
        {
            // No targets found, continue straight
            hasTarget = false;
        }
    }
    
    protected override void OnTriggerEnter(Collider other)
    {
        if (isDestroyed) return;
        
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && !enemy.isDead)
        {
            HitTarget(other.transform);
        }
    }
    
    public override void SetTarget(Transform newTarget)
    {
        base.SetTarget(newTarget);
        homingTimer = 0f; // Reset homing timer when getting new target
    }
}
