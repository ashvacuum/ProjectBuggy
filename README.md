# Project Starship

A 3D bullet heaven rogue-like space combat game built with Unity DOTS (ECS) and HDRP. Survive against cybernetic empire drone swarms while upgrading your ship and collecting an arsenal of powerful weapons.

![Unity Version](https://img.shields.io/badge/Unity-6000.x-blue)
![DOTS Version](https://img.shields.io/badge/DOTS-1.3.x-green)
![License](https://img.shields.io/badge/License-MIT-yellow)

## 🎮 Game Overview

**Project Starship** is inspired by Vampire Survivors but set in 3D space with advanced weapon systems and ship customization. Players pilot decommissioned spacecraft using the "Alchemist System" to transmute scrap into powerful upgrades.

### Core Features
- **3D Space Combat** - Full 360-degree movement and targeting
- **Weapon Evolution** - Weapons transform dramatically at max level
- **Ship Varieties** - Multiple ship types with unique starting weapons
- **Scrap-Based Progression** - Collect C/B/S tier materials for upgrades
- **Boss Encounters** - Every 5 minutes with salvageable derelicts
- **Scalable Difficulty** - From 10 enemies at 2min to 3000 at 30min

## 🚀 Getting Started

### Prerequisites
- Unity 6000.x or later
- HDRP pipeline support
- Minimum 8GB RAM (16GB recommended for large enemy counts)

### Installation
1. Clone the repository
2. Open in Unity 6000.x
3. Install required packages (Unity will prompt automatically)
4. Open `Assets/Scenes/CameraFollow ECS.unity`
5. Press Play!

## 🏗️ Architecture Overview

### Unity DOTS/ECS Structure
The game is built using Unity's Data-Oriented Technology Stack for maximum performance:

```
Assets/Scripts/ShipECS/
├── Components/          # Data-only ECS components
├── Systems/            # Game logic and behavior
├── Entities/           # Entity definitions and aspects
├── Data/              # Shared data structures
└── Utility/           # Helper utilities
```

### Key Systems
- **Movement Systems** - Player/enemy movement with spatial optimization
- **Weapon Systems** - Modular, data-driven weapon framework
- **Experience System** - Level progression with exponential scaling
- **Spawning Systems** - Dynamic enemy/boss generation
- **VFX Systems** - Performant visual effects management

## ⚔️ Weapon System Documentation

### Overview
The weapon system is designed to be **completely data-driven** and **highly extensible**. Both simple and complex weapon behaviors can be created without touching core systems.

### Architecture

#### 1. Legacy Weapon System (Original)
Located in `Assets/Scripts/NonECS/BaseWeapons/`

**Components:**
- `WeaponBase.cs` - Base ScriptableObject for weapon data
- `ProjectileWeaponBase.cs` - Inherits from WeaponBase
- `ArtilleryWeaponBase.cs` - Inherits from WeaponBase

**ECS Components:**
- `ProjectileAttack` (in `ProjectileFiringSystem.cs`)
- `ArtilleryAttack` (in `ArtilleryQueueSystem.cs`)
- `DroneAttack` (in `DroneSpawningSystem.cs`)

**Pros:** ✅ Working, battle-tested
**Cons:** ❌ Manual component creation, lots of copy-paste code

#### 2. Automated Weapon System (New)
Located in `Assets/Scripts/NonECS/WeaponGeneration/`

**Core Files:**
- `WeaponDefinition.cs` - ScriptableObject for defining any weapon
- `WeaponCodeGenerator.cs` - Automatically generates ECS components
- `WeaponBehaviorSystem.cs` - Handles complex weapon behaviors
- `WeaponJsonImporter.cs` - Import/export weapons from JSON

**Generated Files:**
```
Assets/Scripts/ShipECS/Components/Generated/
├── [WeaponName]Attack.cs           # Main weapon component
├── [WeaponName]BeamState.cs        # Beam-specific data
├── [WeaponName]ChainTarget.cs      # Chain lightning targets
└── [WeaponName]HomingData.cs       # Homing missile data

Assets/Scripts/NonECS/BaseWeapons/Generated/
└── [WeaponName]WeaponBase.cs       # ScriptableObject for Unity editor

Assets/Scripts/NonECS/ScriptableObjects/Generated/
├── [WeaponName]Data.cs             # Upgrade progression data
└── GeneratedUpgradeTypes.cs        # Auto-updated enum
```

### Creating New Weapons

#### Method 1: Unity Editor (Recommended)
1. Open `Tools > Weapon Generator`
2. Click "Create New Weapon"
3. Fill in weapon details:
   - **Weapon Name**: Code-friendly name (e.g., "PlasmaRifle")
   - **Display Name**: User-friendly name (e.g., "Plasma Rifle")
   - **Category**: Projectile, Artillery, Laser, Beam, etc.
   - **Archetype**: SingleTarget, AreaOfEffect, Continuous, etc.
4. Click "Create Weapon Definition"
5. Configure stats and behaviors in the inspector
6. Click "Generate Selected Weapon"

#### Method 2: JSON Definition
Create a JSON file in your preferred location:

```json
{
  "weaponName": "PlasmaRifle",
  "displayName": "Plasma Rifle",
  "description": "High-energy plasma bolts that pierce through enemies",
  "category": "Laser",
  "archetype": "Piercing",
  "baseStats": {
    "baseDamage": 25.0,
    "baseFireRate": 2.0,
    "baseNumProjectiles": 1,
    "baseRange": 20.0,
    "baseSpeed": 15.0,
    "baseLifetime": 2.0,
    "baseSize": 1.0,
    "basePenetration": 3,
    "baseKnockback": 1.0,
    "baseCritical": 0.15
  },
  "upgradeLevels": [
    {
      "level": 1,
      "description": "Increased damage and penetration",
      "statMultipliers": {
        "baseDamage": 1.3
      },
      "statAdditions": {
        "basePenetration": 1
      },
      "specialEffects": []
    }
  ],
  "behaviorFlags": ["PierceEnemies", "AutoTarget"],
  "unlockRequirements": []
}
```

Then import via the Weapon Generator window.

### Weapon Behavior Types

#### Simple Behaviors (Automatic)
These work with existing systems:

- **StandardProjectile** - Basic bullets (uses `ProjectileFiringSystem`)
- **StandardArtillery** - Explosive shells (uses `ArtilleryQueueSystem`)

#### Complex Behaviors (Generated Components)
These generate additional ECS components:

##### ContinuousBeam
**Generated Components:**
- `[WeaponName]BeamState` - Tracks beam position and state
- Additional fields in main component: `BeamWidth`, `BeamLength`, `EnergyConsumption`

**Parameters:**
- `beamWidth` - Width of the beam
- `beamLength` - Maximum beam length  
- `chargeTime` - Time to charge before firing
- `sustainTime` - How long beam lasts
- `energyConsumption` - Energy per second
- `pierceEnemies` - Can hit multiple enemies

**Required System:** `BeamWeaponSystem` (you implement)

##### ChainLightning
**Generated Components:**
- `[WeaponName]ChainTarget` - Buffer of chain targets
- Additional fields: `MaxChains`, `ChainRange`, `DamageReductionPerChain`

**Parameters:**
- `maxChains` - Maximum chain jumps
- `chainRange` - Distance to find next target
- `damageReductionPerChain` - Damage multiplier per jump
- `chainDelay` - Time between jumps

**Required System:** `ChainLightningSystem` (you implement)

##### HomingMissile
**Generated Components:**
- `[WeaponName]HomingData` - Target and tracking state
- Additional fields: `TrackingSpeed`, `MaxTrackingRange`, `AccelerationTime`

**Parameters:**
- `trackingSpeed` - Turn rate in degrees/second
- `maxTrackingRange` - Max distance to track
- `accelerationTime` - Time to reach max speed
- `retargetOnKill` - Find new target if current dies

**Required System:** `HomingMissileSystem` (you implement)

##### DroneSwarm
**Generated Components:**
- `[WeaponName]DroneData` - Buffer of active drones
- Additional fields: `MaxDrones`, `DroneLifetime`, `DroneSpeed`

**Parameters:**
- `maxDrones` - Maximum active drones
- `droneLifetime` - How long drones last
- `droneSpeed` - Drone movement speed
- `spawnDelay` - Time between spawns

**Required System:** `DroneSwarmSystem` (you implement)

### Example Weapon Definitions

#### Basic Projectile (Simple)
```csharp
var weapon = CreateInstance<WeaponDefinition>();
weapon.weaponName = "BasicBullet";
weapon.category = WeaponCategory.Projectile;
weapon.behaviorFlags = WeaponBehaviorFlags.AutoTarget | WeaponBehaviorFlags.DestroyOnContact;
// Uses existing ProjectileFiringSystem
```

#### Plasma Beam (Complex)
```csharp
var weapon = CreateInstance<WeaponDefinition>();
weapon.weaponName = "PlasmaBeam";
weapon.category = WeaponCategory.Beam;
weapon.behaviorFlags = WeaponBehaviorFlags.RequiresChargeUp | WeaponBehaviorFlags.ContinuousFire;

var beamBehavior = new WeaponBehaviorDefinition
{
    behaviorType = WeaponBehaviorType.ContinuousBeam,
    customSystemName = "BeamWeaponSystem",
    parameters = new List<BehaviorParameter>
    {
        new BehaviorParameter { name = "chargeTime", value = "1.5" },
        new BehaviorParameter { name = "sustainTime", value = "3.0" },
        new BehaviorParameter { name = "beamWidth", value = "2.0" }
    }
};
weapon.complexBehaviors = new List<WeaponBehaviorDefinition> { beamBehavior };
```

### Implementing Custom Systems

When you generate a weapon with complex behaviors, you'll need to implement the corresponding system. Here's the pattern:

```csharp
[UpdateInGroup(typeof(PausableSystemGroup))]
public partial struct BeamWeaponSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlasmaBeamAttack>();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Handle beam charging
        foreach (var beamAspect in SystemAPI.Query<BeamWeaponAspect>())
        {
            if (beamAspect.IsCharging)
            {
                // Update charge progress
                beamAspect.CurrentCharge += SystemAPI.Time.DeltaTime;
                
                if (beamAspect.CurrentCharge >= beamAspect.ChargeTime)
                {
                    // Start firing beam
                    StartBeam(ref state, beamAspect);
                }
            }
            else if (beamAspect.IsFiring)
            {
                // Update beam damage and effects
                UpdateBeam(ref state, beamAspect);
            }
        }
    }
}
```

### Integration with Upgrade System

The automated system integrates seamlessly with the existing upgrade system:

1. **Generated enum** - `GeneratedUpgradeTypes.cs` automatically includes new weapons
2. **Upgrade application** - `UpgradeApplicationSystem.cs` can be extended for new weapon types
3. **UI integration** - Weapon names and descriptions are automatically available

### Performance Considerations

#### ECS Optimization
- All weapon components use `IComponentData` for cache efficiency
- Complex behaviors use additional components rather than expanding base component
- Burst compilation supported for all generated code
- Buffer components used for variable-length data (chains, drones, etc.)

#### Spatial Grid Integration
- All projectiles automatically work with the existing spatial grid system
- Complex behaviors can opt into spatial optimization by implementing `SpatialGridData`

#### Memory Management
- Generated components follow ECS best practices
- No managed allocations in hot paths
- Efficient data layouts for maximum performance

## 🎯 Experience & Progression System

### Level Calculation
```csharp
public int GetCurrentLevel()
{
    var level = 0;
    var expRequiredToNextLevel = 100;
    for (var i = 1; i < 1000; i++)
    {
        var levelRequirement = 100 + (10 * i);
        if (TotalExperience >= expRequiredToNextLevel)
            expRequiredToNextLevel += levelRequirement;
        else
        {
            level = i;
            break;
        }
    }
    return level;
}
```

### Scrap Types
- **C Scrap** - Basic experience (available immediately)
- **B Scrap** - 5x experience (unlocked at 10 minutes)
- **S Scrap** - 20x experience (unlocked at 20 minutes)

### Boss System
- Spawns every 5 minutes
- Becomes salvageable derelict when defeated
- Provides 1-5 random upgrades based on luck stat
- Luck system provides bonus rolls up to 5 total

## 🎨 UI System Architecture

### Modular UI Components
- **UIManager** - Central coordinator with singleton pattern
- **GameUIView** - Health, experience, and level display
- **UpgradeUIController** - Clean upgrade selection interface
- **UpgradeApplicationSystem** - Separates upgrade logic from UI

### Responsive Design
- CSS media queries for different screen sizes
- Flexible layouts that adapt to content
- Hover effects and animations
- Designer-friendly configuration via ScriptableObjects

## 🔧 Development Tools

### Weapon Generator Window
Access via `Tools > Weapon Generator`

**Features:**
- Visual weapon creation wizard
- List and edit existing weapons
- Generate code for single weapons or all at once
- JSON import/export functionality
- Real-time validation and error checking

### Code Generation
- Automatic ECS component generation
- ScriptableObject creation for Unity editor
- Upgrade system integration
- Consistent naming conventions
- Organized file structure

### JSON Workflow
- Define weapons in external tools
- Version control friendly
- Easy collaboration
- Batch import capabilities

## 📁 Project Structure

```
ProjectSpaceship/
├── Assets/
│   ├── Scripts/
│   │   ├── ShipECS/                    # ECS gameplay systems
│   │   │   ├── Components/             # ECS components
│   │   │   │   └── Generated/          # Auto-generated components
│   │   │   ├── Systems/                # Game logic systems
│   │   │   ├── Entities/               # Entity definitions
│   │   │   └── Data/                   # Shared data structures
│   │   ├── NonECS/                     # MonoBehaviour systems
│   │   │   ├── UI/                     # User interface
│   │   │   ├── BaseWeapons/            # Legacy weapon system
│   │   │   │   └── Generated/          # Auto-generated weapon bases
│   │   │   ├── WeaponGeneration/       # New weapon system
│   │   │   └── ScriptableObjects/      # Data containers
│   │   │       └── Generated/          # Auto-generated data classes
│   │   └── Authoring/                  # Conversion systems
│   ├── Editor/                         # Unity Editor tools
│   ├── Scenes/                         # Game scenes
│   ├── Prefabs/                        # Game objects
│   │   ├── Player Ships/               # Ship varieties
│   │   ├── Enemy Ships/                # Enemy types
│   │   ├── Projectiles/                # Weapon projectiles
│   │   └── Scrap/                      # Collectible items
│   ├── UI Toolkit/                     # UI definitions
│   │   ├── XML/                        # UXML layouts
│   │   └── Style Sheets/               # USS styling
│   ├── Art/                            # Visual assets
│   ├── VFX/                            # Visual effects
│   └── ScriptableObjects/              # Configuration data
├── ProjectSettings/                    # Unity project settings
└── README.md                           # This file
```

## 🚀 Performance

### Scalability
- **3000+ enemies** at 30 minutes with stable 60+ FPS
- **Spatial grid optimization** for collision detection
- **Burst compilation** for critical systems
- **Job system utilization** for parallel processing

### Memory Management
- ECS archetypes for cache-efficient data access
- Minimal garbage collection in gameplay loops
- Efficient VFX pooling and recycling
- Optimized rendering with HDRP

## 🔄 Development Workflow

### Adding New Weapons (Recommended)
1. Use `Tools > Weapon Generator` to create weapon definition
2. Configure stats, behaviors, and upgrade progression
3. Generate code automatically
4. If using complex behaviors, implement required system
5. Test in-game and iterate

### Adding New Ship Types
1. Create ship prefab in `Assets/Prefabs/Player Ships/`
2. Add `MainCharacterAuthoring` component
3. Configure starting weapon and stats
4. Update ship selection system

### Adding New Enemy Types
1. Create enemy prefab in `Assets/Prefabs/Enemy Ships/`
2. Add `EnemyAuthoring` component
3. Configure AI behavior and stats
4. Update enemy spawn system

## 🧪 Testing

### Unit Testing
- ECS system testing with `Unity.Entities.Tests`
- Weapon behavior validation
- Performance profiling tools

### Playtesting
- Survival time metrics
- Weapon balance feedback
- Performance monitoring

## 📊 Metrics & Analytics

### Performance Metrics
- Entity count tracking
- Frame time analysis
- Memory usage monitoring
- System update time profiling

### Gameplay Metrics
- Weapon usage statistics
- Survival time distribution
- Upgrade choice analysis
- Boss encounter success rate

## 🤝 Contributing

### Code Style
- Follow Unity C# coding conventions
- Use ECS best practices
- Document complex behaviors
- Maintain backward compatibility

### Weapon Contributions
- Use the automated weapon system for new weapons
- Provide comprehensive upgrade progressions
- Include clear behavior descriptions
- Test with various ship types

### System Contributions
- Follow existing system patterns
- Maintain Burst compilation compatibility
- Document performance characteristics
- Include unit tests where applicable

## 📝 License

MIT License - see LICENSE file for details

## 🙏 Acknowledgments

- Unity DOTS team for the excellent ECS framework
- Gabriel Aguiar Productions for VFX assets
- Kenney for UI art assets
- Community feedback and testing

---

*Built with ❤️ using Unity DOTS and ECS*