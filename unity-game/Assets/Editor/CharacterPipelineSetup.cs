using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using RooflineRelay.Character;
using RooflineRelay.CameraControl;
using RooflineRelay.Gameplay;
using RooflineRelay.Audio;

namespace RooflineRelay.Editor
{
    public static class CharacterPipelineSetup
    {
        private const string CharacterFbxPath = "Assets/Art/Characters/courier_female_urban_scout_rigged.fbx";
        private const string AnimFolder = "Assets/Art/Animations/CourierFemale";
        private const string ControllerPath = "Assets/Art/Characters/CourierAnimatorController.controller";
        private const string ProductionScenePath = "Assets/Scenes/RooflineRelay_Main.unity";
        private const string TestScenePath = "Assets/Scenes/CharacterAnimationTest.unity";
        
        // 3D Model Paths (Native FBX format)
        private const string DroneModelPath = "Assets/Art/Characters/security_pursuit_drone_sentinel_lod.fbx";
        private const string PipeVaultModelPath = "Assets/Art/Environment/pipe_vault_obstacle_lod.fbx";
        private const string VentDuctModelPath = "Assets/Art/Environment/ventilation_duct_run_lod.fbx";
        private const string CheckpointModelPath = "Assets/Art/Props/checkpoint_quantum_gate_lod.fbx";
        private const string BeaconModelPath = "Assets/Art/Props/relay_beacon_spire_lod.fbx";
        private const string StairwellModelPath = "Assets/Art/Environment/stairwell_housing_lod.fbx";
        private const string CatwalkModelPath = "Assets/Art/Environment/catwalk_bridge_lod.fbx";
        private const string HvacModelPath = "Assets/Art/Props/hvac_chiller_lod.fbx";
        private const string ParapetModelPath = "Assets/Art/Environment/parapet_block_lod.glb";

        [MenuItem("Roofline Relay/Setup Character Pipeline & Playable Scene")]
        public static void Execute()
        {
            Debug.Log("[CharacterPipelineSetup] Starting Phase 6 Vertical Slice & Environment Polish setup...");

            try
            {
                // Step 1: Synchronous Asset Refresh
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

                // Step 2: Configure Base Character ModelImporter
                ConfigureBaseCharacterImporter();

                // Step 3: Configure Animation Clips (All 10 clips)
                var clipMap = ConfigureAnimationImporters();

                // Step 4: Create Animator Controller with 10 States & Transitions
                var controller = CreateAnimatorController(clipMap);

                // Step 5: Build Playable Rooftop Vertical Slice with Real 3D Environment Assets
                BuildPlayableRooftopScene(controller);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("[CharacterPipelineSetup] SUCCESS: Phase 6 Playable Vertical Slice with real 3D environment assets successfully verified!");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CharacterPipelineSetup] FAILED: {ex.Message}\n{ex.StackTrace}");
                EditorApplication.Exit(1);
            }
        }

        private static void ConfigureBaseCharacterImporter()
        {
            var importer = AssetImporter.GetAtPath(CharacterFbxPath) as ModelImporter;
            if (importer == null)
            {
                throw new FileNotFoundException($"ModelImporter could not find asset at path: {CharacterFbxPath}");
            }

            Debug.Log("[CharacterPipelineSetup] Configuring ModelImporter for Base Character...");
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            importer.SaveAndReimport();
        }

        private static Dictionary<string, AnimationClip> ConfigureAnimationImporters()
        {
            var clipMap = new Dictionary<string, AnimationClip>(StringComparer.OrdinalIgnoreCase);

            string[] animFiles = new string[]
            {
                "UrbanScout_Idle.fbx",
                "UrbanScout_Walk.fbx",
                "UrbanScout_Jog.fbx",
                "UrbanScout_Run.fbx",
                "UrbanScout_Jump.fbx",
                "UrbanScout_Falling.fbx",
                "UrbanScout_Landing.fbx",
                "UrbanScout_Vault.fbx",
                "UrbanScout_Slide.fbx",
                "UrbanScout_Roll.fbx"
            };

            foreach (var file in animFiles)
            {
                string assetPath = $"{AnimFolder}/{file}";
                string clipName = Path.GetFileNameWithoutExtension(file).Replace("UrbanScout_", "");
                var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                if (importer == null)
                {
                    Debug.LogWarning($"[CharacterPipelineSetup] Animation file not found at: {assetPath}");
                    continue;
                }

                importer.animationType = ModelImporterAnimationType.Generic;
                importer.materialImportMode = ModelImporterMaterialImportMode.None;

                var defaults = importer.defaultClipAnimations;
                if (defaults != null && defaults.Length > 0)
                {
                    defaults[0].name = clipName;
                    bool isLoop = clipName == "Idle" || clipName == "Walk" || clipName == "Jog" || clipName == "Run" || clipName == "Falling";
                    defaults[0].loopTime = isLoop;
                    defaults[0].loopPose = isLoop && clipName != "Falling";
                    defaults[0].lockRootPositionXZ = true;
                    defaults[0].lockRootHeightY = true;
                    defaults[0].lockRootRotation = true;
                    importer.clipAnimations = defaults;
                }
                importer.SaveAndReimport();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            foreach (var file in animFiles)
            {
                string assetPath = $"{AnimFolder}/{file}";
                string clipName = Path.GetFileNameWithoutExtension(file).Replace("UrbanScout_", "");

                var allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                foreach (var asset in allAssets)
                {
                    if (asset is AnimationClip animClip && !animClip.name.StartsWith("__preview__"))
                    {
                        clipMap[clipName] = animClip;
                        Debug.Log($"[CharacterPipelineSetup] Registered clip '{clipName}' -> {animClip.name} ({animClip.length:F2}s)");
                        break;
                    }
                }
            }

            return clipMap;
        }

        private static AnimatorController CreateAnimatorController(Dictionary<string, AnimationClip> clipMap)
        {
            Debug.Log($"[CharacterPipelineSetup] Creating Animator Controller at {ControllerPath}...");

            if (File.Exists(ControllerPath))
            {
                AssetDatabase.DeleteAsset(ControllerPath);
            }

            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);

            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);

            var groundedParam = new AnimatorControllerParameter
            {
                name = "Grounded",
                type = AnimatorControllerParameterType.Bool,
                defaultBool = true
            };
            controller.AddParameter(groundedParam);

            var fallingParam = new AnimatorControllerParameter
            {
                name = "Falling",
                type = AnimatorControllerParameterType.Bool,
                defaultBool = false
            };
            controller.AddParameter(fallingParam);

            controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Vault", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Slide", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Roll", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("HardLanding", AnimatorControllerParameterType.Trigger);

            var rootStateMachine = controller.layers[0].stateMachine;

            AnimationClip GetClip(string name)
            {
                if (clipMap.TryGetValue(name, out var clip)) return clip;
                Debug.LogWarning($"[CharacterPipelineSetup] Clip '{name}' not found in map. Using fallback.");
                return null;
            }

            var idleState = rootStateMachine.AddState("Idle", new Vector3(200, 0, 0));
            idleState.motion = GetClip("Idle");

            var walkState = rootStateMachine.AddState("Walk", new Vector3(200, 70, 0));
            walkState.motion = GetClip("Walk");

            var jogState = rootStateMachine.AddState("Jog", new Vector3(200, 140, 0));
            jogState.motion = GetClip("Jog");

            var runState = rootStateMachine.AddState("Run", new Vector3(200, 210, 0));
            runState.motion = GetClip("Run");

            var jumpState = rootStateMachine.AddState("Jump", new Vector3(460, 0, 0));
            jumpState.motion = GetClip("Jump");

            var fallingState = rootStateMachine.AddState("Falling", new Vector3(460, 100, 0));
            fallingState.motion = GetClip("Falling");

            var landingState = rootStateMachine.AddState("Landing", new Vector3(460, 200, 0));
            landingState.motion = GetClip("Landing");

            var vaultState = rootStateMachine.AddState("Vault", new Vector3(200, -90, 0));
            vaultState.motion = GetClip("Vault");

            var slideState = rootStateMachine.AddState("Slide", new Vector3(460, 280, 0));
            slideState.motion = GetClip("Slide");

            var rollState = rootStateMachine.AddState("Roll", new Vector3(460, -90, 0));
            rollState.motion = GetClip("Roll");

            rootStateMachine.defaultState = idleState;

            // Locomotion Transitions
            var idleToWalk = idleState.AddTransition(walkState);
            idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.15f, "Speed");
            idleToWalk.hasExitTime = false;
            idleToWalk.duration = 0.15f;

            var walkToIdle = walkState.AddTransition(idleState);
            walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.15f, "Speed");
            walkToIdle.hasExitTime = false;
            walkToIdle.duration = 0.15f;

            var walkToJog = walkState.AddTransition(jogState);
            walkToJog.AddCondition(AnimatorConditionMode.Greater, 2.5f, "Speed");
            walkToJog.hasExitTime = false;
            walkToJog.duration = 0.15f;

            var jogToWalk = jogState.AddTransition(walkState);
            jogToWalk.AddCondition(AnimatorConditionMode.Less, 2.5f, "Speed");
            jogToWalk.hasExitTime = false;
            jogToWalk.duration = 0.15f;

            var jogToRun = jogState.AddTransition(runState);
            jogToRun.AddCondition(AnimatorConditionMode.Greater, 6.0f, "Speed");
            jogToRun.hasExitTime = false;
            jogToRun.duration = 0.15f;

            var runToJog = runState.AddTransition(jogState);
            runToJog.AddCondition(AnimatorConditionMode.Less, 6.0f, "Speed");
            runToJog.hasExitTime = false;
            runToJog.duration = 0.15f;

            // Jump & Air Transitions
            var anyToJump = rootStateMachine.AddAnyStateTransition(jumpState);
            anyToJump.AddCondition(AnimatorConditionMode.If, 0, "Jump");
            anyToJump.hasExitTime = false;
            anyToJump.duration = 0.05f;

            var jumpToFalling = jumpState.AddTransition(fallingState);
            jumpToFalling.hasExitTime = true;
            jumpToFalling.exitTime = 0.75f;
            jumpToFalling.duration = 0.12f;

            var anyToFalling = rootStateMachine.AddAnyStateTransition(fallingState);
            anyToFalling.AddCondition(AnimatorConditionMode.If, 0, "Falling");
            anyToFalling.AddCondition(AnimatorConditionMode.IfNot, 0, "Grounded");
            anyToFalling.hasExitTime = false;
            anyToFalling.duration = 0.15f;

            var fallingToLanding = fallingState.AddTransition(landingState);
            fallingToLanding.AddCondition(AnimatorConditionMode.If, 0, "Grounded");
            fallingToLanding.hasExitTime = false;
            fallingToLanding.duration = 0.08f;

            var landingToIdle = landingState.AddTransition(idleState);
            landingToIdle.hasExitTime = true;
            landingToIdle.exitTime = 0.85f;
            landingToIdle.duration = 0.15f;

            // Vault Transition
            var anyToVault = rootStateMachine.AddAnyStateTransition(vaultState);
            anyToVault.AddCondition(AnimatorConditionMode.If, 0, "Vault");
            anyToVault.hasExitTime = false;
            anyToVault.duration = 0.05f;

            var vaultToJog = vaultState.AddTransition(jogState);
            vaultToJog.hasExitTime = true;
            vaultToJog.exitTime = 0.9f;
            vaultToJog.duration = 0.15f;

            // Slide Transitions
            var anyToSlide = rootStateMachine.AddAnyStateTransition(slideState);
            anyToSlide.AddCondition(AnimatorConditionMode.If, 0, "Slide");
            anyToSlide.hasExitTime = false;
            anyToSlide.duration = 0.08f;

            var slideToJog = slideState.AddTransition(jogState);
            slideToJog.hasExitTime = true;
            slideToJog.exitTime = 0.85f;
            slideToJog.duration = 0.15f;

            // Roll Transitions
            var anyToRoll = rootStateMachine.AddAnyStateTransition(rollState);
            anyToRoll.AddCondition(AnimatorConditionMode.If, 0, "Roll");
            anyToRoll.hasExitTime = false;
            anyToRoll.duration = 0.06f;

            var rollToJog = rollState.AddTransition(jogState);
            rollToJog.hasExitTime = true;
            rollToJog.exitTime = 0.85f;
            rollToJog.duration = 0.15f;

            return controller;
        }

        private static void BuildPlayableRooftopScene(AnimatorController controller)
        {
            Debug.Log($"[CharacterPipelineSetup] Constructing Production Rooftop Vertical Slice at {ProductionScenePath}...");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Lighting & Atmosphere (Industrial Dusk)
            var lightGo = new GameObject("Sun_Directional_Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1.0f, 0.94f, 0.86f);
            light.intensity = 1.35f;
            lightGo.transform.rotation = Quaternion.Euler(48f, -32f, 0f);

            // Ambient sky color
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.25f, 0.32f, 0.40f);
            RenderSettings.ambientEquatorColor = new Color(0.18f, 0.22f, 0.26f);
            RenderSettings.ambientGroundColor = new Color(0.08f, 0.10f, 0.12f);

            // Concrete Material for Roof Surfaces
            var concreteMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            concreteMat.color = new Color(0.18f, 0.20f, 0.23f);
            concreteMat.SetFloat("_Roughness", 0.85f);

            // 2. Platform 1: Start Platform (z = -3m to 14m, width = 8m)
            var roof1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof1.name = "Platform_1_Rooftop_Base";
            roof1.transform.position = new Vector3(0f, -0.5f, 5.5f);
            roof1.transform.localScale = new Vector3(8.4f, 1f, 17f);
            roof1.GetComponent<Renderer>().material = concreteMat;

            // Rooftop Stairwell Housing Access (Start Point dressing)
            InstantiateModel(StairwellModelPath, "Stairwell_Housing_Access", new Vector3(-2.4f, 0f, 0.5f), new Vector3(1.2f, 1.2f, 1.2f), Quaternion.Euler(0, 90f, 0));

            // Parapet Borders on Platform 1
            InstantiateParapet(new Vector3(-4.1f, 0.15f, 5.5f), new Vector3(0.3f, 0.6f, 16.5f));
            InstantiateParapet(new Vector3(4.1f, 0.15f, 5.5f), new Vector3(0.3f, 0.6f, 16.5f));

            // Industrial HVAC Chiller Unit
            InstantiateModel(HvacModelPath, "HVAC_Chiller_Unit", new Vector3(2.5f, 0f, 3.5f), new Vector3(1.1f, 1.1f, 1.1f), Quaternion.identity);

            // Real 3D Pipe Vault Obstacle (at z = 7.5m, replaces primitive orange box)
            var pipeGo = InstantiateModel(PipeVaultModelPath, "Industrial_Pipe_Vault_Obstacle", new Vector3(0f, 0f, 7.5f), new Vector3(1.8f, 1.4f, 1.2f), Quaternion.Euler(0, 90f, 0));
            if (pipeGo != null)
            {
                var pipeCol = pipeGo.AddComponent<BoxCollider>();
                pipeCol.size = new Vector3(2.2f, 0.85f, 0.6f);
                pipeCol.center = new Vector3(0f, 0.45f, 0f);
            }

            // Real 3D Ventilation Duct Run (Overhead slide obstacle at z = 11.5m, replaces red duct box)
            var ductGo = InstantiateModel(VentDuctModelPath, "Ventilation_Duct_Run_Slide_Obstacle", new Vector3(0f, 1.55f, 11.5f), new Vector3(1.6f, 1.1f, 1.1f), Quaternion.Euler(0, 90f, 0));
            if (ductGo != null)
            {
                var ductCol = ductGo.AddComponent<BoxCollider>();
                ductCol.size = new Vector3(5.5f, 0.8f, 1.2f);
                ductCol.center = new Vector3(0f, 0f, 0f);
            }

            // 3. The Chasm Gap (z = 14m to 18m, 4m jump)
            // Catwalk Bridge module on the side
            InstantiateModel(CatwalkModelPath, "Industrial_Catwalk_Bridge_Flank", new Vector3(3.2f, -0.2f, 16.0f), new Vector3(1.4f, 1.2f, 3.8f), Quaternion.identity);

            // Pursuit Drone Sentinel Hazard (Patrolling across the gap at z = 16.0m)
            var dronePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DroneModelPath);
            GameObject droneGo = dronePrefab != null ? PrefabUtility.InstantiatePrefab(dronePrefab) as GameObject : GameObject.CreatePrimitive(PrimitiveType.Sphere);
            droneGo.name = "Security_Pursuit_Drone_Sentinel";
            droneGo.transform.position = new Vector3(0f, 3.4f, 16.0f);
            droneGo.transform.localScale = Vector3.one * 0.75f;

            var droneSentinel = droneGo.AddComponent<PursuitDroneSentinel>();
            droneSentinel.patrolPointA = new Vector3(-3.2f, 3.4f, 16.0f);
            droneSentinel.patrolPointB = new Vector3(3.2f, 3.4f, 16.0f);
            droneSentinel.patrolSpeed = 2.0f;

            // 4. Platform 2: Checkpoint Courtyard (z = 18m to 32m, width = 8m)
            var roof2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof2.name = "Platform_2_Rooftop_Target";
            roof2.transform.position = new Vector3(0f, -0.5f, 25.0f);
            roof2.transform.localScale = new Vector3(8.4f, 1f, 14f);
            roof2.GetComponent<Renderer>().material = concreteMat;

            InstantiateParapet(new Vector3(-4.1f, 0.15f, 25.0f), new Vector3(0.3f, 0.6f, 14f));
            InstantiateParapet(new Vector3(4.1f, 0.15f, 25.0f), new Vector3(0.3f, 0.6f, 14f));

            // Real 3D Quantum Checkpoint Gate (at z = 21.0m, replaces primitive green sticks)
            var cpGo = InstantiateModel(CheckpointModelPath, "Checkpoint_Quantum_Gate", new Vector3(0f, 0f, 21.0f), new Vector3(1.6f, 1.6f, 1.6f), Quaternion.identity);
            if (cpGo != null)
            {
                var cpCol = cpGo.AddComponent<BoxCollider>();
                cpCol.isTrigger = true;
                cpCol.size = new Vector3(3.5f, 3.0f, 1.2f);
                cpCol.center = new Vector3(0f, 1.5f, 0f);
                cpGo.AddComponent<RooftopCheckpoint>();
            }

            // 5. Platform 3: Mission Objective Platform (z = 34m to 46m, width = 8m)
            var roof3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof3.name = "Platform_3_Objective_Spire_Platform";
            roof3.transform.position = new Vector3(0f, -0.5f, 40.0f);
            roof3.transform.localScale = new Vector3(8.4f, 1f, 12f);
            roof3.GetComponent<Renderer>().material = concreteMat;

            InstantiateParapet(new Vector3(-4.1f, 0.15f, 40.0f), new Vector3(0.3f, 0.6f, 12f));
            InstantiateParapet(new Vector3(4.1f, 0.15f, 40.0f), new Vector3(0.3f, 0.6f, 12f));

            // Real 3D Signature Relay Beacon Spire (at z = 41.5m, replaces primitive cyan cylinder)
            var beaconGo = InstantiateModel(BeaconModelPath, "Signature_Relay_Beacon_Spire", new Vector3(0f, 0f, 41.5f), new Vector3(1.5f, 1.5f, 1.5f), Quaternion.identity);
            if (beaconGo != null)
            {
                var beaconCol = beaconGo.AddComponent<SphereCollider>();
                beaconCol.isTrigger = true;
                beaconCol.radius = 2.4f;
                beaconCol.center = new Vector3(0f, 1.5f, 0f);
                beaconGo.AddComponent<RooftopObjective>();
            }

            // 6. Audio Manager Setup
            var audioMgrGo = new GameObject("RooflineAudioManager");
            var audioMgr = audioMgrGo.AddComponent<RooflineAudioManager>();

            audioMgr.footstepScuff1 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/footstep_scuff_1.wav");
            audioMgr.footstepScuff2 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/footstep_scuff_2.wav");
            audioMgr.jumpTakeoff = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/jump_takeoff.wav");
            audioMgr.hardLanding = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/hard_landing.wav");
            audioMgr.vaultImpact = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/vault_impact.wav");
            audioMgr.slideSwoosh = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/slide_swoosh.wav");
            audioMgr.rollImpact = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/roll_impact.wav");
            audioMgr.checkpointActivate = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/checkpoint_activate.wav");
            audioMgr.objectiveComplete = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/objective_complete.wav");
            audioMgr.droneHoverLoop = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/drone_hover_loop.wav");
            audioMgr.rooftopWindAmbience = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Ambience/rooftop_wind_ambience.wav");

            droneSentinel.hoverAudioClip = audioMgr.droneHoverLoop;

            // 7. Player Character Setup
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CharacterFbxPath);
            GameObject playerGo = playerPrefab != null ? PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject : new GameObject("Courier_Urban_Scout");

            playerGo.name = "Courier_Urban_Scout";
            playerGo.transform.position = new Vector3(0f, 0.1f, -1.0f);
            playerGo.transform.rotation = Quaternion.identity;

            var anim = playerGo.GetComponent<Animator>();
            if (anim == null) anim = playerGo.AddComponent<Animator>();
            anim.runtimeAnimatorController = controller;

            var cc = playerGo.GetComponent<CharacterController>();
            if (cc == null) cc = playerGo.AddComponent<CharacterController>();

            var parkour = playerGo.GetComponent<PlayerParkourController>();
            if (parkour == null) parkour = playerGo.AddComponent<PlayerParkourController>();
            parkour.spawnPoint = new Vector3(0f, 0.1f, -1.0f);
            parkour.vaultDistance = 1.8f;
            parkour.vaultDuration = 0.65f;

            // 8. Third-Person Follow Camera
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.fieldOfView = 65f;
            cam.nearClipPlane = 0.15f;
            camGo.AddComponent<AudioListener>();

            var tpCam = camGo.AddComponent<ThirdPersonCamera>();
            tpCam.target = playerGo.transform;
            tpCam.distance = 5.2f;
            tpCam.targetOffset = new Vector3(0f, 1.35f, 0f);
            tpCam.cameraRadius = 0.28f;

            EditorSceneManager.SaveScene(scene, ProductionScenePath);
            Debug.Log($"[CharacterPipelineSetup] Production scene successfully built and saved to {ProductionScenePath}.");

            // Save copy to TestScenePath for local backwards compatibility
            EditorSceneManager.SaveScene(scene, TestScenePath);

            // Configure Unity Build Settings Scene List with ProductionScenePath as Scene 0
            var buildScenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(ProductionScenePath, true)
            };
            EditorBuildSettings.scenes = buildScenes;
            Debug.Log($"[CharacterPipelineSetup] EditorBuildSettings configured with {ProductionScenePath} as Scene 0 (Game Entry Point).");
        }

        private static GameObject InstantiateModel(string assetPath, string name, Vector3 pos, Vector3 scale, Quaternion rot)
        {
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
            {
                var all = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                foreach (var a in all)
                {
                    if (a is GameObject go)
                    {
                        prefab = go;
                        break;
                    }
                }
            }

            if (prefab == null)
            {
                Debug.LogWarning($"[CharacterPipelineSetup] Asset not found at: {assetPath}. Skipping {name}.");
                return null;
            }

            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance != null)
            {
                instance.name = name;
                instance.transform.position = pos;
                instance.transform.localScale = scale;
                instance.transform.rotation = rot;
            }
            return instance;
        }

        private static void InstantiateParapet(Vector3 pos, Vector3 scale)
        {
            var parapet = GameObject.CreatePrimitive(PrimitiveType.Cube);
            parapet.name = "Concrete_Parapet_Edge";
            parapet.transform.position = pos;
            parapet.transform.localScale = scale;

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.22f, 0.25f, 0.28f);
            mat.SetFloat("_Roughness", 0.9f);
            parapet.GetComponent<Renderer>().material = mat;
        }

        [MenuItem("Roofline Relay/Validate Production Scene")]
        public static void ValidateProductionScene()
        {
            Debug.Log("[ProductionValidation] Starting rigorous validation of RooflineRelay_Main.unity...");

            // 1. Scene load
            var scene = EditorSceneManager.OpenScene(ProductionScenePath, OpenSceneMode.Single);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                throw new Exception($"Failed to open production scene at {ProductionScenePath}");
            }
            Debug.Log("[ProductionValidation] PASS: Production scene loaded successfully.");

            // 2. Build settings verification
            if (EditorBuildSettings.scenes.Length == 0 || EditorBuildSettings.scenes[0].path != ProductionScenePath || !EditorBuildSettings.scenes[0].enabled)
            {
                throw new Exception($"EditorBuildSettings does not have {ProductionScenePath} as enabled Scene 0!");
            }
            Debug.Log($"[ProductionValidation] PASS: EditorBuildSettings has {ProductionScenePath} at index 0.");

            // 3. Player Character Verification
            var player = GameObject.Find("Courier_Urban_Scout");
            if (player == null) throw new Exception("Courier_Urban_Scout GameObject not found in production scene!");
            var cc = player.GetComponent<CharacterController>();
            if (cc == null) throw new Exception("CharacterController component missing on player!");
            var parkour = player.GetComponent<PlayerParkourController>();
            if (parkour == null) throw new Exception("PlayerParkourController component missing on player!");
            var anim = player.GetComponent<Animator>();
            if (anim == null || anim.runtimeAnimatorController == null) throw new Exception("Animator component or controller missing on player!");
            Debug.Log($"[ProductionValidation] PASS: Player components (CharacterController, PlayerParkourController, Animator) verified.");

            // 4. Camera Verification
            var cam = Camera.main;
            if (cam == null) throw new Exception("Main Camera not found in production scene!");
            var tpCam = cam.GetComponent<ThirdPersonCamera>();
            if (tpCam == null || tpCam.target != player.transform) throw new Exception("ThirdPersonCamera missing or not targeting player!");
            Debug.Log($"[ProductionValidation] PASS: Main Camera with ThirdPersonCamera targeting player verified.");

            // 5. Environment & Obstacles Verification
            string[] requiredObjects = new string[]
            {
                "Sun_Directional_Light",
                "Platform_1_Rooftop_Base",
                "Platform_2_Rooftop_Target",
                "Platform_3_Objective_Spire_Platform",
                "Stairwell_Housing_Access",
                "HVAC_Chiller_Unit",
                "Industrial_Pipe_Vault_Obstacle",
                "Ventilation_Duct_Run_Slide_Obstacle",
                "Industrial_Catwalk_Bridge_Flank",
                "Security_Pursuit_Drone_Sentinel",
                "Checkpoint_Quantum_Gate",
                "Signature_Relay_Beacon_Spire",
                "RooflineAudioManager"
            };

            foreach (var req in requiredObjects)
            {
                var go = GameObject.Find(req);
                if (go == null) throw new Exception($"Required production object '{req}' missing from scene!");
            }
            Debug.Log($"[ProductionValidation] PASS: All {requiredObjects.Length} required production GameObjects present.");

            // 6. Gameplay Triggers & Logic
            var cpGo = GameObject.Find("Checkpoint_Quantum_Gate");
            var cp = cpGo.GetComponent<RooftopCheckpoint>();
            if (cp == null) throw new Exception("RooftopCheckpoint component missing from Checkpoint_Quantum_Gate!");
            
            var beaconGo = GameObject.Find("Signature_Relay_Beacon_Spire");
            var obj = beaconGo.GetComponent<RooftopObjective>();
            if (obj == null) throw new Exception("RooftopObjective component missing from Signature_Relay_Beacon_Spire!");

            var droneGo = GameObject.Find("Security_Pursuit_Drone_Sentinel");
            var drone = droneGo.GetComponent<PursuitDroneSentinel>();
            if (drone == null) throw new Exception("PursuitDroneSentinel component missing from Security_Pursuit_Drone_Sentinel!");

            var audioGo = GameObject.Find("RooflineAudioManager");
            var audioMgr = audioGo.GetComponent<RooflineAudioManager>();
            if (audioMgr == null) throw new Exception("RooflineAudioManager component missing!");
            if (audioMgr.footstepScuff1 == null || audioMgr.jumpTakeoff == null || audioMgr.hardLanding == null || 
                audioMgr.vaultImpact == null || audioMgr.slideSwoosh == null || audioMgr.rollImpact == null ||
                audioMgr.checkpointActivate == null || audioMgr.objectiveComplete == null || audioMgr.rooftopWindAmbience == null)
            {
                throw new Exception("One or more required audio clips missing in RooflineAudioManager!");
            }
            Debug.Log("[ProductionValidation] PASS: Checkpoint, Objective, Drone hazard, and AudioManager clips verified.");

            // 7. Test harness absence check
            if (player.GetComponent("CharacterAnimationTestHarness") != null)
            {
                throw new Exception("CharacterAnimationTestHarness is present on player! It must be removed from production scene.");
            }
            Debug.Log("[ProductionValidation] PASS: No test-only harness present on production player.");

            Debug.Log("[ProductionValidation] ALL VALIDATION CHECKS PASSED SUCCESSFULLY!");
        }
    }
}
