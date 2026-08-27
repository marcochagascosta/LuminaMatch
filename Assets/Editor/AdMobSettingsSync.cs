using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using LuminaMatch.Monetization;

namespace LuminaMatch.Editor
{
    /// <summary>
    /// Copies AdMob App IDs from AdsConfig.json into the GMA plugin settings
    /// (that class is internal, so this uses reflection).
    /// </summary>
    [InitializeOnLoad]
    public class AdMobSettingsSync : IPreprocessBuildWithReport
    {
        public int callbackOrder => 1000;

        static AdMobSettingsSync()
        {
            EditorApplication.delayCall += Apply;
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            Apply();
            StripJCenterFromGradleTemplates();
        }

        [MenuItem("Lumina Match/Sync AdMob App IDs")]
        public static void Apply()
        {
            var cfg = AdsConfig.Current;
            if (string.IsNullOrEmpty(cfg.androidAppId) && string.IsNullOrEmpty(cfg.iosAppId))
                return;

            var settingsType = FindSettingsType();
            if (settingsType == null)
            {
                Debug.LogWarning("[LuminaMatch] Google Mobile Ads plugin not imported yet — skip App ID sync.");
                return;
            }

            var load = settingsType.GetMethod("LoadInstance", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (load == null) return;
            var instance = load.Invoke(null, null);
            if (instance == null) return;

            SetProp(settingsType, instance, "GoogleMobileAdsAndroidAppId", cfg.androidAppId);
            SetProp(settingsType, instance, "GoogleMobileAdsIOSAppId", cfg.iosAppId);
            SetBoolProp(settingsType, instance, "EnableGradleBuildPreProcessor", true);
            EditorUtility.SetDirty((UnityEngine.Object)instance);
            AssetDatabase.SaveAssets();
            Debug.Log($"[LuminaMatch] AdMob App IDs synced (Android={cfg.androidAppId}, iOS={cfg.iosAppId}).");
            StripJCenterFromGradleTemplates();
        }

        public static void StripJCenterFromGradleTemplates()
        {
            StripJCenterUnder(Path.Combine("Assets", "Plugins", "Android"));
            StripJCenterUnder(Path.Combine("Library", "Bee", "Android"));
        }

        public static void StripJCenterUnder(string root)
        {
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root)) return;
            foreach (var file in Directory.GetFiles(root, "*.gradle", SearchOption.AllDirectories))
            {
                var text = File.ReadAllText(file);
                if (!text.Contains("jcenter()")) continue;
                var next = text.Replace("        jcenter()\n", "").Replace("        jcenter()\r\n", "").Replace("jcenter()", "");
                if (next != text)
                {
                    File.WriteAllText(file, next);
                    Debug.Log($"[LuminaMatch] Removed jcenter() from {file} (Gradle 9).");
                }
            }
        }

        static void SetProp(System.Type type, object instance, string name, string value)
        {
            var prop = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null && prop.CanWrite)
                prop.SetValue(instance, value ?? "");
        }

        static void SetBoolProp(System.Type type, object instance, string name, bool value)
        {
            var prop = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null && prop.CanWrite)
                prop.SetValue(instance, value);
        }

        static System.Type FindSettingsType()
        {
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                System.Type t = null;
                try { t = asm.GetType("GoogleMobileAds.Editor.GoogleMobileAdsSettings"); }
                catch { continue; }
                if (t != null) return t;
            }
            return null;
        }
    }

    public class AdMobGradleJCenterStrip : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 99999;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            // path is the unityLibrary gradle project.
            var root = Directory.GetParent(path)?.FullName ?? path;
            AdMobSettingsSync.StripJCenterUnder(root);
            AdMobSettingsSync.StripJCenterUnder(path);
        }
    }
}
