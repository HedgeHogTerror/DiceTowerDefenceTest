using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Stats")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float lifeTime = 5f;
    
    [Header("Effects")]
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private TrailRenderer trail;
    
    private Transform target;
    private Vector3 targetPosition;
    private bool hasTarget = false;
    
    public float Speed => speed;
    public float Damage => damage;
    
    private void Start()
    {
        // Destroy projectile after lifetime to prevent memory leaks
        Destroy(gameObject, lifeTime);
    }
    
    private void Update()
    {
        if (hasTarget && target != null)
        {
            // Update target position for moving targets
            targetPosition = target.position;
        }
        
        // Move towards target position
        MoveTowardsTarget();
    }
    
    private void MoveTowardsTarget()
    {
        if (!hasTarget) return;
        
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
            HitTarget();
        }
    }
    
    private void HitTarget()
    {
        // Deal damage to target if it still exists
        if (target != null)
        {
            Health targetHealth = target.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
            }
        }
        
        // Spawn hit effect
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, transform.rotation);
        }
        
        // Destroy projectile
        Destroy(gameObject);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Alternative hit detection using colliders
        if (hasTarget && other.transform == target)
        {
            HitTarget();
        }
        else if (!hasTarget)
        {
            // If no specific target, hit any enemy
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                Health enemyHealth = other.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage);
                }
                
                // Spawn hit effect
                if (hitEffect != null)
                {
                    Instantiate(hitEffect, transform.position, transform.rotation);
                }
                
                Destroy(gameObject);
            }
        }
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            targetPosition = target.position;
            hasTarget = true;
        }
    }
    
    public void SetTargetPosition(Vector3 position)
    {
        targetPosition = position;
        hasTarget = true;
        target = null; // No specific target, just a position
    }
    
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }
    
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
    
    public void SetLifeTime(float newLifeTime)
    {
        lifeTime = newLifeTime;
        
        // Cancel previous destroy call and set new one
        CancelInvoke(nameof(DestroyProjectile));
        Invoke(nameof(DestroyProjectile), lifeTime);
    }
    
    private void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}
