using UnityEngine;

public class ProjectileD6 : ProjectileBase
{
    [Header("D6 Standard Settings")]
    [SerializeField] private bool predictiveAiming = true;
    [SerializeField] private float predictionAccuracy = 0.8f; // 0-1, how accurate the prediction is
    
    private Vector3 lastTargetPosition;
    private Vector3 targetVelocity;
    
    protected override void Start()
    {
        base.Start();
        
        if (hasTarget && target != null)
        {
            lastTargetPosition = target.position;
        }
    }
    
    protected override void UpdateProjectile()
    {
        if (hasTarget)
        {
            if (IsTargetValid())
            {
                // Calculate target velocity for predictive aiming
                if (predictiveAiming)
                {
                    Vector3 currentTargetPos = target.position;
                    targetVelocity = (currentTargetPos - lastTargetPosition) / Time.deltaTime;
                    lastTargetPosition = currentTargetPos;
                    
                    // Predict where target will be
                    float timeToTarget = Vector3.Distance(transform.position, currentTargetPos) / speed;
                    Vector3 predictedPosition = currentTargetPos + (targetVelocity * timeToTarget * predictionAccuracy);
                    targetPosition = predictedPosition;
                }
                else
                {
                    targetPosition = target.position;
                }
            }
            else
            {
                // Target is invalid, continue to last known position
                hasTarget = false;
            }
        }
        
        // Move towards target position
        MoveTowardsTarget();
    }
    
    private void MoveTowardsTarget()
    {
        if (!hasTarget && targetPosition == Vector3.zero) return;
        
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        
        // Rotate to face movement direction
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
        
        // Check if we've reached the target position
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            if (hasTarget && IsTargetValid())
            {
                HitTarget();
            }
            else
            {
                // Reached position but no target, destroy projectile
                DestroyProjectile();
            }
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
        
        if (newTarget != null)
        {
            lastTargetPosition = newTarget.position;
            targetVelocity = Vector3.zero;
        }
    }
}
