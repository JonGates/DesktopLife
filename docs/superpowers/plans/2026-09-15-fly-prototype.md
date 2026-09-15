# Fly Prototype Implementation Plan

> Execute in this session with the executing-plans workflow. The user requested direct implementation and environment setup.

**Goal:** Deliver MILESTONE 1: one desktop fly that reacts to global mouse movement.
**Architecture:** Five projects with one-way references. Engine and Creatures contain no Windows/WPF types. App composes Windows input and a single rendering loop; Rendering draws all creatures in one surface.
**Tech Stack:** C#, .NET 10, WPF, Win32, xUnit.
**Spec:** `docs/DesktopLife_Codex_Development_Spec.md`, milestone 1 (stages 0–4).

## Constraints and decisions

- Windows primary monitor only; physical pixels are the simulation coordinate system.
- Render physical coordinates through the current WPF device-to-DIP transform. Declare PerMonitorV2 awareness.
- Preserve user focus: layered, transparent, no-activate, tool-window styles applied before showing the overlay.
- A single CompositionTarget.Rendering subscription drives updates; cap simulation steps at 50 ms while measuring input idle time with actual elapsed time.
- Draw a cached vector fly placeholder; no per-creature controls or timers.
- Minimal tray pause/resume and exit ensure the prototype can always be controlled. Full settings, other species and installer are later milestones.
- Use the already-installed SDK 10.0.204. No system installation is necessary.

## Tasks

### 0. Solution and development environment
- [x] Create `DesktopLife.sln`, `Directory.Build.props`, `global.json`, `.gitignore`, README.
- [x] Create five projects in `src/DesktopLife.{App,Engine,Creatures,Rendering,Windows}` and two xUnit projects in `tests/`.
- [x] Restore, build, test and launch the empty WPF shell with a bounded smoke lifetime.

### 1. Overlay
- [x] Implement `Windows/NativeMethods.cs`, `WindowStyles.cs`, `MonitorService.cs`, `OverlayWindowHelper.cs`.
- [x] Implement `App/Overlay/OverlayWindow.xaml` and a test-dot render surface, plus tray exit and safe exception logging.
- [x] Add a repeatable native smoke probe checking actual HWND styles, bounds, no activation, and hit testing over painted pixels.
- [x] Run `dotnet build DesktopLife.sln`, `dotnet test DesktopLife.sln`, and the probe before proceeding.

### 2. Input and HUD
- [x] Write failing `MouseTrackerTests`: first sample has no motion, 3-4-5 displacement gives 100 px/s over 50 ms, idle accumulates and resets, zero/invalid dt remains finite.
- [x] Implement `Engine/Input/MouseState.cs`, `MouseTracker.cs` using `Update(Vector2 position, float elapsedSeconds)`; Win32 cursor polling stays in Windows.
- [x] Add a throttled Debug HUD with physical cursor position, speed and idle seconds. Verify build/test and launch.

### 3. Simulation and rendering
- [x] Write failing tests for normalized vectors, deterministic random, nonzero-origin bounds, 50 ms step limit, and one update per creature.
- [x] Implement `Engine/Math/*`, `World/*`, `Time/*`, `Creatures/*`. `SimulationWorld.Update(float elapsedSeconds, Vector2 cursor)` owns tracking and manager update.
- [x] Implement `Rendering/IRenderer.cs`, `WpfCreatureRenderer.cs`; connect a moving DebugCreature to the single loop.
- [x] Verify build/test and native launch probe.

### 4. Fly behavior
- [x] Write failing tests for Offscreen→Approach, Approach→Orbit, idle→Depart→Offscreen, reactivation without teleport, Panic moving away then recovering, deterministic long-run stability.
- [x] Implement `Creatures/Fly/FlyState.cs`, `FlyOptions.cs`, `FlyBrain.cs`, `FlyCreature.cs` with injected random and frame-rate independent steering.
- [x] Replace DebugCreature with one fly; use cached vector wings/body and orientation.
- [x] Run Debug/Release build/test, native probe, repeatable scripted simulation and sustained runtime sampling.
- [x] Publish a runnable win-x64 build, add launch script, record evidence and remaining manual checks in `docs/MILESTONE1_VERIFICATION.md`.

## Acceptance

Build and unit tests must pass. Native integration must verify click-through and focus before behavior development. Report any desktop interaction or DPI scale not actually exercised as unverified. Do not claim full V0.1 (other species and productization) is delivered.


## Final status
Implementation and automated checks completed. See docs/MILESTONE1_VERIFICATION.md for evidence and outstanding manual desktop acceptance. Inline implementation was used; one independent code review was delegated under the requesting-code-review skill. This directory had no Git repository, so no commit or merge was performed.
