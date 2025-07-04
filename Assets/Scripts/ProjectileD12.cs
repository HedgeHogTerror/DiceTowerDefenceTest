using UnityEngine;

public class ProjectileD12 : ProjectileBase
{
    [Header("D12 Shotgun Settings")]
    [SerializeField] private int pelletCount = 5;
    [SerializeField] private float spreadAngle = 30f; // Total spread angle in degrees
    [SerializeField] private GameObject pelletPrefab;
    [SerializeField] private float pelletDamageMultiplier = 0.3f; // Each pellet does less damage
    [SerializeField] private float pelletSpeed = 15f;
    [SerializeField] private float pelletLifetime = 2f;
    
    private bool hasFired = false;
    
    protected override void Start()
    {
        base.Start();
        
        // Fire pellets immediately on spawn
        if (!hasFired)
        {
            FirePellets();
        }
    }
    
    protected override void UpdateProjectile()
    {
        // D12 projectile doesn't move itself, it just fires pellets and destroys itself
        if (hasFired)
        {
            DestroyProjectile();
        }
    }
    
    private void FirePellets()
    {
        if (hasFired) return;
        hasFired = true;
        
        Vector3 baseDirection;
        
        if (hasTarget && IsTargetValid())
        {
            baseDirection = (target.position - transform.position).normalized;
        }
        else if (hasTarget)
        {
            baseDirection = (targetPosition - transform.position).normalized;
        }
        else
        {
            baseDirection = transform.forward;
        }
        
        // Calculate pellet damage
        float pelletDamage = damage * pelletDamageMultiplier;
        
        // Fire pellets in a spread pattern
        for (int i = 0; i < pelletCount; i++)
        {
            // Calculate spread angle for this pellet
            float angleOffset;
            if (pelletCount == 1)
            {
                angleOffset = 0f;
            }
            else
            {
                // Distribute pellets evenly across the spread angle
                float t = (float)i / (pelletCount - 1); // 0 to 1
                angleOffset = Mathf.Lerp(-spreadAngle / 2f, spreadAngle / 2f, t);
            }
            
            // Create pellet direction with spread
            Vector3 pelletDirection = RotateVectorByAngle(baseDirection, angleOffset);
            
            // Create pellet
            CreatePellet(pelletDirection, pelletDamage);
        }
    }
    
    private void CreatePellet(Vector3 direction, float pelletDamage)
    {
        GameObject pellet;
        
        if (pelletPrefab != null)
        {
            pellet = Instantiate(pelletPrefab, transform.position, Quaternion.LookRotation(direction));
        }
        else
        {
            // Create a simple pellet if no prefab is assigned
            pellet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pellet.transform.position = transform.position;
            pellet.transform.rotation = Quaternion.LookRotation(direction);
            pellet.transform.localScale = Vector3.one * 0.1f;
            
            // Add rigidbody for movement
            Rigidbody rb = pellet.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.linearVelocity = direction * pelletSpeed;
            
            // Add collider trigger
            SphereCollider col = pellet.GetComponent<SphereCollider>();
            col.isTrigger = true;
        }
        
        // Add pellet component
        ShotgunPellet pelletComponent = pellet.GetComponent<ShotgunPellet>();
        if (pelletComponent == null)
        {
            pelletComponent = pellet.AddComponent<ShotgunPellet>();
        }
        
        pelletComponent.Initialize(direction, pelletSpeed, pelletDamage, pelletLifetime);
        
        // Copy trail effect if available
        if (trail != null)
        {
            TrailRenderer pelletTrail = pellet.GetComponent<TrailRenderer>();
            if (pelletTrail == null)
            {
                pelletTrail = pellet.AddComponent<TrailRenderer>();
                pelletTrail.material = trail.material;
                pelletTrail.startWidth = trail.startWidth * 0.5f;
                pelletTrail.endWidth = trail.endWidth * 0.5f;
                pelletTrail.time = trail.time * 0.5f;
            }
        }
    }
    
    private Vector3 RotateVectorByAngle(Vector3 vector, float angle)
    {
        // Rotate around the up axis (Y-axis) for horizontal spread
        // You could also add vertical spread by rotating around right axis
        return Quaternion.AngleAxis(angle, Vector3.up) * vector;
    }
    
    protected override void OnTriggerEnter(Collider other)
    {
        // D12 projectile doesn't hit targets directly, only its pellets do
    }
}
