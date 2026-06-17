# Project Dune-Crawler — Architecture & Design

> Working codenames: **Dune-Crawler** / SANDSLAM. Repo: `ProjectBuggy` (forked from a Unity DOTS
> space-ship survivors project). This document is the canonical, decided architecture. Where the
> source brainstorm docs (Gemini/GDD dumps) and this document disagree, **this document wins** — it
> records the engineering decisions made after review, including several deliberate reversals of the
> brainstorm. Read the "Decisions & Rationale" and "Engineering Invariants" sections before building.

---

## 1. The Game

A top-down **arcade dune-buggy survivor**. You pilot a fast, agile vehicle across an endless,
mutating desert while an escalating swarm closes in. Survival is forgiving (auto-weapons handle the
floor); **skill expression lives in verticality** — launch off dunes, flip Rocket-League-style, and
slam back into the crowd. A clean slope-matched landing detonates a shockwave; a bad landing stuns
you, prone in the swarm. The arena itself is a weapon: terrain deforms (the "brittle eggshell"
crust), level-up cards mutate the world (cliffs, quicksand), and the swarm funnels through the
geometry you shape.

**Pillars**
1. Survival forgiving, damage skillful — the skill ceiling is on the *reward* axis, not the *survival* axis.
2. The landing is the punch — all the juice lands on a clean touchdown.
3. Every jump is a live risk/reward decision (greed vs. landing tolerance).
4. The terrain is a live participant — it launches you, channels the swarm, and mutates with progression.

---

## 2. Decisions & Rationale (the contested ones)

These are the decisions that reversed the brainstorm docs. Each lists what the brainstorm proposed,
what we chose, and why. **Do not re-open without the listed trigger.**

| Topic | Brainstorm proposed | **Decided** | Why | Reopen trigger |
|---|---|---|---|---|
| **Swarm scale** | 500,000 GPU compute boids | **~1k–5k ECS entities (Burst)** | A screen fits ~3000; 500k is a vanity metric. 500k was the *only* thing justifying the entire GPU-compute rewrite. ECS+Burst already does 3000+. | Genuinely need >30k *visible* |
| **Swarm sim** | GPU compute shader | **Reuse existing ECS** (movement, spatial grid, spawn, damage, death) | At thousands, CPU Burst is comfortable and the existing systems already do it. | Same as above |
| **Render pipeline** | URP | **Stay HDRP** | Every GPU technique here (compute, indirect, vertex displacement) is pipeline-agnostic. Migrating is pure disruption for zero gameplay gain. | A concrete URP-only win appears |
| **World origin** | Floating origin (buggy locked at 0,0,0, world scrolls) | **Follow-grid in world space** (buggy moves; terrain grid follows, snapped to cell size) | Precision is the only real benefit, and the math kills it: float32 ULP at 48 km ≈ **6 mm** — invisible. Floating origin would force counter-offsetting every dynamic entity and inverting movement/spawn/camera. Same infinite-desert visual either way. | Pivot to GPU-scale swarm (>30k) |
| **Terrain tech** | (varied) | **Analytical formula + displaced follow-grid mesh** (not Unity Terrain) | Terrain is a *formula*, nothing to store. Adjustability (level-up world mutation) is O(1) params on a formula vs. rewriting a baked array. Unity Terrain can't go infinite without painful tile streaming. | — |
| **Height noise** | fbm / gradient noise | **Sum-of-sines** | CPU physics samples the height; GPU displaces the mesh; they **must** match exactly. Sum-of-sines is trivially bit-similar across C# and HLSL; gradient-noise implementations diverge. | — |
| **Tree placement hash** | `frac(sin(cell·k)*43758)` | **Integer bit-hash** | `sin`-based hashing is *not* portable CPU↔GPU; the tiny `sin` difference is amplified by the big multiply, so tree collision desyncs from the rendered tree. Integer ops are bit-identical both sides. | — |
| **Deformation map** | GPU-only Render Texture | **CPU-owned array, uploaded to GPU** | If only the shader reads it, only the *visual* collapses; physics wouldn't fall into craters. CPU owns it, samples it for physics, uploads a copy for rendering. | — |
| **Cliff terracing** | hard `round()` steps | **Softened (smoothstep) terraces** | Hard `round()` makes every step an impassable vertical wall everywhere → buggy can't move, swarm jams. Soften to steep climbable ramps. | — |
| **Tree rendering** | `RenderMeshIndirect` (compute path) | **Standard GPU instancing** | At ~200–300 visible trees, indirect/compute is overkill; `Graphics.RenderMeshInstanced` / DOTS rendering is plenty. | High tree density at scale |

**Net effect:** dropping 500k unwinds the entire GPU rewrite. The project lands at **the original fork
plan (max reuse of the ECS survivors infrastructure) + the arcade-vehicle / verticality identity from
Dune-Crawler.** Closer to a graft than a rewrite.

---

## 3. System Salvage Map (what of the existing repo survives)

The dividing line: **player/meta-side survives (ECS, world-space); the swarm stays ECS too because we
dropped 500k.** Almost everything is reused.

**KEEP — reuse as-is or nearly**
- `VFXSystem` — the buffer-based GPU VFX manager (`VFXManager<T>`, request structs → GraphicsBuffer →
  single `SendEvent`). The crown jewel: it already speaks exactly the buffer-request language tracers,
  sparks, slam dust, and sand puffs need. Extend with new request structs; never build a parallel path.
- `EnemyMovementSystem`, `SpatialGridSystem` — per-entity steering + spatial hash. Add terrain-normal
  slope-repulsion (see §5) for channeling. (A flow field is optional later; not required at this scale.)
- `EnemySpawnSystem` / spawn cadence — reuse; spawn relative to the buggy's frontier.
- `DamageCollisionSystem`, `KnockbackSystem`, health, death, `DeadComponentCleanupSystem` — reuse.
- Progression: `ExperienceGatherAndNotify`, `LevelUpNotifySystem`, `UpgradeApplicationSystem` + Upgrade
  UI — reuse. Upgrade cards now *also* push terrain modifier params (see §4.2).
- `LootDropSystem`, `ScrapMovementSystem` — reuse (gems float to player).
- `TimeManagerSystem`, `GamePauseSystem`, `PausableSystemGroup` — reuse (pause + run timer).
- `HealthComponent`/`HealthNotifySystem` — reuse for the **buggy** (chassis HP).
- UI (`GameUIView`, `UIManager`, `DamageNumberUIManager`) — reuse.
- Weapon/projectile systems — reuse at this scale; hitscan is an option, not a requirement (see §6).

**REPLACE / NEW**
- `CharacterMovementSystem` (twin-stick, y=0 pinned) → **arcade vehicle physics** (§5). The one real
  movement rewrite: ship twin-stick → heading-committed buggy momentum.
- Terrain: **new** formula + displaced follow-grid + deformation field (§4).

---

## 4. Terrain Architecture

Terrain is **Analytical Mathematical Geometry**: a deterministic formula evaluated identically on CPU
(physics) and GPU (rendering). No stored heightmap for the base shape. Three layers, evaluated together:

```
LAYER 3  OBSTACLE MATRIX     deterministic integer-hash grid -> trees / pillars / ruins
LAYER 2  DEFORMATION FIELD   CPU-owned scrolling array -> craters, level-up cliffs, eggshell pits
LAYER 1  BASE FORMULA        sum-of-sines -> macro dunes + micro ripples
```

### 4.1 Layer 1 — Base formula (sum-of-sines)

The single source of truth. **MUST be byte-identical in C# and HLSL.** Sum-of-sines (not noise) for
exactly that reason. Current prototype uses `TerrainNoise` (fbm) on a Unity Terrain to validate the
sampling seam; the **target** is this shared formula sampled by physics and displaced on the grid.

```
height(p, modifiers) =
      macroDunes(p)          // low frequency, high amplitude
    + microRipples(p)        // high frequency, low amplitude
    + terraceMix(p, modifiers._CliffsModifier)   // Layer-2 global modifier (§4.2)
    + deformation.Sample(p)  // Layer-2 local field (§4.3)
```

Rules:
- Sum of a small fixed set of sine terms with a domain-warp for irregularity. No `sin`-based hashing,
  no gradient noise — keep it portable.
- Takes a `modifiers` struct (global params) **from day one** so world-mutation cards are free (§4.2).
- Gradient/normal by central difference (sample ±ε on x and z); normal = `normalize(-dhdx, 1, -dhdz)`.

### 4.2 Layer 2a — Global modifiers (level-up world mutation) — *free*

Cliffs, quicksand intensity, "seismic instability" cards. These are **global uniforms** fed to the
formula; one float morphs the entire infinite world on CPU and GPU at once, zero storage.

```hlsl
float _CliffsModifier; // 0 = smooth dunes ... 1 = canyons
// SOFTENED terracing (NOT hard round() — that makes every step an impassable wall):
float terraced = smoothstep_terrace(baseDunes, stepHeight);
return lerp(baseDunes, terraced, _CliffsModifier);
```

The C# height function takes the same `modifiers` struct so suspension reads the mutated world
identically. Upgrade cards set these params via the existing `UpgradeApplicationSystem`.

### 4.3 Layer 2b — Local deformation field (craters / eggshell) — *CPU-owned*

Localized, persistent edits a global formula can't express. **The field is a CPU-owned array**
(a `NativeArray`/`NativeArray2D` scrolling around the player), NOT a GPU-only Render Texture.

Lifecycle:
1. **Trigger** — heavy landing / explosive impact / seismic mortar.
2. **Paint** — CPU writes a negative delta (e.g. −15 m "eggshell collapse") into the field cells under
   that world coordinate.
3. **Physics** — suspension + enemy Y sample `height(p) + field.Sample(p)` on the CPU → they actually
   fall into the pit.
4. **Render** — CPU uploads a copy of the field to the displacement shader each frame; the GPU mesh
   caves in to match. VFX manager spawns rock-shard quads at the break edge.

Resolution: a modest field around the player (e.g. 256² covering ~200 m) — cheap to sample and upload.
The sharpness of the −15 m drop at cell edges creates the steep walls the slope-deflection (§5.3)
reads as impassable, trapping the swarm in pits automatically.

> **Coupling warning:** deformable pits trap the *buggy* too. Escape requires the jump/air mechanic.
> Do not ship craters before air control works, or the player can soft-lock in a pit.

### 4.4 Layer 3 — Obstacle matrix (trees / pillars) — integer hash

Positions are computed, not stored. A deterministic **integer** hash per grid cell decides if/where an
obstacle exists. Evaluated by the CPU physics job (collision) and the GPU vertex shader (render) — so
the hash **must** be bit-identical:

```
// identical in HLSL and C# (uint math is deterministic both sides)
uint Hash(uint x){ x^=x>>16; x*=0x7feb352d; x^=x>>15; x*=0x846ca68b; x^=x>>16; return x; }
float r = Hash(cell.x*73856093u ^ cell.y*19349663u) / 4294967295.0; // [0,1)
// r < density -> obstacle here; derive offset + trunkRadius from more hash bits
```

- **Render:** standard GPU instancing (`Graphics.RenderMeshInstanced` / DOTS rendering) — not indirect;
  ~200–300 visible. Tree Y anchored via `height()` incl. deformation → trees sink into craters for free.
- **Buggy collision:** cylinder push-out against nearest cells' trunks; deflect velocity into the trunk,
  stun if hit fast (§5.3).
- **Swarm:** each blob adds a cylinder-repulsion vector away from nearby trunks → horde parts around
  trees like water.

### 4.5 Rendering — follow-grid (NOT floating origin)

A 512² grid mesh whose GameObject follows the buggy's XZ, **snapped to cell size** (so vertices land on
stable world sample points and the surface doesn't swim). The vertex shader samples `height(worldPos,
modifiers) + deformation` and displaces Y; normal for lighting from the same gradient. The buggy moves
in world space; precision is fine to hundreds of km. Biome = material/texture swap only (§4.6).

### 4.6 Multi-biome reskinning

Geometry (math) is decoupled from presentation (material). Swap surface texture, subsurface-cavity
texture, and VFX particle palette via `MaterialPropertyBlock` — physics and steering unchanged. Biomes:
Glacial Shelf, Volcanic Fields, Overgrown Exo-Planet, Megastructure Scrapyard (see source GDD for the
art table).

---

## 5. Vehicle Architecture (arcade physics, ECS)

Arcade feel comes from **direct vector manipulation**, not a physics solver. Suspension only keeps the
buggy hovering over the formula; drifting, jumping, flipping all override velocity/rotation directly.

### 5.1 Components

```csharp
public struct PlayerVehicleTag : IComponentData {}

public struct VehiclePhysicsData : IComponentData
{
    public float3 Velocity;
    public float3 AngularVelocity;  // aerial flips
    public float  CurrentSpeed;
    public bool   IsGrounded;
    public bool   IsDrifting;
    public bool   IsStunned;
    public float  StunTimer;
    // config (per archetype)
    public float TopSpeed, Acceleration, DriftGrip, AirControlSpeed;
}

public struct WheelSuspensionData : IComponentData
{
    public float3 LocalOffset;
    public float  RestLength;       // hover height
    public float  SpringStiffness;  // K
    public float  DamperStiffness;  // D
    public float  LookAheadTime;    // predictive sample window (~0.1s)
}
// Variable wheel counts (motorcycle 2 ... semi 8) -> DynamicBuffer<WheelRef> on the vehicle.
```

`TerrainNoiseSingleton { NoiseParams Noise; float3 Origin; }` already bridges the formula params into
ECS (pushed by the terrain generator). Suspension samples the shared height function through it.

### 5.2 Build order (do not skip ahead)
1. **Suspension bed** — buggy hovers over the formula at `RestLength`. *(DONE in prototype as
   `BuggyTerrainSystem`: sets player Y from the sampled height; reuses existing movement for X/Z.)*
2. **Arcade throttle + steering** — replace twin-stick `CharacterMovementSystem` with heading-committed
   momentum (throttle along facing, steer rotates facing). **The ship→buggy fork point.**
3. **Drift** — split velocity into forward + lateral; erase lateral normally, keep a `DriftGrip`
   fraction while the drift button is held → tail slides while nose spins.
4. **Air control + jump** — on `!IsGrounded`, input drives `AngularVelocity` (roll/pitch/yaw); jump =
   instant `Velocity.y = JumpForce` override; double-jump/flip = impulse along local up/side.
5. **Stun fail + slam reward** — §7.

### 5.3 Predictive suspension & deflection
- **Look-ahead:** sample `height()` at `P + Velocity*LookAheadTime`; if it spikes, pre-stiffen
  `DamperStiffness` to brace. Must be robust to *frequent* discontinuities (terraces, crater edges) —
  clamp the per-frame displacement so a sudden step can't launch the buggy to orbit.
- **Slope deflection (cliffs/trees):** `steep = dot(groundNormal, up) < 0.7` (~45°) ⇒ impassable.
  Subtract the velocity component going *into* the wall (slide along it); stun if speed was high.

---

## 6. Swarm, Weapons, VFX

- **Swarm (ECS, thousands):** reuse `EnemyMovementSystem` + `SpatialGridSystem`. Add a per-enemy
  **slope-repulsion** term (sample terrain normal; if steep, push away) — this is what funnels the
  horde into valleys and traps it in pits, identical math to the vehicle deflection. At thousands the
  extra 4 height samples/enemy/frame are trivial on Burst.
- **Weapons:** reuse existing projectile/weapon ECS. Hitscan + GPU tracers (via the VFX buffer manager)
  is an *option* for a machine-gun feel, not a requirement at this scale. Terrain-deformer weapons
  (seismic mortar) write craters into the Layer-2 deformation field.
- **VFX:** everything routes through `VFXSystem`'s `VFXManager<T>` buffer pattern — tracers, sparks,
  slam dust, sand puffs. New effect = new request struct + a manager block (mirror the explosions
  block) + a VFX Graph reading the buffer. Tire tracks / persistent marks = HDRP Decal Projector
  (separate from the buffer particles).

---

## 7. Verticality Skill Loop (the core verb)

```
KITE → herd the swarm into a cluster
LAUNCH → crest a dune at speed (emergent) or hop; IsGrounded = false
FLIP → input drives AngularVelocity; greed scales the payload, tightens the landing window
LAND → compare chassis up-vector to terrain surface normal:
        CLEAN (within tolerance) → speed boost + Kinetic Ground Slam: radial force on nearby
               enemies (reuse the existing artillery radius-query AoE) + slam VFX
        BAD   (>30° off, or roof-first) → IsStunned, StunTimer=1.5s, Velocity zeroed, inputs locked;
               the swarm closes in. (Simplified fail state — ~20 lines, no recovery minigame.)
```

The slam reuses the existing AoE radius query into the spatial hash. The stun is the entire failure
state — no wreck/recovery state, deliberately kept lean.

---

## 8. Engineering Invariants (the hard rules)

1. **CPU/GPU height parity is sacred.** The height function exists in C# (physics) and HLSL (render) and
   must produce identical results. Sum-of-sines only; no gradient noise, no `sin`-hash. Edit one, edit
   the other — they live as a documented pair.
2. **Integer hashes for any procedural placement** (trees, scatter). Never `frac(sin()*big)` across the
   CPU/GPU boundary.
3. **Deformation is CPU-owned.** The CPU writes and samples the deformation field for physics, then
   uploads a copy to the GPU for rendering. Never GPU-only (no readback path).
4. **World space, not floating origin.** The buggy moves; the grid follows (cell-snapped). Revisit only
   if the swarm goes GPU-scale.
5. **Single VFX path.** All particles through `VFXManager<T>`. No parallel particle systems.
6. **Suspension must survive discontinuities.** Clamp per-frame vertical correction so terraces and
   crater edges can't launch the vehicle.
7. **Don't ship craters before air control** — deformable pits can trap the player; escape needs jumps.

---

## 9. Implementation Plan & Process

### 9.1 Working process (how we actually build this)

The build runs as a loop of small, individually-verifiable **slices**. The constraints below are not
bureaucracy — they're forced by the environment and the architecture.

- **No compiler in the Claude environment.** All C#/HLSL/shader code is written blind and **verified in
  the user's Unity Editor.** Therefore every slice must end at *something observable* — it compiles and
  produces a stated visible/runnable result. No slice is "done" on faith.
- **One slice = one focused change with a Definition of Done.** Build the minimum that proves the slice,
  not the surrounding scaffolding. (Ponytail: shortest working diff; defer everything not needed yet.)
- **Commit cadence:** one commit per *working, verified* slice, straight to `main` (the user's
  preference). **Stage only the slice's files** — the repo carries unrelated `.meta`/asset/ProjectSettings
  churn, so never `git add -A`; list paths explicitly. End commit messages with the Co-Authored-By line.
- **Parity discipline (invariant 1 & 2):** any edit to the height formula or a placement hash changes the
  C# *and* the HLSL in the **same slice**. They are a documented pair; never let a slice land that touches
  one without the other.
- **Sequencing guard (invariant 7):** air control (1.4) must land before deformation ships, and
  deformation before the mortar weapon (5.2) — or the player can soft-lock in a self-made pit.
- **Per-slice Definition of Done:** (a) compiles in Unity, (b) the stated observable behavior is true,
  (c) committed with only the slice's files.

### 9.2 Phase plan (slices, dependencies, Definition of Done)

Legend: ✅ done · ⏭ next · ☐ planned.

**Phase 0 — Terrain foundation**
- ✅ 0.1 Height-sampling seam. `TerrainNoise` (fbm) + `TerrainGenerator` (Unity Terrain) +
  `BuggyTerrainSystem` (ECS). *DoD met:* a cube/player rides the dunes; gameplay samples the same
  surface the mesh renders.
- ⏭ 0.2 **Shared sum-of-sines formula** (`TerrainField`, C# + HLSL pair) with a `modifiers` param stub.
  *DoD:* C# self-check passes; HLSL include compiles. *(Reverts the fbm choice — see §2.)*
- ☐ 0.3 **Displaced follow-grid floor.** 512² grid mesh follows the buggy XZ (cell-snapped); vertex
  shader displaces Y from the formula; normal for lighting. Retire `TerrainGenerator`/Unity Terrain.
  *DoD:* drive a dummy object, endless dunes scroll with no surface "swimming."
- ☐ 0.4 Repoint `BuggyTerrainSystem` to `TerrainField`. *DoD:* buggy sits exactly on the displaced floor.

**Phase 1 — Arcade vehicle (the identity; build order is strict)**
- ✅ 1.1 Suspension bed (ECS height-follow exists). Evolve to hover at `RestLength`.
- ☐ 1.2 **Arcade throttle + steering** — replace twin-stick `CharacterMovementSystem` with
  heading-committed momentum. *The ship→buggy fork point. DoD:* drive around the dunes, car-like.
- ☐ 1.3 **Drift** — split velocity fwd/lateral; erase lateral normally, keep `DriftGrip` fraction on the
  drift button. *DoD:* hold drift → tail slides while nose turns.
- ☐ 1.4 **Air control + jump** — `IsGrounded` flag; input drives `AngularVelocity`; jump =
  `Velocity.y = JumpForce` override; flip impulse. *DoD:* crest a dune, flip in the air.
- ☐ 1.5 **Predictive look-ahead + slope deflection** — brace `DamperStiffness` on detected spikes; slide
  along >45° walls; clamp per-frame vertical correction. *DoD:* high speed over rough terrain doesn't
  launch to orbit; can't climb a cliff.

**Phase 2 — Verticality loop (the verb)**
- ☐ 2.1 Landing evaluation — chassis up-vector vs surface normal at touchdown.
- ☐ 2.2 **Clean slam** — reuse the existing artillery AoE radius-query → radial force/damage on nearby
  enemies + slam VFX via `VFXManager`. *DoD:* a clean landing clears a crowd.
- ☐ 2.3 **Bad-landing stun** — `IsStunned`, 1.5 s, zero velocity, lock input. *DoD:* a bad landing leaves
  you stuck and swarmed.

**Phase 3 — Swarm on terrain (reuse + augment)**
- ☐ 3.1 Confirm the reused swarm (`EnemyMovementSystem`/`SpatialGridSystem`/spawn) rides terrain Y.
- ☐ 3.2 **Slope-repulsion** term in `EnemyMovementSystem` (sample terrain normal; push from steep).
  *DoD:* the horde funnels through valleys / around cliffs.
- ☐ 3.3 Spawn relative to the buggy's frontier; recycle distant enemies.

**Phase 4 — Survivors meta (reuse)**
- ☐ 4.1 XP / level-up draft (reuse) wired to buggy kills.
- ☐ 4.2 **World-mutation cards** set `_CliffsModifier` etc. via `UpgradeApplicationSystem`. *DoD:* pick a
  card → the world morphs.
- ☐ 4.3 Survival timer + win/loss condition.

**Phase 5 — Weapons + VFX (reuse + extend)**
- ☐ 5.1 Reuse weapon ECS as buggy auto-fire mounts.
- ☐ 5.2 **Terrain-deformer mortar** → writes the deformation field. *Depends on Phase 6 deformation +
  1.4 air control (invariant 7).*
- ☐ 5.3 Tracer VFX via the buffer manager.

**Phase 6+ — Content (deferred)**
- ☐ Deformation/eggshell field (CPU-owned; §4.3) · trees (integer hash + instancing; §4.4) ·
  biome reskins (§4.6) · boss (giant blob) · vehicle archetypes.

### 9.3 Immediate next step
**Slice 0.2** — revive `TerrainField` as the shared sum-of-sines formula (C# + HLSL, with a `modifiers`
param), then **0.3** the displaced follow-grid floor. The fbm `TerrainNoise`/`TerrainGenerator` stay as a
working reference until 0.3 renders, then retire.

> Uncommitted as of this doc: `TerrainNoiseSingleton`, `BuggyTerrainSystem`, the `TerrainGenerator` ECS
> push, and these consolidated docs.

---

## 10. Open Questions / Deferred
- Exact base-formula sine constants (tuning, not architecture).
- Vehicle archetype matrix (Tank/Motorcycle/Ice-Cream-Truck/Semi/Buggy) — data profiles over the same
  systems, with a `DynamicBuffer<WheelRef>` for variable wheel counts; post-MVP.
- Boss patterns (Giga-Blob crust-crusher, Sand-Leviathan, Nanite Maelstrom) — global coordinate
  influencers; post-MVP.
- Whether a flow field ever replaces per-entity steering (only if channeling needs it).
- **Weapon-generation system** (`NonECS/WeaponGeneration/`, ~1,566 lines of codegen) — audit actual
  usage and decide: keep the full codegen framework, or thin it to a data-driven `WeaponDefinition` →
  runtime path. Built ahead of the loop being proven; revisit when wiring Phase 5 weapons. Check target-
  leading assumptions (buggy is momentum/heading-committed, not free strafe).
- Minor code-debt to clean when adapting artillery into the slam (Phase 2.2): `ArtilleryExplosionSystem`
  does a `state.Dependency.Complete()` per explosion inside a nested loop, and a possible dead
  `using UnityEngine;` after earlier `Debug.Log` removal.
