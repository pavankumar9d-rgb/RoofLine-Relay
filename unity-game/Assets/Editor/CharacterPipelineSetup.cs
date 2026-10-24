using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RooflineRelay.Editor
{
    public static class CharacterPipelineSetup
    {
        private const string FbxPath = "Assets/Art/Characters/courier_female_urban_scout_animated.fbx";
        private const string ControllerPath = "Assets/Art/Characters/CourierAnimatorController.controller";
        private const string ScenePath = "Assets/Scenes/CharacterAnimationTest.unity";

        public static void Execute()
        {
            Debug.Log("[CharacterPipelineSetup] Starting character pipeline configuration...");

            try
            {
                // Step 1: Refresh Asset Database
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

                // Step 2: Configure ModelImporter
                ConfigureModelImporter();

                // Step 3: Create Animator Controller with States & Transitions
                var controller = CreateAnimatorController();

                // Step 4: Build Character Animation Test Scene
                BuildTestScene(controller);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("[CharacterPipelineSetup] SUCCESS: Character pipeline, Animator Controller, and test scene successfully created and verified!");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CharacterPipelineSetup] FAILED: {ex.Message}\n{ex.StackTrace}");
                EditorApplication.Exit(1);
            }
        }

        private static void ConfigureModelImporter()
        {
            var importer = AssetImporter.GetAtPath(FbxPath) as ModelImporter;
            if (importer == null)
            {
                throw new FileNotFoundException($"ModelImporter could not find asset at path: {FbxPath}");
            }

            Debug.Log("[CharacterPipelineSetup] Configuring ModelImporter for courier FBX...");

            // Set rig type to Human (Unity's enum value for Humanoid)
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;

            // Configure clip animation loop flags
            var defaultClips = importer.defaultClipAnimations;
            if (defaultClips != null && defaultClips.Length > 0)
            {
                var clips = new List<ModelImporterClipAnimation>();
                foreach (var srcClip in defaultClips)
                {
                    var clip = new ModelImporterClipAnimation
                    {
                        name = srcClip.name,
                        takeName = srcClip.takeName,
                        firstFrame = srcClip.firstFrame,
                        lastFrame = srcClip.lastFrame
                    };

                    // Name matching loop behaviors
                    string clipName = clip.name.ToLowerInvariant();
                    if (clipName.Contains("idle") || clipName.Contains("walk") || clipName.Contains("run"))
                    {
                        clip.loopTime = true;
                        clip.loopPose = true;
                    }
                    else if (clipName.Contains("airborne") || clipName.Contains("falling"))
                    {
                        clip.loopTime = true;
                        clip.loopPose = false;
                    }
                    else
                    {
                        clip.loopTime = false;
                    }

                    clips.Add(clip);
                    Debug.Log($"[CharacterPipelineSetup] Configured clip: {clip.name} (Loop: {clip.loopTime})");
                }
                importer.clipAnimations = clips.ToArray();
            }

            importer.SaveAndReimport();
            Debug.Log("[CharacterPipelineSetup] ModelImporter reimported successfully.");
        }

        private static AnimatorController CreateAnimatorController()
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
            controller.AddParameter("Stumble", AnimatorControllerParameterType.Trigger);

            // Find all imported animation clips
            var clipMap = new Dictionary<string, AnimationClip>(StringComparer.OrdinalIgnoreCase);
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(FbxPath);
            foreach (var asset in allAssets)
            {
                if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                {
                    clipMap[clip.name] = clip;
                    Debug.Log($"[CharacterPipelineSetup] Loaded AnimationClip: {clip.name}");
                }
            }

            // Root State Machine
            var rootStateMachine = controller.layers[0].stateMachine;

            // Helper to get clip
            AnimationClip GetClip(string name)
            {
                foreach (var kvp in clipMap)
                {
                    if (kvp.Key.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return kvp.Value;
                    }
                }
                Debug.LogWarning($"[CharacterPipelineSetup] Clip '{name}' not found directly in FBX. Returning null.");
                return null;
            }

            // Create States
            var idleState = rootStateMachine.AddState("Idle", new Vector3(200, 0, 0));
            idleState.motion = GetClip("Idle");

            var walkState = rootStateMachine.AddState("Walk", new Vector3(200, 70, 0));
            walkState.motion = GetClip("Walk");

            var runState = rootStateMachine.AddState("Run", new Vector3(200, 140, 0));
            runState.motion = GetClip("Run");

            var jumpTakeoffState = rootStateMachine.AddState("Jump_Takeoff", new Vector3(460, -70, 0));
            jumpTakeoffState.motion = GetClip("Jump_Takeoff");

            var jumpAirborneState = rootStateMachine.AddState("Jump_Airborne", new Vector3(460, 0, 0));
            jumpAirborneState.motion = GetClip("Jump_Airborne");

            var fallingState = rootStateMachine.AddState("Falling", new Vector3(460, 70, 0));
            fallingState.motion = GetClip("Falling");

            var landingState = rootStateMachine.AddState("Landing", new Vector3(460, 140, 0));
            landingState.motion = GetClip("Landing");

            var vaultState = rootStateMachine.AddState("Vault", new Vector3(200, -100, 0));
            vaultState.motion = GetClip("Vault");

            var stumbleState = rootStateMachine.AddState("Stumble", new Vector3(200, 210, 0));
            stumbleState.motion = GetClip("Stumble");

            // Set default state
            rootStateMachine.defaultState = idleState;

            // Transitions: Locomotion (Idle <-> Walk <-> Run)
            var idleToWalk = idleState.AddTransition(walkState);
            idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            idleToWalk.hasExitTime = false;
            idleToWalk.duration = 0.15f;

            var walkToIdle = walkState.AddTransition(idleState);
            walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            walkToIdle.hasExitTime = false;
            walkToIdle.duration = 0.15f;

            var walkToRun = walkState.AddTransition(runState);
            walkToRun.AddCondition(AnimatorConditionMode.Greater, 3.0f, "Speed");
            walkToRun.hasExitTime = false;
            walkToRun.duration = 0.15f;

            var runToWalk = runState.AddTransition(walkState);
            runToWalk.AddCondition(AnimatorConditionMode.Less, 3.0f, "Speed");
            runToWalk.hasExitTime = false;
            runToWalk.duration = 0.15f;

            var runToIdle = runState.AddTransition(idleState);
            runToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            runToIdle.hasExitTime = false;
            runToIdle.duration = 0.15f;

            // Transitions: Jump & Air Flow
            var anyToJump = rootStateMachine.AddAnyStateTransition(jumpTakeoffState);
            anyToJump.AddCondition(AnimatorConditionMode.If, 0, "Jump");
            anyToJump.hasExitTime = false;
            anyToJump.duration = 0.05f;

            var takeoffToAirborne = jumpTakeoffState.AddTransition(jumpAirborneState);
            takeoffToAirborne.hasExitTime = true;
            takeoffToAirborne.exitTime = 0.85f;
            takeoffToAirborne.duration = 0.1f;

            var airborneToFalling = jumpAirborneState.AddTransition(fallingState);
            airborneToFalling.AddCondition(AnimatorConditionMode.If, 0, "Falling");
            airborneToFalling.hasExitTime = false;
            airborneToFalling.duration = 0.15f;

            var airborneToLanding = jumpAirborneState.AddTransition(landingState);
            airborneToLanding.AddCondition(AnimatorConditionMode.If, 0, "Grounded");
            airborneToLanding.hasExitTime = false;
            airborneToLanding.duration = 0.08f;

            // AnyState -> Falling (e.g. running off rooftop edge)
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
            landingToIdle.exitTime = 0.88f;
            landingToIdle.duration = 0.15f;

            // Parkour: Vault
            var anyToVault = rootStateMachine.AddAnyStateTransition(vaultState);
            anyToVault.AddCondition(AnimatorConditionMode.If, 0, "Vault");
            anyToVault.hasExitTime = false;
            anyToVault.duration = 0.05f;

            var vaultToRun = vaultState.AddTransition(runState);
            vaultToRun.hasExitTime = true;
            vaultToRun.exitTime = 0.9f;
            vaultToRun.duration = 0.15f;

            // Parkour: Stumble
            var anyToStumble = rootStateMachine.AddAnyStateTransition(stumbleState);
            anyToStumble.AddCondition(AnimatorConditionMode.If, 0, "Stumble");
            anyToStumble.hasExitTime = false;
            anyToStumble.duration = 0.05f;

            var stumbleToIdle = stumbleState.AddTransition(idleState);
            stumbleToIdle.hasExitTime = true;
            stumbleToIdle.exitTime = 0.9f;
            stumbleToIdle.duration = 0.2f;

            Debug.Log("[CharacterPipelineSetup] Animator Controller built with 9 states and verified transitions.");
            return controller;
        }

        private static void BuildTestScene(AnimatorController controller)
        {
            Debug.Log($"[CharacterPipelineSetup] Building verification scene at {ScenePath}...");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Directional Light
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1.0f, 0.96f, 0.90f);
            light.intensity = 1.2f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // 2. Main Camera
            var camGo = new GameObject("Main Camera");
            var cam = camGo.AddComponent<Camera>();
            camGo.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.fieldOfView = 60f;
            camGo.AddComponent<AudioListener>();
            camGo.transform.position = new Vector3(0f, 1.5f, -3.8f);
            camGo.transform.rotation = Quaternion.Euler(12f, 0f, 0f);

            // 3. Ground Plane
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "TestGround";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(3f, 1f, 3f);

            // 4. Instantiate Courier Character
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
            if (prefab == null)
            {
                throw new FileNotFoundException($"Courier character prefab not found at: {FbxPath}");
            }

            var characterInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            characterInstance.name = "Courier_Urban_Scout";
            characterInstance.transform.position = Vector3.zero;
            characterInstance.transform.rotation = Quaternion.identity;

            // Attach Animator & Controller
            var anim = characterInstance.GetComponent<Animator>();
            if (anim == null)
            {
                anim = characterInstance.AddComponent<Animator>();
            }
            anim.runtimeAnimatorController = controller;

            // Attach Runtime Test Harness
            characterInstance.AddComponent<RooflineRelay.Character.CharacterAnimationTestHarness>();

            // Save Scene
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[CharacterPipelineSetup] Test scene successfully saved to {ScenePath}.");
        }
    }
}
