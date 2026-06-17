# CLAUDE.md

Guidance for Claude Code when working in this repository.

## Project Overview

**Project Dune-Crawler** (repo name `ProjectBuggy`, forked from a Unity DOTS space-ship survivors
project) is a top-down **arcade dune-buggy survivor**. You drive a fast vehicle across an endless,
mutating mathematical desert while an escalating swarm chases; survival is forgiving, but damage is
gated behind **verticality** — launch off dunes, flip, and slam into the crowd. Built with Unity 6
DOTS (ECS, Burst, Unity Physics) and HDRP.

**`ARCHITECTURE.md` is the canonical design + technical-architecture document.** It records the decided
approach and the rationale for several deliberate reversals of the brainstorm docs. Read it before
making design or terrain/vehicle decisions. Key settled choices (do not re-open without the triggers
listed there):
- Swarm is **~1k–5k ECS entities (Burst)**, not 500k GPU compute.
- **Stay HDRP** (not URP).
- Terrain is an **analytical sum-of-sines formula** rendered on a **world-space follow-grid** (not
  Unity Terrain, not floating origin).
- Maximum reuse of the existing ECS survivors infrastructure; the genuinely new work is the formula
  terrain, the arcade vehicle physics, and the verticality (slam/stun) loop.

## Architecture (current codebase)

### ECS Structure
- **Components**: Data-only `IComponentData` structs are colocated with the system or authoring file
  that owns them (e.g. `SpatialGrid` is in `SpatialGridSystem.cs`). The `ShipECS/Components` and
  `ShipECS/Data` folders exist but are empty scaffolding.
- **Systems**: `Assets/Scripts/ShipECS/Systems/` — game logic.
- **Entities**: `Assets/Scripts/ShipECS/Entities/` — entity/aspect definitions. (Aspects have been
  migrated off the deprecated `IAspect` to plain readonly structs + constructors.)
- **Blob assets**: `Assets/Scripts/BlobAsset/`.
- **Authoring**: `Assets/Scripts/Authoring/` — MonoBehaviours baked to ECS.
- **Terrain (new)**: `Assets/Scripts/Terrain/` — `TerrainNoise`/`TerrainGenerator` (prototype height +
  Unity Terrain, validates the sampling seam) and `BuggyTerrainSystem` (ECS height-follow). Being
  migrated to the displaced follow-grid per `ARCHITECTURE.md`.

### System Groups
`PausableSystemGroup` (and `Pre/Post/InPhysics` variants) gate updates on pause. Order with
`[UpdateAfter]`/`[UpdateBefore]`.

## Development

- **Unity** 6000.x, HDRP 17.x, Unity DOTS 1.3.x.
- **Key deps**: `com.unity.entities`, `com.unity.physics`, `com.unity.entities.graphics`,
  `com.unity.render-pipelines.high-definition`, `com.unity.inputsystem`.
- **Build**: Unity project — build via the Editor. No custom build scripts.
- **There is no compiler in the Claude environment.** Shaders, GPU code, and C# can be written here but
  must be verified in the user's Unity Editor. Work in small, verifiable slices (see the Implementation
  Plan in `ARCHITECTURE.md`).
- **Scenes**: `Assets/Scenes/TerrainTest.unity` (current terrain work). Legacy ship scenes
  (`CameraFollow ECS`, `Movement`, `VFX/VFX Lessons`) remain as reference.

## Code Patterns
- `IJobEntity` / `IJobChunk` + Burst for parallel work; `RefRO<T>`/`RefRW<T>` access; `ComponentLookup<T>`
  for random access; `NativeArray` / `NativeParallelMultiHashMap` for data structures.
- VFX goes through `VFXSystem`'s `VFXManager<T>` buffer pattern (request struct → GraphicsBuffer →
  single `SendEvent`). Never stand up a parallel particle path.
- ScriptableObjects in `Assets/ScriptableObjects/`, prefabs in `Assets/Prefabs/`.

## Engineering Invariants (from ARCHITECTURE.md §8 — these are hard rules)
1. **CPU/GPU height parity is sacred** — the height formula lives in C# (physics) and HLSL (render) and
   must match exactly. Sum-of-sines only; no gradient noise, no `sin`-hash.
2. **Integer hashes** for procedural placement (trees/scatter) — never `frac(sin()*big)` across the
   CPU/GPU boundary.
3. **Deformation is CPU-owned** — CPU writes/samples the deformation field for physics, uploads a copy
   to the GPU for render. Never GPU-only.
4. **World space, not floating origin** — the buggy moves; the grid follows (cell-snapped).
5. **Single VFX path** through `VFXManager<T>`.
