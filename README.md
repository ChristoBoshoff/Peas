# Riftwake

Top-down action RPG prototype in Unity — **version 2** first playthrough slice for the Peas project.

You wake as a **Riftborn** in **Bracken Hollow**, choose a path, learn the controls from **Elder Bram**, and survive **Ashmites**.

## Requirements

- **Unity 2022.3 LTS** (project recorded as `2022.3.50f1`)
- Built-in Render Pipeline (no URP/HDRP package required)
- Windows / macOS / Linux Editor

## Open & play

1. Clone this repo.
2. Open the folder in Unity Hub → **Add** → select the repo root (the folder that contains `Assets/` and `ProjectSettings/`).
3. Let Unity import scripts on first open.
4. Open **`Assets/Scenes/ClassSelect.unity`**.
5. Press Play.

`ClassSelect` is also scene index 0 in Build Settings. You can play **`Assets/Scenes/BrackenHollow.unity`** directly; it defaults to the Steelhand path if none was chosen.

## First playthrough flow

1. **Choose a path** — Steelhand, Wildbinder, or Spellwright (keys `1` / `2` / `3`, then Enter).
2. Enter **Bracken Hollow** — short arrival into the Verge.
3. Follow **Elder Bram** prompts — move, aim, use your primary.
4. Clear **Ashmites** that chase and melee when close.

## Controls

| Input | Action |
| --- | --- |
| WASD / Arrow keys | Move |
| Mouse | Aim / face |
| Left mouse button | Primary (path-gated) |
| 1 / 2 / 3 | Highlight path on class select |
| Enter / Space | Confirm path and enter Bracken Hollow |
| R | Rise again after falling |

### Path primaries

| Path | Primary | Notes |
| --- | --- | --- |
| **Steelhand** | Cleaving Strike | Close melee arc |
| **Wildbinder** | Thorn Dart | Ranged nature dart |
| **Spellwright** | Ember Bolt | Ranged bolt; spends Focus |

## Project layout

```
Assets/
  Scenes/           ClassSelect, BrackenHollow
  Scripts/
    Game/           Session + hero paths
    Player/         Move + combat
    Combat/         Vitality, Focus, projectiles, melee
    Enemy/          Ashmite AI
    Camera/         Isometric follow
    UI/             Class select + HUD
    NPCs/           Elder Bram
    World/          Tutorial + scene bootstraps
ProjectSettings/
Packages/
```

Scenes bootstrap most of the playable content at runtime from primitives so the first slice stays easy to open and extend.

## Status

Playable first slice: path select → Bracken Hollow tutorial combat loop. Placeholder primitives only — art pass later.
