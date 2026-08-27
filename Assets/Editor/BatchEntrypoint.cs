using UnityEditor;
using UnityEngine;

namespace LuminaMatch.Editor
{
    public static class BatchEntrypoint
    {
        public static void SetupAndTest()
        {
            ProjectSetup.SetupScenes();
            ProjectSetup.RunTests();
            if (!Application.isBatchMode)
                return;
            Debug.Log("[Lumina Match] SetupAndTest OK — scenes configured; Edit Mode tests requested.");
        }

        public static void RunEditModeTests()
        {
            ProjectSetup.RunTests();
        }

        public static void BuildAndroid()
        {
            BuildScripts.BuildAndroidApkRelease();
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
