using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LuminaMatch.Editor
{
    public static class BuildScripts
    {
        [MenuItem("Lumina Match/Build Android APK (Debug)")]
        public static void BuildAndroidApk()
        {
            ProjectSetup.SetupScenes();
            ConfigureAndroidCommon();

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
            Debug.Log($"[Lumina Match] Android APK: {report.summary.result} -> {path}");
        }

        [MenuItem("Lumina Match/Build Android AAB (Release)")]
        public static void BuildAndroidAab()
        {
            ProjectSetup.SetupScenes();
            ConfigureAndroidCommon();
            ApplyAndroidReleaseSigning();
            ApplyAppIconIfPresent();

            string dir = Path.Combine(Directory.GetParent(Application.dataPath)!.FullName, "Builds", "Android");
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, "LuminaMatch-release.aab");

            EditorUserBuildSettings.buildAppBundle = true;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Boot.unity" },
                locationPathName = path,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"[Lumina Match] Android AAB: {report.summary.result} -> {path}");
            if (report.summary.result != BuildResult.Succeeded)
                EditorApplication.Exit(1);
        }

        static void ConfigureAndroidCommon()
        {
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.marcosaas.luminamatch");
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.bundleVersion = "0.1.2";
            PlayerSettings.Android.bundleVersionCode = 3;

            try
            {
                var androidType = typeof(PlayerSettings).GetNestedType("Android");
                var prop = androidType?.GetProperty("applicationEntry");
                if (prop != null)
                {
                    object activityValue = Enum.Parse(prop.PropertyType, "Activity");
                    prop.SetValue(null, activityValue);
                    Debug.Log("[Lumina Match] Android applicationEntry -> Activity");
                }
            }
            catch (Exception ex)
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

        static void ApplyAndroidReleaseSigning()
        {
            string keystore = Environment.GetEnvironmentVariable("LUMINA_KEYSTORE");
            string alias = Environment.GetEnvironmentVariable("LUMINA_KEYALIAS");
            string storePass = Environment.GetEnvironmentVariable("LUMINA_STOREPASS");
            string keyPass = Environment.GetEnvironmentVariable("LUMINA_KEYPASS");

            if (string.IsNullOrEmpty(keystore))
            {
                keystore = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".lumina-match-secrets", "lumina-upload.keystore");
                alias = "lumina_upload";
                storePass = "LuminaMatch2026Upload!";
                keyPass = "LuminaMatch2026Upload!";
            }

            if (!File.Exists(keystore))
                throw new FileNotFoundException("Android keystore not found", keystore);

            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = keystore;
            PlayerSettings.Android.keyaliasName = alias;
            PlayerSettings.Android.keystorePass = storePass;
            PlayerSettings.Android.keyaliasPass = keyPass;
            Debug.Log($"[Lumina Match] Using keystore: {keystore}");
        }

        static void ApplyAppIconIfPresent()
        {
            string iconPath = "Assets/Art/Icons/lumina-match-icon-1024.png";
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
            if (tex == null)
            {
                Debug.LogWarning($"[Lumina Match] Icon not found at {iconPath}");
                return;
            }

            var importer = AssetImporter.GetAtPath(iconPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new[] { tex });
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, new[] { tex });
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.iOS, new[] { tex });
            Debug.Log("[Lumina Match] App icon applied from Art/Icons");
        }

        [MenuItem("Lumina Match/Build iOS Xcode Project")]
        public static void BuildIos()
        {
            BuildIosInternal(iOSSdkVersion.DeviceSDK, "iOS", development: false);
        }

        [MenuItem("Lumina Match/Build iOS Simulator")]
        public static void BuildIosSimulator()
        {
            BuildIosInternal(iOSSdkVersion.SimulatorSDK, "iOS-Simulator", development: true);
        }

        static void BuildIosInternal(iOSSdkVersion sdk, string folderName, bool development)
        {
            ProjectSetup.SetupScenes();
            ApplyAppIconIfPresent();
            PlayerSettings.iOS.sdkVersion = sdk;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.marcosaas.luminamatch");
            PlayerSettings.bundleVersion = "0.1.2";
            PlayerSettings.iOS.buildNumber = "4";
            PlayerSettings.iOS.appleEnableAutomaticSigning = true;
            // Encryption compliance for apps without custom crypto
            PlayerSettings.iOS.allowHTTPDownload = false;

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
                options = development
                    ? BuildOptions.Development | BuildOptions.AllowDebugging
                    : BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"[Lumina Match] iOS build ({sdk}): {report.summary.result} -> {dir}");
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            if (report.summary.result != BuildResult.Succeeded)
                EditorApplication.Exit(1);
        }

        static void ForceIosSimulatorArm64()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (assets == null || assets.Length == 0) return;
            var so = new SerializedObject(assets[0]);
            var prop = so.FindProperty("iOSSimulatorArchitecture");
            if (prop != null)
            {
                prop.intValue = 1;
                so.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssets();
                Debug.Log("[Lumina Match] iOSSimulatorArchitecture set to ARM64 (1)");
            }
        }
    }
}
