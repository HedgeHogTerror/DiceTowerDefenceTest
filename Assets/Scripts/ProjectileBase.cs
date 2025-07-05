using UnityEngine;

public abstract class ProjectileBase : MonoBehaviour
{
    [Header("Base Projectile Stats")]
    [SerializeField] protected float speed = 10f;
    [SerializeField] protected float damage = 25f;
    [SerializeField] protected float lifeTime = 5f;
    
    [Header("Effects")]
    [SerializeField] protected GameObject hitEffect;
    [SerializeField] protected TrailRenderer trail;
    
    protected Transform target;
    protected Vector3 targetPosition;
    protected bool hasTarget = false;
    protected bool isDestroyed = false;
    
    public float Speed => speed;
    public float Damage => damage;
    
    protected virtual void Start()
    {
        // Destroy projectile after lifetime to prevent memory leaks
        Destroy(gameObject, lifeTime);
    }
    
    protected virtual void Update()
    {
        if (isDestroyed) return;
        
        UpdateProjectile();
    }
    
    protected abstract void UpdateProjectile();
    
    protected virtual void HitTarget(Transform hitTarget = null)
    {
        if (isDestroyed) return;
        isDestroyed = true;
        
        Transform actualTarget = hitTarget != null ? hitTarget : target;
        
        // Deal damage to target if it still exists
        if (actualTarget != null)
        {
            Health targetHealth = actualTarget.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
            }
        }
        
        // Spawn hit effect
        SpawnHitEffect();
        
        // Perform any additional hit logic
        OnHit(actualTarget);
        
        // Destroy projectile
        Destroy(gameObject);
    }
    
    protected virtual void SpawnHitEffect()
    {
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, transform.rotation);
        }
    }
    
    protected virtual void OnHit(Transform hitTarget)
    {
        // Override in derived classes for special hit behavior
    }
    
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (isDestroyed) return;
        
        // Check if we hit our target
        if (hasTarget && other.transform == target)
        {
            HitTarget(other.transform);
        }
        else if (!hasTarget)
        {
            // If no specific target, hit any enemy
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null && !enemy.isDead)
            {
                HitTarget(other.transform);
            }
        }
    }
    
    public virtual void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            targetPosition = target.position;
            hasTarget = true;
        }
    }
    
    public virtual void SetTargetPosition(Vector3 position)
    {
        targetPosition = position;
        hasTarget = true;
        target = null; // No specific target, just a position
    }
    
    public virtual void SetDamage(float newDamage)
    {
        damage = newDamage;
    }
    
    public virtual void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
    
    public virtual void SetLifeTime(float newLifeTime)
    {
        lifeTime = newLifeTime;
        
        // Cancel previous destroy and set new one
        CancelInvoke();
        Invoke(nameof(DestroyProjectile), lifeTime);
    }
    
    protected virtual void DestroyProjectile()
    {
        if (!isDestroyed)
        {
            isDestroyed = true;
            Destroy(gameObject);
        }
    }
    
    protected bool IsTargetValid()
    {
        if (target == null) return false;
        
        Enemy enemy = target.GetComponent<Enemy>();
        return enemy != null && !enemy.isDead;
    }
}
