# Tower Defense Game Classes

This collection of C# scripts provides a complete foundation for creating a tower defense game in Unity. The classes are designed to work together to create a functional tower defense experience.

## Core Classes Overview

### 1. Health.cs
**Purpose**: Manages health for any game object that can take damage.
- Tracks current and maximum health
- Provides events for health changes and death
- Includes healing functionality
- Used by both enemies and potentially towers

### 2. Enemy.cs
**Purpose**: Controls enemy behavior and movement.
- Follows waypoint-based paths
- Configurable stats (speed, reward value, damage)
- Automatically finds waypoints in scene
- Handles reaching the end and damaging the player

### 3. Tower.cs
**Purpose**: Manages tower behavior, targeting, and combat.
- Automatic enemy detection within range
- Configurable damage, range, and fire rate
- Visual range indicators
- Upgrade system for improving stats
- Multiple targeting strategies (closest enemy by default)

### 4. Projectile.cs
**Purpose**: Handles projectile movement and damage dealing.
- Tracks moving targets
- Configurable speed, damage, and lifetime
- Hit effects support
- Collision-based and distance-based hit detection

### 5. GameManager.cs
**Purpose**: Central game state management (Singleton pattern).
- Manages player resources (money, lives)
- Handles game states (game over, victory, pause)
- Wave progression tracking
- Save/load functionality framework
- Event system for UI updates

### 6. WaveManager.cs
**Purpose**: Controls enemy wave spawning and progression.
- Configurable wave settings (enemy count, spawn rate, delays)
- Enemy stat modifiers per wave
- Automatic wave progression
- Wave completion tracking
- Support for different enemy types per wave

### 7. TowerPlacer.cs
**Purpose**: Handles tower placement mechanics.
- Mouse-based tower placement
- Placement validation (obstacles, existing towers)
- Visual placement preview
- Grid snapping option
- Keyboard shortcuts for tower selection

### 8. UIManager.cs
**Purpose**: Manages all user interface elements.
- Real-time display updates (money, lives, wave info)
- Tower placement buttons
- Game state panels (pause, game over, victory)
- Tower selection and upgrade interface
- Wave progress tracking

## Setup Instructions

### Scene Setup Requirements

1. **Camera**: Ensure you have a main camera tagged as "MainCamera"

2. **Waypoints**: Create a GameObject named "Waypoints" with child objects representing the path enemies should follow

3. **Spawn Point**: Create a GameObject named "SpawnPoint" where enemies will spawn

4. **Ground/Placement Surface**: Create a ground plane or terrain with appropriate colliders for tower placement

### GameObject Setup

#### GameManager Setup
1. Create an empty GameObject named "GameManager"
2. Add the `GameManager` script
3. Configure starting money and lives in the inspector

#### WaveManager Setup
1. Create an empty GameObject named "WaveManager"
2. Add the `WaveManager` script
3. Create enemy prefabs and assign them to wave configurations
4. Set spawn point and waypoints references

#### TowerPlacer Setup
1. Create an empty GameObject named "TowerPlacer"
2. Add the `TowerPlacer` script
3. Create tower prefabs and assign them to the tower prefabs array
4. Configure placement materials for preview feedback

#### UI Setup
1. Create a Canvas for your UI
2. Add the `UIManager` script to a GameObject
3. Create UI elements and assign references in the inspector:
   - Text elements for money, lives, wave info
   - Buttons for wave control and game management
   - Panels for game states

### Prefab Creation

#### Enemy Prefab
1. Create a 3D object (cube, capsule, etc.)
2. Add the `Enemy` script
3. Add the `Health` script
4. Add a Collider component
5. Set up the layer for enemy detection

#### Tower Prefab
1. Create a 3D object for the tower base
2. Add the `Tower` script
3. Create a child object for the fire point
4. Assign a projectile prefab
5. Configure tower stats in the inspector

#### Projectile Prefab
1. Create a small 3D object (sphere, arrow, etc.)
2. Add the `Projectile` script
3. Add a Collider set as trigger
4. Optionally add a TrailRenderer for visual effects

## Key Features

### Event System
The classes use Unity's UnityEvent system for loose coupling:
- Health changes trigger UI updates
- Enemy deaths reward money
- Wave completion progresses the game

### Modular Design
Each class has a specific responsibility and can be easily modified or extended:
- Add new tower types by creating new prefabs
- Modify enemy behavior without affecting other systems
- Extend the UI without changing core game logic

### Configurable Parameters
Most gameplay values are exposed in the inspector:
- Tower stats (damage, range, fire rate, cost)
- Enemy stats (health, speed, reward)
- Wave configurations (enemy count, spawn rate, modifiers)
- Game settings (starting money, lives)

## Usage Tips

1. **Layer Setup**: Use different layers for enemies, towers, and placement surfaces for proper collision detection

2. **Performance**: The system is designed to handle moderate numbers of enemies and towers. For larger scales, consider object pooling

3. **Extensibility**: The base classes can be inherited to create specialized tower types or enemy variants

4. **Testing**: Start with simple configurations and gradually add complexity

5. **Visual Feedback**: The system includes range indicators and placement previews for better user experience

## Common Customizations

- **New Tower Types**: Inherit from Tower class and override targeting or firing behavior
- **Special Enemies**: Inherit from Enemy class to add special abilities
- **Different Projectiles**: Create projectile variants with different behaviors (splash damage, piercing, etc.)
- **Advanced UI**: Extend UIManager to add more detailed information displays

This system provides a solid foundation that can be expanded based on your specific game design needs.
