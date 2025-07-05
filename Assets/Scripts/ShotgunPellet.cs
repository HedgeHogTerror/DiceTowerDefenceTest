using UnityEngine;
// Separate component for individual pellets
public class ShotgunPellet : ProjectileBase
{
    [Header("Shotgun Pellet Settings")]
    private Vector3 velocity;

    public void Initialize(Vector3 direction, float pelletSpeed, float pelletDamage, float pelletLifetime)
    {

        // Set pellet specific properties
        damage = pelletDamage;
        lifeTime = pelletLifetime; // Lifetime of the pellet
        speed = pelletSpeed; // Speed of the pellet

        // Set initial velocity
        velocity = transform.forward * speed;

        // Destroy after lifetime
        Destroy(gameObject, lifeTime);
    }
    protected override void Start()
    {
        base.Start();
    }

    protected override void UpdateProjectile()
    {
        // Move pellet
        base.transform.position += base.transform.forward * speed * Time.deltaTime;
    }

}