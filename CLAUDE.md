# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Project Starship** is a 3D bullet heaven rogue-like space combat game inspired by Vampire Survivors but set in space. Built with Unity DOTS (Data-Oriented Technology Stack) using ECS, Unity Physics, and HDRP. Players control decommissioned spacecraft using the "Alchemist System" to transmute scrap into upgrades, surviving against cybernetic empire drone swarms for 1-30 minute sessions.

### Game Concept
- **Genre**: Bullet Heaven Rogue-Like (3D Space Survivors)
- **Core Loop**: Survive → Kill Enemies → Collect Scrap → Level Up → Choose Upgrades → Repeat
- **Victory Condition**: Defeat final boss at 30 minutes
- **USP**: 3D space combat with ECS performance, weapon evolution systems, and ship variety

## Architecture

### ECS Structure
- **Components**: Data-only `IComponentData` structs are colocated with the system or authoring file that owns them (e.g. `SpatialGrid` is defined in `SpatialGridSystem.cs`). The `ShipECS/Components` and `ShipECS/Data` folders exist but are currently empty scaffolding.
- **Systems**: Located in `Assets/Scripts/ShipECS/Systems/` - Contain game logic and behavior processing
- **Entities**: Found in `Assets/Scripts/ShipECS/Entities/` - Entity definitions and aspects for data access patterns
- **Blob assets**: In `Assets/Scripts/BlobAsset/` - Shared blob data structures
- **Authoring**: Located in `Assets/Scripts/Authoring/` - MonoBehaviour components that convert to ECS at runtime

### Key Systems
- **Movement Systems**: `EnemyMovementSystem`, `CharacterMovementSystem`, `ProjectileMovementSystem`
- **Combat Systems**: `WeaponShootingSystem`, `DamageCollisionSystem`, weapon-specific systems in `Systems/Artillery/` and `Systems/FiringSystem/`
- **Spawning Systems**: `EnemySpawnSystem`, `PlayerSpawnSystem`, `DroneSpawningSystem`
- **Spatial Optimization**: `SpatialGridSystem` for efficient collision and proximity queries
- **VFX Systems**: Visual effects management in `Systems/VFX/`

### System Groups
The project uses `PausableSystemGroup` to allow game pausing. Many systems update in groups like:
- `PostPhysicsPausableSystemGroup` for post-physics logic
- System ordering with `[UpdateAfter]` attributes for dependencies

## Development

### Unity Version
Uses Unity 6000.x with HDRP 17.x and Unity DOTS 1.3.x

### Key Dependencies
- `com.unity.entities`: Core ECS framework
- `com.unity.physics`: Physics simulation
- `com.unity.entities.graphics`: DOTS rendering
- `com.unity.render-pipelines.high-definition`: HDRP graphics pipeline
- `com.unity.inputsystem`: Modern input system

### Build Commands
This is a Unity project - build through Unity Editor or Unity command line tools. No custom build scripts detected.

### Scenes
- Main gameplay: `Assets/Scenes/CameraFollow ECS.unity`
- VFX testing: `Assets/Scenes/VFX/VFX Lessons.unity`
- Movement testing: `Assets/Scenes/Movement.unity`

## Code Patterns

### ECS Job Patterns
- Systems use `IJobEntity` for parallel processing (e.g., `FollowPlayerJob` in `EnemyMovementSystem`)
- Components use `RefRO<T>` for read-only access and `RefRW<T>` for read-write access
- Extensive use of `ComponentLookup<T>` for random entity access
- `NativeArray` and `NativeParallelMultiHashMap` for efficient data structures

### Physics Integration
- Uses Unity Physics with custom collision handling
- Knockback system for entity interactions
- Spatial grid optimization for performance at scale

### Asset Management
- ScriptableObjects in `Assets/ScriptableObjects/` for game configuration
- Prefabs organized by type in `Assets/Prefabs/`
- Blob assets for shared data across entities

## Game Systems Design

### Core Gameplay Features
- **Ship Types**: 3 player ships (Cruiser, Behemoth, Starlance) with different starting weapons
- **Weapon Systems**: Single Target Bullet, Artillery Barrage, Laser Strikes - each with evolution mechanics
- **Progression**: Experience from scrap → level up → choose from 3 upgrades (Systems or Ship upgrades)
- **Boss System**: Bosses spawn every 5 minutes, become salvageable derelicts when defeated
- **Difficulty Scaling**: Enemy count scales from 10 at 2min to 3000 at 30min
- **Scrap Types**: C/B/S tier scrap with different exp values and drop rates over time

### Upgrade System
- **Ship Attributes**: Turn Speed, Move Speed, Health, (Luck for player ships)
- **Weapon Evolution**: Weapons require specific ship systems to upgrade beyond max level
- **Luck Mechanic**: Extra upgrade rolls (up to 5 total) from derelict ships and level-ups

### Target Specifications
- **Platform**: PC, Steam Deck, Handheld PCs
- **Session Length**: 1-30 minutes
- **Price Point**: <200PHP (~$3.50 USD)
- **Competition**: TemTem Swarm, Vampire Survivors, Star Survivor

## Important Notes

- The project is being assessed for a pivot from space-ship survivors to a dune-buggy survivors game (see `TODO.md`). Most DOTS infrastructure is reusable; the new work is a terrain heightfield spine and a trick/landing loop.
- `EnemyMovementSystem.cs` uses per-entity steering toward the player (with spatial-grid separation), NOT a flow field. A flow field is planned for terrain-aware swarm channeling.
- Uses Burst compilation for performance-critical systems
- Extensive use of Unity.Mathematics for SIMD-optimized math operations
- Spatial grid system for efficient collision/proximity queries supporting thousands of entities