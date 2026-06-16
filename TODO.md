# TODO

Working roadmap for the pivot from space-ship survivors to a dune-buggy survivors game
(working title: SANDSLAM). The DOTS infrastructure is mostly reusable; the new identity
is the terrain spine + trick/landing loop. Build order follows "prove the one jump first."

## Now — Milestone 0: "The One Jump" (prove the core verb is fun)
- [ ] `TerrainNoise` static class: `SampleHeight(worldPos)` + `SampleGradient(worldPos)`, Burst, with a self-check on the amplitude/gradient invariant
- [ ] `FakeHeight` component + `FakeHeightSystem` (gravity integrate, pin to terrain when grounded) — repurpose the y-axis `CharacterMovementSystem` currently throws away
- [ ] `LaunchSystem` — manual hop input for the prototype (crest-launch needs terrain, that's M1)
- [ ] `TrickRotationSystem` — accumulate rotation, bank spins, scale payload, shrink landing tolerance
- [ ] `LandingResolveSystem` — clean vs. bail check; clean emits a slam
- [ ] Slam = adapt `ArtilleryExplosionSystem` (`SphereCastAll` -> `ProcessHitsJob`) with landing pos + payload radius/damage. Reuses `VFXExplosionRequest` for the burst.
- [ ] `ShadowSystem` + landing juice (shake, freeze-frame, shockwave) — "the landing is the punch"
- Goal: does a single launch -> rotate -> clean/bail -> slam feel good in a bare sandbox? If not, stop and rethink.

## Next — Milestone 1: Terrain comes alive
- [ ] `TerrainGenerator` MonoBehaviour: fill `TerrainData.SetHeights` from `TerrainNoise` + seed at run-start (bounded arena, no streaming)
- [ ] Cartoon sand material: 1-2 textures (base + optional steep-slope splat), constants for roughness/metallic, no normal map
- [ ] Arena bounds (walls / soft push-back at the edge)
- [ ] Flow field: derive `CostField` from terrain gradient, Dijkstra/BFS sweep from player cell, gradient pass -> per-cell arrows. Swap the one steering line in `FollowPlayerJob` to read the field. (Start with flat cost = identical behavior, then plug terrain in.)
- [ ] Crest-launch: wire `LaunchSystem` to terrain shape (emergent, no scripted ramps)

## Later — Milestones 2-4
- [ ] Re-skin AutoWeapon as buggy mounts; add buggy upgrade slots (chassis/engine/wheels) to existing level-up draft
- [ ] Tune payload scaling, landing-window shrink curve, bail vulnerability, terrain steepness/cost
- [ ] Buggy variants, persistent meta upgrades, enemy types, biomes (= heightfield authoring styles)

## Weapon generation system — refinement (kept, needs a hard look)
The `NonECS/WeaponGeneration/` codegen (~1,566 lines: codegen, JSON importer, templates,
example weapons) was built ahead of the core loop being proven. Decide its fate before
investing further:
- [ ] Audit actual usage: are any generated weapons live in-game, or is it all scaffolding?
- [ ] Decide scope: full codegen framework vs. a thinner data-driven `WeaponDefinition` -> runtime path (codegen earns its keep only if weapon variety is high and churn is frequent)
- [ ] Reframe for the buggy: auto-fire mounts vs. ship weapons — check target-leading assumptions (buggy is momentum/heading-committed, not free 360 strafe)
- [ ] Reconcile docs: README documents this as "the recommended workflow" — keep accurate to whatever survives the refinement

## Docs / cleanup debt
- [ ] README.md is 526 lines describing the pre-fork ship game with aspirational claims (e.g. "3000+ enemies at 60fps", unit tests that don't exist). Rewrite once the fork direction is locked — premature now.
- [ ] `ArtilleryExplosionSystem`: `state.Dependency.Complete()` runs per-explosion inside a nested loop (sync point) and there's a `break` after the first artillery aspect — revisit when adapting to the slam.
- [ ] Possible dead `using UnityEngine;` in `ArtilleryExplosionSystem` after Debug.Log removal (harmless warning).
