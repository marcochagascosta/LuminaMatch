using UnityEditor;
using UnityEngine;

namespace LuminaMatch.Editor
{
    public static class BatchEntrypoint
    {
        public static void SetupAndTest()
        {
            ProjectSetup.SetupScenes();
            if (!Application.isBatchMode)
                return;
            Debug.Log("[Lumina Match] SetupAndTest OK — scenes configured.");
        }

        public static void BuildAndroid()
        {
            ProjectSetup.SetupScenes();
            BuildScripts.BuildAndroidApk();
        }

        public static void BuildAndroidAab()
        {
            BuildScripts.BuildAndroidAab();
        }

        public static void BuildIos()
        {
            BuildScripts.BuildIos();
        }

        public static void BuildIosSimulator()
        {
            BuildScripts.BuildIosSimulator();
        }
    }
}
