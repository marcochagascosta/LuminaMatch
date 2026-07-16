using System.IO;
using UnityEditor;
using UnityEngine;

namespace LuminaMatch.Editor
{
    public static class BuildScripts
    {
        [MenuItem("Lumina Match/Build Android APK (Debug)")]
        public static void BuildAndroidApk()
        {
            ProjectSetup.SetupScenes();
            ConfigureAndroidForEmulators();

            string dir = Path.Combine(Directory.GetParent(Application.dataPath)!.FullName, "Builds", "Android");
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, "LuminaMatch-debug.apk");

            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Boot.unity" },
                locationPathName = path,
                target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };

            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"[Lumina Match] Android build: {report.summary.result} -> {path}");
        }

        /// <summary>
        /// BlueStacks/emulators are unreliable with GameActivity + Vulkan.
        /// Force classic Activity + OpenGLES3.
        /// </summary>
        static void ConfigureAndroidForEmulators()
        {
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.marcosaas.luminamatch");
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            // Prefer classic Activity over GameActivity (Unity 6 default).
            try
            {
                var androidType = typeof(PlayerSettings).GetNestedType("Android");
                var prop = androidType?.GetProperty("applicationEntry");
                if (prop != null)
                {
                    var enumType = prop.PropertyType;
                    // Activity = typically bit 1 / named "Activity"
                    object activityValue = System.Enum.Parse(enumType, "Activity");
                    prop.SetValue(null, activityValue);
                    Debug.Log("[Lumina Match] Android applicationEntry -> Activity");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[Lumina Match] Could not set applicationEntry: {ex.Message}");
            }

            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[]
            {
                UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3
            });
            Debug.Log("[Lumina Match] Android graphics API -> OpenGLES3");
        }

        [MenuItem("Lumina Match/Build iOS Xcode Project")]
        public static void BuildIos()
        {
            BuildIosInternal(iOSSdkVersion.DeviceSDK, "iOS");
        }

        [MenuItem("Lumina Match/Build iOS Simulator")]
        public static void BuildIosSimulator()
        {
            BuildIosInternal(iOSSdkVersion.SimulatorSDK, "iOS-Simulator");
        }

        static void BuildIosInternal(iOSSdkVersion sdk, string folderName)
        {
            ProjectSetup.SetupScenes();
            PlayerSettings.iOS.sdkVersion = sdk;
            // Apple Silicon simulators need ARM64. Serialized as iOSSimulatorArchitecture in ProjectSettings.
            if (sdk == iOSSdkVersion.SimulatorSDK)
                ForceIosSimulatorArm64();

            string dir = Path.Combine(Directory.GetParent(Application.dataPath)!.FullName, "Builds", folderName);
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
            Directory.CreateDirectory(dir);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Boot.unity" },
                locationPathName = dir,
                target = BuildTarget.iOS,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };

            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"[Lumina Match] iOS build ({sdk}): {report.summary.result} -> {dir}");
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
        }

        static void ForceIosSimulatorArm64()
        {
            // PlayerSettings.asset key: iOSSimulatorArchitecture (0=x86_64, 1=ARM64)
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (assets == null || assets.Length == 0) return;
            var so = new SerializedObject(assets[0]);
            var prop = so.FindProperty("iOSSimulatorArchitecture");
            if (prop != null)
            {
                prop.intValue = 1; // ARM64
                so.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssets();
                Debug.Log("[Lumina Match] iOSSimulatorArchitecture set to ARM64 (1)");
            }
            else
            {
                Debug.LogWarning("[Lumina Match] Could not find iOSSimulatorArchitecture property");
            }
        }
    }
}
