# Project Dune-Crawler

A top-down **arcade dune-buggy survivor** built with Unity 6 DOTS (ECS, Burst, Unity Physics) and HDRP.

Drive a fast buggy across an endless, mutating mathematical desert while an escalating swarm closes in.
Survival is forgiving — auto-weapons handle the floor — but your damage ceiling lives in **verticality**:
launch off dunes, flip, and slam back into the crowd. Nail a slope-matched landing and you detonate a
shockwave; botch it and you're stunned, prone in the swarm. The terrain itself is a weapon — it deforms,
mutates with your level-up choices, and funnels the horde through the geometry you shape.

> Forked from a Unity DOTS space-ship survivors project. Most of the ECS survivors infrastructure
> (swarm, spawning, damage, XP/upgrades, the buffer-based VFX manager) is reused; the new work is the
> formula terrain, arcade vehicle physics, and the verticality loop.

![Unity](https://img.shields.io/badge/Unity-6000.x-blue)
![DOTS](https://img.shields.io/badge/DOTS-1.3.x-green)
![Pipeline](https://img.shields.io/badge/HDRP-17.x-orange)

## Documentation
- **[ARCHITECTURE.md](ARCHITECTURE.md)** — canonical design, technical architecture, decisions &
  rationale, engineering invariants, and the implementation plan. Start here.
- **[CLAUDE.md](CLAUDE.md)** — guidance for working in the codebase.

## Getting Started
1. Open in Unity 6000.x (HDRP). Unity will restore packages on first open.
2. Open `Assets/Scenes/TerrainTest.unity` for the current terrain/vehicle work.

## Status
Early prototype. The height-sampling seam (gameplay rides the same surface the mesh renders) is
validated; next is the displaced follow-grid floor and the arcade vehicle loop. See
[ARCHITECTURE.md](ARCHITECTURE.md) §9 for the phase plan.
