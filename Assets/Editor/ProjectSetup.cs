using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LuminaMatch.Editor
{
    public static class ProjectSetup
    {
        [MenuItem("Lumina Match/Setup Project Scenes")]
        public static void SetupScenes()
        {
            EnsureFolder("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            scene.name = "Boot";

            var cam = Camera.main;
            if (cam != null)
            {
                cam.backgroundColor = new Color(0.08f, 0.07f, 0.16f);
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.orthographic = true;
            }

            var boot = new GameObject("App");
            boot.AddComponent<Core.AppBootstrap>();

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Boot.unity");
            var scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/Boot.unity", true)
            };
            EditorBuildSettings.scenes = scenes;

            PlayerSettings.companyName = "MarcoSaas";
            PlayerSettings.productName = "Lumina Match";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.marcosaas.luminamatch");
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.marcosaas.luminamatch");
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.iOS.buildNumber = "1";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

            AssetDatabase.SaveAssets();
            Debug.Log("[Lumina Match] Boot scene + player settings configured.");
        }

        [MenuItem("Lumina Match/Run Edit Mode Tests Now")]
        public static void RunTests()
        {
            UnityEditor.TestTools.TestRunner.Api.Filter filter = new()
            {
                testMode = UnityEditor.TestTools.TestRunner.Api.TestMode.EditMode
            };
            var api = ScriptableObject.CreateInstance<UnityEditor.TestTools.TestRunner.Api.TestRunnerApi>();
            api.Execute(new UnityEditor.TestTools.TestRunner.Api.ExecutionSettings(filter));
            Debug.Log("[Lumina Match] Edit Mode tests requested via Test Runner API.");
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
