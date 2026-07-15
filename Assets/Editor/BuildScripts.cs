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
                options = BuildOptions.Development
            };

            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"[Lumina Match] Android build: {report.summary.result} -> {path}");
        }

        [MenuItem("Lumina Match/Build iOS Xcode Project")]
        public static void BuildIos()
        {
            ProjectSetup.SetupScenes();
            string dir = Path.Combine(Directory.GetParent(Application.dataPath)!.FullName, "Builds", "iOS");
            Directory.CreateDirectory(dir);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Boot.unity" },
                locationPathName = dir,
                target = BuildTarget.iOS,
                options = BuildOptions.Development
            };

            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"[Lumina Match] iOS build: {report.summary.result} -> {dir}");
        }
    }
}
