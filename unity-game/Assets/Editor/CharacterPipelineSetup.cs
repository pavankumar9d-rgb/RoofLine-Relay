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

namespace RooflineRelay.Editor
{
    public static class CharacterPipelineSetup
    {
        private const string CharacterFbxPath = "Assets/Art/Characters/courier_female_urban_scout_rigged.fbx";
        private const string AnimFolder = "Assets/Art/Animations/CourierFemale";
        private const string ControllerPath = "Assets/Art/Characters/CourierAnimatorController.controller";
        private const string ScenePath = "Assets/Scenes/CharacterAnimationTest.unity";

        [MenuItem("Roofline Relay/Setup Character Pipeline & Playable Scene")]
        public static void Execute()
        {
            Debug.Log("[CharacterPipelineSetup] Starting Phase 4 Mixamo integration and playable prototype setup...");

            try
            {
                // Step 1: Refresh Asset Database
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

                // Step 2: Configure Base Character ModelImporter & Avatar
                Avatar baseAvatar = ConfigureBaseCharacterImporter();

                // Step 3: Configure Animation Clips ModelImporters
                var clipMap = ConfigureAnimationImporters(baseAvatar);

                // Step 4: Create Animator Controller with States & Transitions
                var controller = CreateAnimatorController(clipMap);

                // Step 5: Build Playable Rooftop Prototype Scene
                BuildPlayableRooftopScene(controller);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("[CharacterPipelineSetup] SUCCESS: Mixamo Humanoid integration, Animator Controller, and Playable Rooftop Course verified!");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CharacterPipelineSetup] FAILED: {ex.Message}\n{ex.StackTrace}");
                EditorApplication.Exit(1);
            }
        }

        private static Avatar ConfigureBaseCharacterImporter()
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

            Debug.Log("[CharacterPipelineSetup] Base Character configured with Generic 1-to-1 Mixamo bone hierarchy.");
            return null;
        }

        private static Dictionary<string, AnimationClip> ConfigureAnimationImporters(Avatar baseAvatar)
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
                "UrbanScout_Vault.fbx"
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

            // Force synchronous asset refresh to register all new sub-assets in Unity's cache
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            // Now load all registered AnimationClip assets
            foreach (var file in animFiles)
            {
                string assetPath = $"{AnimFolder}/{file}";
                string clipName = Path.GetFileNameWithoutExtension(file).Replace("UrbanScout_", "");

                var allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                Debug.Log($"[CharacterPipelineSetup] Inspecting {file}: {allAssets.Length} assets found.");
                foreach (var asset in allAssets)
                {
                    if (asset != null)
                    {
                        Debug.Log($"  -> Asset: {asset.GetType().Name} : '{asset.name}'");
                        if (asset is AnimationClip animClip && !animClip.name.StartsWith("__preview__"))
                        {
                            clipMap[clipName] = animClip;
                            Debug.Log($"[CharacterPipelineSetup] Registered clip '{clipName}' -> {animClip.name} ({animClip.length:F2}s)");
                            break;
                        }
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

            // Add Parameters
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
            controller.AddParameter("HardLanding", AnimatorControllerParameterType.Trigger);

            var rootStateMachine = controller.layers[0].stateMachine;

            // Helper to get clip
            AnimationClip GetClip(string name)
            {
                if (clipMap.TryGetValue(name, out var clip)) return clip;
                Debug.LogWarning($"[CharacterPipelineSetup] Clip '{name}' not found in map. Using fallback.");
                return null;
            }

            // Create States
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

            // Default State
            rootStateMachine.defaultState = idleState;

            // Locomotion Transitions
            // Idle <-> Walk
            var idleToWalk = idleState.AddTransition(walkState);
            idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.15f, "Speed");
            idleToWalk.hasExitTime = false;
            idleToWalk.duration = 0.15f;

            var walkToIdle = walkState.AddTransition(idleState);
            walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.15f, "Speed");
            walkToIdle.hasExitTime = false;
            walkToIdle.duration = 0.15f;

            // Walk <-> Jog
            var walkToJog = walkState.AddTransition(jogState);
            walkToJog.AddCondition(AnimatorConditionMode.Greater, 2.5f, "Speed");
            walkToJog.hasExitTime = false;
            walkToJog.duration = 0.15f;

            var jogToWalk = jogState.AddTransition(walkState);
            jogToWalk.AddCondition(AnimatorConditionMode.Less, 2.5f, "Speed");
            jogToWalk.hasExitTime = false;
            jogToWalk.duration = 0.15f;

            // Jog <-> Run
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

            Debug.Log("[CharacterPipelineSetup] Animator Controller created with 8 clean states and transitions.");
            return controller;
        }

        private static void BuildPlayableRooftopScene(AnimatorController controller)
        {
            Debug.Log($"[CharacterPipelineSetup] Building playable rooftop course at {ScenePath}...");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Directional Sun Light (Industrial Dusk/Dawn)
            var lightGo = new GameObject("Sun_Directional_Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1.0f, 0.92f, 0.82f);
            light.intensity = 1.3f;
            lightGo.transform.rotation = Quaternion.Euler(45f, -35f, 0f);

            // 2. Playable Rooftop Route Construction
            // Rooftop Platform 1 (Start & Runup): 12m long x 8m wide
            var roof1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof1.name = "Rooftop_Start_Platform";
            roof1.transform.position = new Vector3(0f, -0.5f, 5f);
            roof1.transform.localScale = new Vector3(8f, 1f, 16f);

            // Vault Obstacle: Barrier placed across roof1 at z = 7m
            var hurdle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hurdle.name = "Vault_Barrier_Obstacle";
            hurdle.transform.position = new Vector3(0f, 0.45f, 7f);
            hurdle.transform.localScale = new Vector3(4f, 0.9f, 0.5f);
            var hurdleMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            hurdleMat.color = new Color(0.9f, 0.5f, 0.1f); // High-visibility amber
            hurdle.GetComponent<Renderer>().material = hurdleMat;

            // Rooftop Platform 2 (Across a 3.5m gap): z = 20m to 35m
            var roof2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof2.name = "Rooftop_Target_Platform";
            roof2.transform.position = new Vector3(0f, -0.5f, 24.5f);
            roof2.transform.localScale = new Vector3(8f, 1f, 15f);

            // Checkpoint Gate on Platform 2 at z = 20m
            var cpGo = new GameObject("Checkpoint_Quantum_Gate");
            cpGo.transform.position = new Vector3(0f, 0.5f, 20f);
            var cpCol = cpGo.AddComponent<BoxCollider>();
            cpCol.isTrigger = true;
            cpCol.size = new Vector3(4f, 2.5f, 1f);
            cpGo.AddComponent<RooftopCheckpoint>();

            // Visual Checkpoint Arch
            var archL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            archL.name = "Pillar_Left";
            archL.transform.SetParent(cpGo.transform);
            archL.transform.localPosition = new Vector3(-2f, 1f, 0f);
            archL.transform.localScale = new Vector3(0.4f, 2.2f, 0.4f);

            var archR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            archR.name = "Pillar_Right";
            archR.transform.SetParent(cpGo.transform);
            archR.transform.localPosition = new Vector3(2f, 1f, 0f);
            archR.transform.localScale = new Vector3(0.4f, 2.2f, 0.4f);

            // Rooftop Platform 3 (End Spire): z = 38m
            var roof3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof3.name = "Rooftop_Objective_Platform";
            roof3.transform.position = new Vector3(0f, -0.5f, 42f);
            roof3.transform.localScale = new Vector3(8f, 1f, 12f);

            // Objective Relay Spire on Platform 3 at z = 44m
            var objGo = new GameObject("Relay_Beacon_Objective");
            objGo.transform.position = new Vector3(0f, 1.5f, 44f);
            var objCol = objGo.AddComponent<SphereCollider>();
            objCol.isTrigger = true;
            objCol.radius = 2.0f;
            objGo.AddComponent<RooftopObjective>();

            var spire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spire.name = "Spire_Visual";
            spire.transform.SetParent(objGo.transform);
            spire.transform.localPosition = Vector3.zero;
            spire.transform.localScale = new Vector3(0.6f, 1.8f, 0.6f);
            var spireMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            spireMat.color = new Color(0.1f, 0.8f, 1.0f);
            spire.GetComponent<Renderer>().material = spireMat;

            // 3. Player Character Setup
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CharacterFbxPath);
            GameObject playerGo;
            if (playerPrefab != null)
            {
                playerGo = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
            }
            else
            {
                playerGo = new GameObject("Courier_Urban_Scout");
            }

            playerGo.name = "Courier_Urban_Scout";
            playerGo.transform.position = new Vector3(0f, 0.1f, 0f);
            playerGo.transform.rotation = Quaternion.identity;

            // Setup Components
            var anim = playerGo.GetComponent<Animator>();
            if (anim == null) anim = playerGo.AddComponent<Animator>();
            anim.runtimeAnimatorController = controller;

            var cc = playerGo.GetComponent<CharacterController>();
            if (cc == null) cc = playerGo.AddComponent<CharacterController>();

            var parkour = playerGo.GetComponent<PlayerParkourController>();
            if (parkour == null) parkour = playerGo.AddComponent<PlayerParkourController>();
            parkour.spawnPoint = new Vector3(0f, 0.1f, 0f);

            // Optional test harness for auto-demo
            var harness = playerGo.GetComponent<CharacterAnimationTestHarness>();
            if (harness == null) harness = playerGo.AddComponent<CharacterAnimationTestHarness>();
            harness.autoCycleDemo = false;

            // 4. Third-Person Follow Camera
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.fieldOfView = 60f;
            camGo.AddComponent<AudioListener>();

            var tpCam = camGo.AddComponent<ThirdPersonCamera>();
            tpCam.target = playerGo.transform;
            tpCam.distance = 4.2f;

            // Save Scene
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[CharacterPipelineSetup] Playable rooftop course scene successfully saved to {ScenePath}.");
        }
    }
}
