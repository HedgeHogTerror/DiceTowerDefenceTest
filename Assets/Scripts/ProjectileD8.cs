using UnityEngine;

public class ProjectileD8 : ProjectileBase
{
    [Header("D8 Explosive Settings")]
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private float explosionDamageMultiplier = 0.7f; // Damage multiplier for explosion area
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private LayerMask enemyLayerMask = -1;
    [SerializeField] private AnimationCurve damageFalloff = AnimationCurve.Linear(0f, 1f, 1f, 0.3f);
    
    protected override void UpdateProjectile()
    {
        if (hasTarget && IsTargetValid())
        {
            // Update target position for moving targets
            targetPosition = target.position;
        }
        else if (hasTarget && !IsTargetValid())
        {
            // Target is dead/invalid, continue to last known position
            hasTarget = false;
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
            Explode();
        }
    }
    
    protected override void HitTarget(Transform hitTarget = null)
    {
        if (isDestroyed) return;
        
        // Don't call base HitTarget, we handle damage in Explode()
        Explode();
    }
    
    private void Explode()
    {
        if (isDestroyed) return;
        isDestroyed = true;
        
        // Spawn explosion effect
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }
        else
        {
            // Fallback to hit effect
            SpawnHitEffect();
        }
        
        // Find all enemies in explosion radius
        Collider[] enemiesInRadius = Physics.OverlapSphere(transform.position, explosionRadius, enemyLayerMask);
        
        foreach (Collider enemyCollider in enemiesInRadius)
        {
            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null && !enemy.isDead)
            {
                // Calculate distance-based damage
                float distance = Vector3.Distance(transform.position, enemyCollider.transform.position);
                float normalizedDistance = distance / explosionRadius;
                
                // Apply damage falloff curve
                float damageMultiplier = damageFalloff.Evaluate(normalizedDistance) * explosionDamageMultiplier;
                float finalDamage = damage * damageMultiplier;
                
                // Deal damage
                Health enemyHealth = enemyCollider.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(finalDamage);
                }
            }
        }
        
        // Destroy projectile
        Destroy(gameObject);
    }
    
    protected override void OnTriggerEnter(Collider other)
    {
        if (isDestroyed) return;
        
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && !enemy.isDead)
        {
            Explode();
        }
    }
    
    // Visualization for explosion radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
        
        // Draw damage falloff visualization
        Gizmos.color = Color.yellow;
        for (float i = 0; i <= explosionRadius; i += explosionRadius / 10f)
        {
            float normalizedDistance = i / explosionRadius;
            float alpha = damageFalloff.Evaluate(normalizedDistance);
            Gizmos.color = new Color(1f, 1f, 0f, alpha);
            Gizmos.DrawWireSphere(transform.position, i);
        }
    }
}
