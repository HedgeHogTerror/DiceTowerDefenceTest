# Projectile System Setup Guide

## Overview
I've created a complete projectile system with different behaviors for each tower type:

- **D4**: Homing projectile that seeks targets
- **D6**: Standard balanced projectile with predictive aiming
- **D8**: Explosive projectile with area damage
- **D12**: Shotgun-style projectile that fires multiple pellets
- **D14**: Homing laser beam with damage over time

## Files Created

### Base System
- `ProjectileBase.cs` - Abstract base class for all projectiles
- Updated `Tower.cs` - Now supports multiple projectile types

### Projectile Types
- `ProjectileD4.cs` - Homing projectile
- `ProjectileD6.cs` - Standard balanced projectile
- `ProjectileD8.cs` - Explosive projectile
- `ProjectileD12.cs` - Shotgun projectile (includes ShotgunPellet component)
- `ProjectileD14.cs` - Laser beam projectile

## Setup Instructions

### 1. Create Projectile Prefabs
For each tower type, create a prefab with the appropriate projectile script:

**D4 Homing Projectile:**
1. Create empty GameObject
2. Add `ProjectileD4` component
3. Add visual model (sphere, arrow, etc.)
4. Add Collider (set as Trigger)
5. Optional: Add TrailRenderer for visual effect
6. Save as prefab

**D6 Standard Projectile:**
1. Create empty GameObject
2. Add `ProjectileD6` component
3. Add visual model
4. Add Collider (set as Trigger)
5. Optional: Add TrailRenderer
6. Save as prefab

**D8 Explosive Projectile:**
1. Create empty GameObject
2. Add `ProjectileD8` component
3. Add visual model (bomb, grenade, etc.)
4. Add Collider (set as Trigger)
5. Create explosion effect prefab and assign to Explosion Effect field
6. Save as prefab

**D12 Shotgun Projectile:**
1. Create empty GameObject
2. Add `ProjectileD12` component
3. Add visual model (shell, cartridge, etc.)
4. Optional: Create pellet prefab and assign to Pellet Prefab field
5. Save as prefab

**D14 Laser Projectile:**
1. Create empty GameObject
2. Add `ProjectileD14` component
3. Add visual model (energy core, etc.)
4. Create laser material and assign to Laser Material field
5. Optional: Create beam start/end effect prefabs
6. Save as prefab

### 2. Configure Tower Prefabs
For each tower prefab:

1. Open tower prefab
2. In Tower component, expand "References" section
3. Set "Projectile Prefabs" array size to 5
4. Assign projectile prefabs in order:
   - Element 0: D4 projectile prefab
   - Element 1: D6 projectile prefab
   - Element 2: D8 projectile prefab
   - Element 3: D12 projectile prefab
   - Element 4: D14 projectile prefab
5. Set the tower's "Tower Type" to match desired behavior
6. Save prefab

### 3. Projectile Settings

Each projectile type has configurable settings:

**D4 Homing:**
- Homing Strength: How aggressively it tracks (default: 5)
- Max Turn Rate: Maximum turning speed in degrees/second (default: 180)
- Homing Delay: Delay before homing starts (default: 0.1s)

**D6 Standard:**
- Predictive Aiming: Enable/disable target prediction (default: true)
- Prediction Accuracy: How accurate the prediction is 0-1 (default: 0.8)

**D8 Explosive:**
- Explosion Radius: Area of effect radius (default: 3)
- Explosion Damage Multiplier: Damage multiplier for explosion (default: 0.7)
- Damage Falloff: Animation curve for damage falloff with distance

**D12 Shotgun:**
- Pellet Count: Number of pellets fired (default: 5)
- Spread Angle: Total spread angle in degrees (default: 30)
- Pellet Damage Multiplier: Damage per pellet (default: 0.3)
- Pellet Speed: Speed of individual pellets (default: 15)

**D14 Laser:**
- Beam Duration: How long the laser lasts (default: 3s)
- Damage Per Second: Continuous damage rate (default: 15)
- Homing Strength: How aggressively it tracks (default: 8)
- Beam Width: Visual width of laser beam (default: 0.2)

## Visual Effects

### Recommended Effects
- **Trail Renderers**: Add to all projectiles for motion trails
- **Hit Effects**: Particle systems for impact
- **Explosion Effects**: For D8 projectiles
- **Laser Materials**: Glowing/animated materials for D14
- **Muzzle Flash**: Effects at tower fire points

### Materials
Create materials for different projectile types:
- Glowing materials for energy projectiles
- Metallic materials for physical projectiles
- Animated materials for laser beams

## Testing

1. Create test scene with towers of different types
2. Spawn enemies to test targeting
3. Verify each projectile type behaves correctly:
   - D4: Curves toward targets
   - D6: Leads moving targets
   - D8: Explodes and damages multiple enemies
   - D12: Fires spread of pellets
   - D14: Creates continuous laser beam

## Troubleshooting

**Projectiles not firing:**
- Check that projectile prefabs are assigned to tower
- Verify tower type matches array index
- Check console for warning messages

**Projectiles not hitting:**
- Ensure projectile has Collider set as Trigger
- Check enemy LayerMask settings
- Verify enemy has Health component

**Visual issues:**
- Check material assignments
- Verify TrailRenderer settings
- Ensure effect prefabs are assigned

**Performance issues:**
- Reduce pellet count for D12 projectiles
- Optimize particle effects
- Consider object pooling for frequently spawned projectiles

## Extending the System

To add new projectile types:
1. Create new class inheriting from ProjectileBase
2. Override UpdateProjectile() method
3. Implement custom behavior
4. Add to tower's projectile array
5. Update TowerType enum if needed
