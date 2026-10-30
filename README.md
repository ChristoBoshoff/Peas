# Riftwake

Top-down action RPG for the **Peas** project — **version 2** full first campaign through **the Verge**.

Fresh IP only (Riftwake). Play the Windows build below, or open the Unity project in the Editor.

## Play now (Windows)

Distributable player (self-contained, no install):

```
Builds/Windows64/Riftwake.exe
```

1. Download / clone this repo (or grab the exe from the PR artifacts path above).
2. Double-click **`Riftwake.exe`** (Windows 10/11 x64).
3. First launch may extract native libs beside the exe — keep the folder writable.

Saves go to `%LocalAppData%\Riftwake\riftwake_save.json`.

### Controls

| Input | Action |
| --- | --- |
| WASD / Arrows | Move |
| Mouse | Aim |
| Left mouse | Primary attack / cast |
| Q | Secondary (unlocks at level 3) |
| E | Buy Field Charm (hub) |
| Enter / Space | Confirm / continue / depart hub |
| 1 / 2 / 3 | Path select |
| F5 | Save |
| R | Rise after falling |
| Esc | Main menu (progress saved) |

### Campaign flow

1. **Main menu** — New Campaign / Continue  
2. **Path select** — Steelhand · Wildbinder · Spellwright  
3. **Missions** — Bracken Hollow → Mireroad → Dunwick Crossing (hub) → Cinder Mines → Fellwood → Vergewatch Keep → Rift Heart (finale)  
4. **Ending** — campaign complete; return to menu or start new  

Progression: XP, levels, gold, Vitality/damage bonuses, secondary ability at Lv 3. Hub shop: Field Charm.

### Path primaries / secondaries

| Path | LMB | Q (Lv 3+) |
| --- | --- | --- |
| Steelhand | Cleaving Strike | Iron Guard |
| Wildbinder | Thorn Dart | Vine Snare |
| Spellwright | Ember Bolt (Focus) | Flashflare (Focus) |

## Unity Editor (develop / alternate play)

- **Unity 2022.3 LTS** (project version `2022.3.50f1`)
- Built-in Render Pipeline
- Open the **repo root** in Unity Hub
- Play **`Assets/Scenes/MainMenu.unity`** (Build Settings scene 0)

Scenes: `MainMenu` → `ClassSelect` → `Mission` → `Ending` (plus legacy `BrackenHollow` route).

Unity mirrors the same campaign systems (menu, paths, missions, save, progression, ending). Runtime content is bootstrapped from primitives.

### Rebuild the Windows player

Unity Editor licensing is not available in this CI environment, so the checked-in Windows build is produced from the Raylib companion player (same campaign):

```bash
# Requires .NET 8 SDK
dotnet publish Player/Riftwake.Player/Riftwake.Player.csproj \
  -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -o Builds/Windows64
```

Output: `Builds/Windows64/Riftwake.exe`

## Project layout

```
Assets/                 Unity project (Editor)
Player/Riftwake.Player  Win64 player source (Raylib-cs)
Builds/Windows64/       Distributable Riftwake.exe
ProjectSettings/
Packages/
```

## Status

Full first campaign loop is playable via **`Builds/Windows64/Riftwake.exe`**. Placeholder / primitive art. Prefer working campaign over polish.
