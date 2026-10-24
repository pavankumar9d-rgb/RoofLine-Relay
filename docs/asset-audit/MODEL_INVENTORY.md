# Roofline Relay — 3D Model Asset Audit & Master Inventory

## Executive Summary
A comprehensive audit and structural reorganization of all downloaded and converted 3D assets (`.glb`) for **Roofline Relay** has been conducted. 
- **Total Models Cataloged & Staged:** 44 models (10 initial + 13 batch 2 + 13 batch 3 + 8 batch 4)
- **All Models Successfully Inspected:** 100% verified via headless Blender 5.2.2 LTS.
- **Preview Renders Generated:** 44 high-resolution 512x512 neutral studio previews saved to `docs/asset-audit/previews/`.
- **Target Clean Location:** `docs/asset-audit/organized-models/`

---

## Master Model Inventory Table (44 Assets)

| ID | Staged Clean Asset | Category | Triangles | Dimensions (W x D x H) | Status | Required Production Action |
| :--- | :--- | :--- | :---: | :---: | :---: | :--- |
| `AUDIT_MDL_01` | `characters/player/female/courier_female_urban_scout.glb` | characters/player/female | 65,568 | 1.085m x 0.353m x 1.900m | Staged | Quad retopo to 15k tris, Humanoid rig |
| `AUDIT_MDL_02` | `characters/player/male/courier_male_rigged_candidate_a.glb` | characters/player/male | 89,070 | 1.902m x 2.000m x 2.700m | Staged | Rescale to 1.85m, retarget/re-rig to Humanoid, decimate |
| `AUDIT_MDL_03` | `characters/player/male/courier_male_recon_pilot_candidate_b.glb` | characters/player/male | 64,062 | 1.000m x 0.358m x 1.899m | Staged | Quad retopo to 15k tris, Humanoid rig |
| `AUDIT_MDL_04` | `characters/player/male/courier_male_urban_sentinel_candidate_c.glb` | characters/player/male | 104,700 | 0.679m x 1.131m x 1.897m | Staged | Retopo to 15k tris, optional Rival Runner NPC |
| `AUDIT_MDL_05` | `characters/enemies/security_pursuit_drone_sentinel.glb` | characters/enemies | 363,278 | 1.633m x 1.898m x 0.812m | Staged | Decimate to 10k tris, separate rotor disks |
| `AUDIT_MDL_06` | `environment/parkour-obstacles/modular_concrete_parapet_block.glb` | environment/parkour-obstacles | 501,880 | 1.000m x 0.334m x 0.631m | Staged | Extreme poly count: decimate 500k to 300 tris, bake normal map |
| `AUDIT_MDL_07` | `props/industrial/rooftop_hvac_chiller_unit.glb` | props/industrial | 469,522 | 1.899m x 1.874m x 1.265m | Staged | Decimate 469k to 4k tris, box collider |
| `AUDIT_MDL_08` | `props/gameplay/courier_package_delivery_hardcase.glb` | props/gameplay | 1,915,192 | 0.740m x 0.980m x 0.536m | Staged | Extreme poly count: decimate 1.9M to 2.5k tris |
| `AUDIT_MDL_09` | `props/gameplay/signature_relay_beacon_spire.glb` | props/gameplay | 209,988 | 0.942m x 0.942m x 1.900m | Staged | Decimate to 10k tris, scale up 1.5x-2.0x for landmark |
| `AUDIT_MDL_10` | `props/gameplay/relay_activation_control_console.glb` | props/gameplay | 158,334 | 1.000m x 1.023m x 1.899m | Staged | Decimate to 5k tris, separate scanner pad |
| `AUDIT_MDL_11` | `characters/npc/relay_operator_tactical_dispatcher.glb` | characters/npc | 154,774 | 0.802m x 0.595m x 1.899m | Staged | Quad retopo to 15k tris, Humanoid rig |
| `AUDIT_MDL_12` | `characters/npc/rival_runner_urban_trail_sentinel.glb` | characters/npc | 108,706 | 0.660m x 0.454m x 1.896m | Staged | Quad retopo to 15k tris, Humanoid rig |
| `AUDIT_MDL_13` | `props/gameplay/checkpoint_marker_quantum_gate.glb` | props/gameplay | 104,908 | 0.951m x 0.831m x 1.901m | Staged | Decimate to 3k tris, trigger collider & emissive shader |
| `AUDIT_MDL_14` | `props/gameplay/collectible_data_shard_amber_core.glb` | props/gameplay | 134,196 | 1.899m x 1.133m x 0.337m | Staged | Decimate to 1.5k tris, rescale to 0.4m, spin animation |
| `AUDIT_MDL_15` | `environment/architecture/modular_facade_concrete_window_section.glb` | environment/architecture | 151,954 | 1.481m x 0.788m x 1.900m | Staged | Decimate to 2k tris, snap pivot alignment to 4m grid |
| `AUDIT_MDL_16` | `environment/rooftop-modules/rooftop_stairwell_housing_access_01.glb` | environment/rooftop-modules | 175,452 | 1.900m x 1.107m x 1.558m | Staged | Decimate to 3.5k tris, convex mesh collider, vaulting ledge |
| `AUDIT_MDL_17` | `environment/rooftop-modules/rooftop_service_building_maintenance_cabin.glb` | environment/rooftop-modules | 222,682 | 1.681m x 1.900m x 1.613m | Staged | Decimate to 4k tris, compound box colliders, ladder mount |
| `AUDIT_MDL_18` | `environment/parkour-obstacles/industrial_catwalk_bridge_module.glb` | environment/parkour-obstacles | 342,236 | 1.000m x 0.343m x 0.303m | Staged | Decimate to 2.5k tris, bake alpha grating normal, snap flanges |
| `AUDIT_MDL_19` | `environment/parkour-obstacles/fixed_metal_caged_service_ladder.glb` | environment/parkour-obstacles | 48,298 | 0.327m x 0.364m x 1.900m | Staged | Decimate to 1.8k tris, setup climb trigger volume |
| `AUDIT_MDL_20` | `environment/parkour-obstacles/industrial_pipe_vault_obstacle.glb` | environment/parkour-obstacles | 138,130 | 1.901m x 0.955m x 0.614m | Staged | Decimate to 2k tris, parkour vault/slide collider setup |
| `AUDIT_MDL_21` | `props/industrial/industrial_rooftop_exhaust_fan.glb` | props/industrial | 310,758 | 1.899m x 1.815m x 1.649m | Staged | Decimate to 4k tris, separate spinning fan submesh |
| `AUDIT_MDL_22` | `props/industrial/parabolic_satellite_dish_silent_signal.glb` | props/industrial | 475,528 | 1.419m x 1.504m x 1.900m | Staged | Decimate to 5k tris, separate gimbal rotation node |
| `AUDIT_MDL_23` | `props/industrial/industrial_rooftop_water_storage_tank.glb` | props/industrial | 501,324 | 0.630m x 0.526m x 1.000m | Staged | Decimate to 4.5k tris, rescale gantry to 3.5m, compound colliders |
| `AUDIT_MDL_24` | `environment/architecture/modular_mech_facade_panel.glb` | environment/architecture | 396,346 | 1.427m x 0.443m x 1.900m | Staged | Decimate to 2.5k tris, modular 4m snap alignment |
| `AUDIT_MDL_25` | `environment/rooftop-modules/modular_roof_edge_structure.glb` | environment/rooftop-modules | 103,092 | 1.899m x 1.279m x 0.381m | Staged | Decimate to 2k tris, snap collision border |
| `AUDIT_MDL_26` | `environment/architecture/architectural_corner_pillar_module.glb` | environment/architecture | 172,392 | 0.958m x 0.959m x 1.900m | Staged | Decimate to 2.2k tris, corner pivot alignment |
| `AUDIT_MDL_27` | `environment/architecture/skyline_landmark_communications_spire.glb` | environment/architecture | 1,417,516 | 0.736m x 0.711m x 1.992m | Staged | Decimate to 8k tris (LOD0), remove micro-trusses, horizon silhouette LOD |
| `AUDIT_MDL_28` | `environment/parkour-obstacles/rooftop_platform_slab_module.glb` | environment/parkour-obstacles | 101,816 | 1.899m x 1.899m x 0.312m | Staged | Decimate to 2k tris, flat box collider, run surface friction |
| `AUDIT_MDL_29` | `environment/parkour-obstacles/modular_concrete_parapet_corner.glb` | environment/parkour-obstacles | 501,032 | 0.997m x 1.000m x 0.569m | Staged | Decimate to 2k tris, 90-degree corner collider, parkour vault ledge |
| `AUDIT_MDL_30` | `environment/parkour-obstacles/broad_raised_ledge_coping.glb` | environment/parkour-obstacles | 306,512 | 1.000m x 0.269m x 0.376m | Staged | Decimate to 1.8k tris, box collider, parkour shimmy edge tag |
| `AUDIT_MDL_31` | `environment/parkour-obstacles/short_maintenance_truss_bridge.glb` | environment/parkour-obstacles | 317,654 | 1.000m x 0.359m x 0.251m | Staged | Decimate to 3.5k tris, walkable convex mesh collider, railing barrier |
| `AUDIT_MDL_32` | `environment/parkour-obstacles/fire_escape_stairs_section.glb` | environment/parkour-obstacles | 375,236 | 0.917m x 0.411m x 1.000m | Staged | Decimate to 3.5k tris, inclined ramp collider for smooth player run |
| `AUDIT_MDL_33` | `environment/parkour-obstacles/large_industrial_vent_obstacle.glb` | environment/parkour-obstacles | 1,499,760 | 1.993m x 1.887m x 1.784m | Staged | Decimate to 3k tris, box collider, vault/mantle tag |
| `AUDIT_MDL_34` | `environment/parkour-obstacles/gap_crossing_suspension_bridge.glb` | environment/parkour-obstacles | 253,904 | 1.000m x 0.349m x 0.233m | Staged | Decimate to 3.5k tris, suspension cable alpha/LOD, deck run collider |
| `AUDIT_MDL_35` | `props/industrial/ventilation_duct_run_assembly.glb` | props/industrial | 1,499,463 | 1.993m x 0.524m x 0.856m | Staged | Decimate to 3k tris, cylindrical collider, modular pipe snap |
| `AUDIT_MDL_36` | `props/industrial/rooftop_telemetry_antenna_mast.glb` | props/industrial | 122,624 | 0.474m x 0.477m x 1.900m | Staged | Decimate to 2.5k tris, thin-wire LOD fade, non-blocking upper bounds |
| `AUDIT_MDL_37` | `props/industrial/rooftop_solar_panel_array.glb` | props/industrial | 363,408 | 1.900m x 1.838m x 1.901m | Staged | Decimate to 4k tris, flat box collider, angle mount snapping |
| `AUDIT_MDL_38` | `props/industrial/industrial_rooftop_skylight.glb` | props/industrial | 162,976 | 1.899m x 1.309m x 0.570m | Staged | Decimate to 2.5k tris, glass alpha shader, curb collider |
| `AUDIT_MDL_39` | `props/industrial/rooftop_metal_service_door.glb` | props/industrial | 377,816 | 0.570m x 0.225m x 1.000m | Staged | Decimate to 3k tris, separate door slab hinge pivot from frame |
| `AUDIT_MDL_40` | `props/industrial/rooftop_access_hatch_curb.glb` | props/industrial | 502,364 | 0.993m x 1.000m x 0.764m | Staged | Decimate to 2.5k tris, separate lid hinge node for open/close state |
| `AUDIT_MDL_41` | `props/industrial/cable_tray_and_spool_reel.glb` | props/industrial | 501,124 | 0.475m x 1.000m x 0.543m | Staged | Decimate to 4k tris, cylindrical collider, cable conduit snap |
| `AUDIT_MDL_42` | `props/industrial/modular_industrial_utility_crate.glb` | props/industrial | 249,244 | 1.900m x 1.893m x 1.716m | Staged | Decimate to 1.8k tris, box collider, stackable snapping bounds |
| `AUDIT_MDL_43` | `props/industrial/modular_safety_railing_barrier.glb` | props/industrial | 60,280 | 1.899m x 0.277m x 0.923m | Staged | Decimate to 2k tris, perimeter barrier collider, parkour vault edge |
| `AUDIT_MDL_44` | `props/industrial/rooftop_warning_beacon_light.glb` | props/industrial | 1,499,084 | 1.189m x 1.405m x 1.991m | Staged | Decimate to 3k tris, amber emissive point-light source node |

---

## Directory Organization Structure
```
docs/asset-audit/organized-models/
├── characters/
│   ├── enemies/
│   │   └── security_pursuit_drone_sentinel.glb
│   ├── npc/
│   │   ├── relay_operator_tactical_dispatcher.glb
│   │   └── rival_runner_urban_trail_sentinel.glb
│   └── player/
│       ├── female/
│       │   └── courier_female_urban_scout.glb
│       └── male/
│           ├── courier_male_recon_pilot_candidate_b.glb
│           ├── courier_male_rigged_candidate_a.glb
│           └── courier_male_urban_sentinel_candidate_c.glb
├── environment/
│   ├── architecture/
│   │   ├── architectural_corner_pillar_module.glb
│   │   ├── modular_facade_concrete_window_section.glb
│   │   ├── modular_mech_facade_panel.glb
│   │   └── skyline_landmark_communications_spire.glb
│   ├── parkour-obstacles/
│   │   ├── broad_raised_ledge_coping.glb
│   │   ├── fire_escape_stairs_section.glb
│   │   ├── fixed_metal_caged_service_ladder.glb
│   │   ├── gap_crossing_suspension_bridge.glb
│   │   ├── industrial_catwalk_bridge_module.glb
│   │   ├── industrial_pipe_vault_obstacle.glb
│   │   ├── large_industrial_vent_obstacle.glb
│   │   ├── modular_concrete_parapet_block.glb
│   │   ├── modular_concrete_parapet_corner.glb
│   │   ├── rooftop_platform_slab_module.glb
│   │   └── short_maintenance_truss_bridge.glb
│   └── rooftop-modules/
│       ├── modular_roof_edge_structure.glb
│       ├── rooftop_service_building_maintenance_cabin.glb
│       └── rooftop_stairwell_housing_access_01.glb
└── props/
    ├── gameplay/
    │   ├── checkpoint_marker_quantum_gate.glb
    │   ├── collectible_data_shard_amber_core.glb
    │   ├── courier_package_delivery_hardcase.glb
    │   ├── relay_activation_control_console.glb
    │   └── signature_relay_beacon_spire.glb
    └── industrial/
        ├── cable_tray_and_spool_reel.glb
        ├── industrial_rooftop_exhaust_fan.glb
        ├── industrial_rooftop_skylight.glb
        ├── industrial_rooftop_water_storage_tank.glb
        ├── modular_industrial_utility_crate.glb
        ├── modular_safety_railing_barrier.glb
        ├── parabolic_satellite_dish_silent_signal.glb
        ├── rooftop_access_hatch_curb.glb
        ├── rooftop_hvac_chiller_unit.glb
        ├── rooftop_metal_service_door.glb
        ├── rooftop_solar_panel_array.glb
        ├── rooftop_telemetry_antenna_mast.glb
        ├── rooftop_warning_beacon_light.glb
        └── ventilation_duct_run_assembly.glb
```
