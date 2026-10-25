# Roofline Relay — Phase 6 Gap Audit & Project Status

**Date:** 2026-10-25  
**Engine & Target:** Unity 6000.6.7f1 (URP) | Windows Desktop & Android Landscape  
**Hardware Profile:** Intel UHD Graphics / 8 GB RAM (~₹0 Budget Constraint)  
**Branch / Base Commit:** `main` @ `1e26e2b`

---

## 1. Executive Summary & Verified Baseline

Following Phase 5 integration, the core animation pipeline and audio systems have been established:
- **10 Mixamo Animation Clips** (`Idle`, `Walk`, `Jog`, `Run`, `Jump`, `Falling`, `Landing`, `Vault`, `Slide`, `Roll`) are cleanly extracted, generic-rigged, and driven by `CourierAnimatorController.controller`.
- **Procedural Audio Suite** (11 WAV files: footsteps, jump, landing, vault, slide, roll, quantum checkpoint, relay beacon, drone hum, rooftop wind) synthesized and wired via `RooflineAudioManager.cs`.
- **Pursuit Drone Sentinel LOD** (`security_pursuit_drone_sentinel_lod.glb`, 18,163 triangles) hovering with 3D audio and detection cone.
- **Showcase Video Generated:** `roofline_relay_gameplay_showcase.mp4` verified on disk.

However, a critical review of the actual playable game reveals three major gaps preventing the prototype from feeling like a convincing, cohesive vertical slice:
1. **Greybox Visuals:** The gameplay route still relies on primitive Unity Cubes (grey boxes, orange rectangular hurdle, red duct box, primitive cylinder beacon). The audited library of 44 real 3D models has not yet been assembled into the scene.
2. **Camera Framing & Clipping:** While the fixed showcase camera angles were widened, dynamic player actions during steep vertical jumps and hurdle vaults still suffer from tight framing, and the third-person gameplay camera needs robust obstacle SphereCasting and smoothed vertical damping.
3. **Player Physics & Obstacle Collision:** The player's CharacterController height transition during slides needs collision guarantees, vault hurdle clearance must accurately match the real pipe hurdle model, and gap jumping needs a visual bridge/platform reference.

---

## 2. Comprehensive Gap Analysis Matrix

| Priority | Subsystem | Current State | Issue / Gap | Target Production Fix |
| :---: | :--- | :--- | :--- | :--- |
| **P1** | **Camera Framing & Collision** | `ThirdPersonCamera.cs` uses simple linecast and fixed pitch. Narrow framing during peak jump. | Camera can pull in too close during obstacles; jump apex still risks top cropping on vertical leaps. | Increase base distance to 5.2m, FOV to 65°, implement smoothed vertical focus point, and use 0.3m SphereCast for occlusion. |
| **P2** | **Environment & Level Art** | Scene consists of primitive colored cubes (`Rooftop_Start_Platform`, `Vault_Barrier_Obstacle`, etc.). | Visually crude; does not represent the cyber-industrial rooftop fantasy. High-poly models in library (300k–500k tris) will stutter on Intel UHD. | Decimate audited models into lightweight LODs (2k–5k tris each): stairwell cabin, pipe vault obstacle, overhead ventilation duct, catwalk bridge, quantum gate, and beacon spire. |
| **P3** | **Parkour Mechanics & Collision** | Slide and Roll implemented via coroutines; primitive barrier collider used for vault. | Obstacle detection raycasts need exact alignment with real 3D mesh colliders; ensure slide capsule safely uncrouches only when overhead clearance is verified. | Configure compound box colliders on real pipe hurdle, add trigger volumes, tune vault arc speed and apex height (1.1m). |
| **P4** | **Input & Controls** | Keyboard and on-screen GUI buttons exist in `OnGUI`. | Android touch buttons are drawn via legacy `GUI.RepeatButton`, which can conflict with multi-touch gestures. | Refine touch input handling for clean simultaneous movement + sprint/jump/slide; support camera-relative vector math. |
| **P5** | **Audio & Visual Feedback** | `RooflineAudioManager.cs` plays clips; basic color change on checkpoint. | Lacks dynamic particle pulse, light intensity flare, and spatialized ambient positioning. | Add lightweight URP light pulse and audio triggers upon gate passage and beacon acquisition. |
| **P6** | **Pursuit Drone Sentinel** | Script exists, patrol between X coordinates, sphere cast detection. | Drone visual mesh needs proper emission glow and patrol pacing across the chasm gap. | Tune patrol speed (2.0 m/s), add pulsing warning light cone, verify detection respawn reliability. |
| **P7** | **Gameplay Verification & Capture** | Only scripted showcase video exists; gameplay recording needed. | Need verifiable proof of interactive player-controlled gameplay running inside Unity. | Record complete run: Start rooftop $\to$ Pipe vault $\to$ Duct slide $\to$ Gap jump past drone $\to$ Landing roll $\to$ Checkpoint gate $\to$ Beacon collection. |

---

## 3. Prioritized Implementation Roadmap

1. **Camera Overhaul ([`ThirdPersonCamera.cs`](file:///C:/Users/saipa/OneDrive/Desktop/Roofline%20Relay/unity-game/Assets/Scripts/Camera/ThirdPersonCamera.cs)):**
   - Base distance: 5.2m, min distance: 2.5m, max distance: 6.5m, FOV: 65°.
   - Vertical follow damping with offset $(0, 1.35\text{m}, 0)$ to keep character body comfortably centered with generous headroom during leaps.
   - SphereCast occlusion with radius $0.3\text{m}$ to prevent near-clipping against rooftop walls.
2. **Asset Optimization & LOD Pipeline:**
   - Optimize and import:
     - `rooftop_stairwell_housing_lod.glb` (~3.5k tris)
     - `industrial_pipe_vault_obstacle_lod.glb` (~2.8k tris)
     - `ventilation_duct_run_assembly_lod.glb` (~3.2k tris)
     - `industrial_catwalk_bridge_lod.glb` (~3.0k tris)
     - `checkpoint_marker_quantum_gate_lod.glb` (~3.8k tris)
     - `signature_relay_beacon_spire_lod.glb` (~4.5k tris)
     - `rooftop_hvac_chiller_unit_lod.glb` (~3.6k tris)
     - `modular_concrete_parapet_block_lod.glb` (~1.8k tris)
3. **Environment Construction ([`CharacterPipelineSetup.cs`](file:///C:/Users/saipa/OneDrive/Desktop/Roofline%20Relay/unity-game/Assets/Editor/CharacterPipelineSetup.cs)):**
   - Replace primitive cubes with the real LOD models.
   - Build a structured, high-readability 3-platform rooftop route:
     - **Platform 1 (Launchpad):** Industrial rooftop with stairwell access housing, concrete parapet borders, HVAC chiller prop, and real pipe vault obstacle ($Z = 7\text{m}$).
     - **Mid-Section (Low Clearance):** Industrial ventilation duct run suspended at $Z = 11\text{m}$ requiring a running slide.
     - **The Chasm (Gap Crossing):** $4.0\text{m}$ gap flanked by an industrial catwalk bridge truss, patrolled overhead by the Pursuit Drone Sentinel ($Z = 16\text{m}$).
     - **Platform 2 (Checkpoint Courtyard):** Quantum Checkpoint Gate model with glowing arch pillars at $Z = 22\text{m}$.
     - **Platform 3 (Signal Spire):** Telemetry platform hosting the spinning Signature Relay Beacon Spire model at $Z = 38\text{m}$.
4. **Gameplay & Player Controller Tuning ([`PlayerParkourController.cs`](file:///C:/Users/saipa/OneDrive/Desktop/Roofline%20Relay/unity-game/Assets/Scripts/Character/PlayerParkourController.cs)):**
   - Refine raycast detection masks for the new pipe hurdle.
   - Ensure reliable slide un-crouch and smooth momentum transition.
5. **Gameplay Verification & Demonstration Video:**
   - Run Unity batch build and verify 0 errors.
   - Produce a 30–60s gameplay recording demonstrating real player controls on the finished environment.
