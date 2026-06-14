# 🐱 Minimal Duet Cats — Playable Ads Prototype

> **Amanotes Case Study — Playable Ads Developer**  
> *"Capture the core rhythm-catcher gameplay of Duet Cats in a minimal, WebGL-optimized playable ad."*

![Unity](https://img.shields.io/badge/Unity-6000.3_LTS-000?logo=unity&style=flat)
![Target](https://img.shields.io/badge/Target-WebGL-blueviolet)
![Size](https://img.shields.io/badge/Textures-1.3%20MB-green)
![Status](https://img.shields.io/badge/Status-Playable-brightgreen)

---

## 🎯 Project Overview

A **minimal playable ad** version of [Duet Cats](https://play.google.com/store/apps/details?id=com.amanotes.gs.g06) — a rhythm game where two cats catch falling candies in sync with music. Built as a case study for the **Amanotes Playable Ads Developer** position.

The entire codebase is written in **hand-crafted C#** with zero visual scripting, focusing on:
- **Tiny build footprint** (textures: **~1.3 MB**) — critical for playable ads
- **Clean architecture** — event-driven, pooled, configurable
- **Production-quality animation** — Spine 2D skeletal animation + DOTween + Cartoon FX Remaster

---

## 📁 Project Structure

```
Assets/
├── Editor/
│   └── BuildSizeOptimizer.cs          # One-click WebGL build optimizer (Unity 6.3)
│
├── Game/
│   ├── Art/                           # ~1.3 MB total textures
│   │   ├── cat-skins/                 # Spine 2D atlas & skeleton data
│   │   ├── notes/                     # Candy sprites (Normal / Long / Strong)
│   │   └── ui/                        # UI assets & backgrounds (PT/LS variants)
│   ├── audio/
│   │   └── JsonMidi_BabyMonster.json  # Note chart data (MIDI-derived JSON)
│   ├── Data/                          # ScriptableObject configs
│   ├── Prefabs/                       # Unity prefabs
│   ├── Scripts/
│   │   ├── Candy/                     # Candy entity, factory & spawner
│   │   ├── Cat/                       # Cat controller & Spine animation
│   │   ├── Game/                      # Core loop, events, sound
│   │   ├── Input/                     # Unified touch/mouse input
│   │   ├── SO/                        # ScriptableObject definitions
│   │   └── VFX/                       # Pooled particle VFX system
│   ├── UI/                            # Screen flow (Home → Pick → GamePlay)
│   └── fonts/                         # TMP font assets
│
├── Scenes/
│   └── SampleScene.unity              # The single scene (root for everything)
│
├── Scripts/
│   └── GameSequenceController.cs      # Intro/outro sequence with vignette shader
│
├── Shaders/
│   ├── VignetteShader.shader          # Custom vignette transition shader
│   └── Paw.mat                        # Animated paw material
│
├── JMO Assets/Cartoon FX Remaster/    # Commercial VFX pack
├── Plugins/Demigiant/DOTween/         # Animation tweening library
└── Spine/                             # Spine 2D runtime (skeletal animation)
```

---

## 🏗️ Architecture & Key Design Patterns

### 🎯 **1. Event-Driven Game Loop** (`GameEvents.cs`)

A **static event aggregator** decouples every system in the game:

```csharp
public static class GameEvents
{
    public static event Action<VFXType, Vector2> SpawnVFX;
    public static event Action OnGameStart;
    public static event Action OnGameOver;
    public static event Action OnGameComplete;
    public static event Action OnScoreChanged;
    public static event Action OnGameReset;
}
```

No system references another directly — GameController raises events, UIManager, SoundController, VFXManager, and CatController subscribe independently. This makes the code **testable, extensible, and trivially reorderable** for different ad formats.

### ♻️ **2. Object Pooling Everywhere** (`CandyFactory.cs`, `VFXManager.cs`)

Both candies and VFX use Unity's `ObjectPool<T>`:

```csharp
_candyPool = new ObjectPool<Candy>(
    createFunc: () => Instantiate(_candyPrefab),
    actionOnGet: candy => candy.gameObject.SetActive(true),
    actionOnRelease: candy => candy.gameObject.SetActive(false),
    collectionCheck: false,
    defaultCapacity: 10,
    maxSize: 100
);
```

- ✅ **Zero allocations during gameplay** — no `Instantiate`/`Destroy` after startup
- ✅ **VFX auto-returns to pool** via `OnParticleSystemStopped()` callback
- ✅ Pool capacity tuned for WebGL memory constraints

### 🧩 **3. Interface-Based Hit Detection** (`ICandyEater.cs`)

```csharp
public interface ICandyEater
{
    public void OnEatCandy(int score);
}
```

`CatController` implements `ICandyEater`. `Candy.OnTriggerEnter2D` discovers the eater through the interface — **no hard references, no instanceof checks**. Adding a new eater (e.g., a power-up zone) means implementing the interface, touching zero existing code.

### 📋 **4. Config-Driven Everything** (ScriptableObjects)

| Config | Purpose |
|--------|---------|
| `SongConfig` | Holds audio clips, random song selection |
| `CandyConfig` | Maps candy IDs → sprites + score values |
| `CatConfig` | Stores Spine animation names (idle, eat, win, lose, start) |

Configs are **ScriptableObject assets** — tweakable in-editor without touching code. This is essential for playable ads where designers need to swap art/music rapidly.

### 🏭 **5. Factory Pattern** (`CandyFactory.cs` → `CandySpawner.cs`)

`CandyFactory` owns the pool and creates `Candy` instances. `CandySpawner` owns the **spatial logic** — mapping `pid` (note lane) to spawn points. Separation means:
- **Swap spawn logic** (e.g., random lanes, 3-lane → 4-lane) without touching pooling
- **Swap candy visuals** by changing the factory's prefab

### 🎬 **6. Screen Flow via UI Manager** (`UIManager.cs`)

Stack-based screen management using `Singleton<UIManager>`:

```
HomeScreen  →  PickSongScreen  →  GamePlayScreen
     ↑                              ↓
     └────────── GameReset ─────────┘
```

Each screen extends `UIBase` with DOTween fade transitions. Consistent pattern — all screens init, show, and hide the same way.

---

## 🎮 Core Gameplay Systems

### 📝 **Input System** (`InputReader.cs`)

A single pass divides screen space by world X:

```
Mouse X ≤ 0  →  LeftCatMove (left cat)
Mouse X > 0  →  RightCatMove (right cat)
```

Designed for **zero-allocation runtime** — no raycasts, no touch objects. Cat's `Drag(float x)` uses `Mathf.Clamp` + `Vector3.Lerp` for smooth, damped movement.

### 🍬 **Note Spawning** (`CandySpawner.cs`)

Notes are parsed from **JSON** (exported from MIDI) into `List<NoteData>`:

```json
{ "id": 1, "n": 101, "ta": 1.2857, "ts": 1.2857, "d": 0.1071, "v": 50, "pid": 0 }
```

- `ta` → spawn timing (synced to `AudioSource.time`)
- `pid` → lane mapping (0/2/3/5 → 4 lanes)
- `n` → candy type ID (resolved via `CandyConfig`)
- Notes are **sorted by timing** for O(1) next-note lookup

### 🎯 **Hit Detection** (`Candy.cs` + `GameController.cs`)

**Two-stage detection:**

1. **Miss check** — if `Candy.IsOutRange` (elapsed > hitTime + missWindow), game over
2. **Hit check** — `OnTriggerEnter2D` via `ICandyEater` interface

No per-frame distance calculations. No complex timing windows. Simple, fast, WebGL-friendly.

### 📊 **Scoring & Game Flow**

```csharp
AddScore(int score) → GameEvents.RaiseScoreChanged(_score)
```

- Win condition: `song complete + all candies eaten`
- Lose condition: `any candy passes the miss window`
- Reset: clears active candies, shows HomeScreen

### 🐱 **Cat Animation** (`CatAnimation.cs` + `CatController.cs`)

Spine skeletal animations are driven through a clean facade:

```
CatController  →  CatAnimation  →  SkeletonAnimation.state.SetAnimation()
```

`CatConfig` (ScriptableObject) stores animation names with Spine attribute validation — **impossible to type the wrong animation name**.

---

## 🚀 Build Size — Key Optimizations

### Texture Budget (~1.3 MB total)

| Category | Size | Technique |
|----------|------|-----------|
| Cat atlases | ~130 KB | Spine atlas + PNG |
| Candy sprites | ~10 KB | Individual small sprites |
| UI assets | ~430 KB | Crunch compression, max 1024px |
| Backgrounds | ~170 KB | JPG (lossy, no alpha) |
| **Total** | **~1.3 MB** | |

### Editor Tool: `BuildSizeOptimizer.cs`

A **one-click Unity Editor window** (`Tools → Build Size Optimizer`) that applies:

| Setting | Value |
|---------|-------|
| IL2CPP | Optimize Size |
| Managed Stripping | High |
| .NET API | Standard 2.1 |
| WebGL Compression | Brotli + decompression fallback |
| Exceptions | None (smallest build) |
| Debug Symbols | Off |
| Code Optimization | Disk Size + LTO |
| Texture Crunch | DXT1/DXT5 Crunched, quality 50 |
| Audio | Vorbis, quality 0.45, OptimizeSampleRate |

Also includes an **Audit** feature that flags heavy assets the tool can't auto-fix (large Spine JSON → recommends binary `.skel` format for ~50-70% savings).

### Tech Stack Size Impact

| Library | Purpose | Size Impact |
|---------|---------|-------------|
| Spine 2D | Skeletal animation | Atlas textures only (no code stripping issue) |
| DOTween | Animation tweening | Strippable via IL2CPP |
| Cartoon FX Remaster | VFX particles | Only used prefabs ship |
| Newtonsoft.Json | JSON parsing | Included but tree-shaken |

---

## 🔧 Trade-offs & Simplifications

Given the **3-5 day case study scope**, deliberate simplifications were made:

| Decision | Rationale | If More Time… |
|----------|-----------|---------------|
| **Single scene** | Faster load, no scene transitions | Add additive scene loading for splash screen |
| **Mouse-only input** (no touch API) | Unity's `Input.GetMouseButton` works for both touch & mouse on WebGL | Add `Input.touches` for native mobile feel |
| **No audio sync precision** | Simple `AudioSource.time` is good enough for playable ads | Implement DSP clock for frame-perfect sync |
| **4 fixed lanes** | JSON notes already use pid → lane mapping, extensible | Dynamic lane count per difficulty |
| **No hold notes** | JSON has `d` (duration) field — data is there, logic unimplemented | Implement hold-note detection & release scoring |
| **Linear miss window** | Simple `elapsed > timeout` check | Add graded timing (Perfect/Good/Miss zones) |
| **No combo multiplier** | Keeps score simple for ad metrics | Add combo chain with visual feedback |
| **Spine JSON (not .skel)** | Faster prototyping in-editor | Export as binary `.skel` for 50-70% size reduction |
| **Auto-play intro/outro** | Vignette shader + paw animation sequence | Skip-able intro, branded outro with CTA |

---

## 💡 Improvement Ideas

### 1. **Hold-Note Support**
The JSON data already includes a `d` (duration) field. Implementing hold notes would make the gameplay significantly closer to the original Duet Cats and increase player retention metrics.

### 2. **Graded Timing Windows**
Replace the binary hit/miss with **Perfect / Good / OK / Miss** zones. This adds skill expression and makes the ad feel more rewarding — critical for playable ad conversion rates.

### 3. **Combo System & Score Multiplier**
A streak counter with visual feedback (screen shake at 10x, fire VFX at 25x) creates dopamine loops that drive longer play sessions in the ad.

### 4. **Native Touch API**
Adding `Input.touches` alongside mouse input improves mobile feel, reduces perceived latency, and increases ad engagement on mobile devices.

### 5. **Dynamic Difficulty**
Adjust note fall speed and spawn density based on player performance. Better players get harder charts → longer engagement → higher conversion.

### 6. **CTA Overlay at End Screen**
Replace the simple "pick song" loop with a branded end card: score summary + "Install Now" or "Get the Full Game" button. This is the **primary conversion point** for playable ads.

---

## 🛠️ Building

### Requirements
- **Unity 6000.3 LTS** (or compatible 6.x)
- WebGL module installed

### Build Steps

1. Open project in Unity
2. `Tools → Build Size Optimizer` → **Apply Optimizations**
3. `File → Build Settings` → WebGL → **Build**
4. Deploy the build folder to any static host (GitHub Pages, itch.io, S3)

### Development

Build size budget is tracked through the optimizer tool's **Audit Assets** feature. After any asset addition, run the audit to check for budget creep.

---

## 📐 Design Principles

> **"Tiny builds need intentional architecture."**

- **Every byte counts** — pooled objects, stripped engine code, crunched textures
- **Separate concerns** — GameController never touches UI; UIManager never touches gameplay
- **Interface over inheritance** — `ICandyEater` over abstract base class
- **Config over code** — ScriptableObjects for anything that might change per-campaign
- **Event-driven decoupling** — `GameEvents` keeps the codebase flat and composable

---

Built with 💛 for the Amanotes Playable Ads Developer case study.
