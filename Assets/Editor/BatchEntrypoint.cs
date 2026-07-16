using UnityEditor;
using UnityEngine;

namespace LuminaMatch.Editor
{
    /// <summary>
    /// Batchmode entry points:
    /// Unity -batchmode -projectPath ... -executeMethod LuminaMatch.Editor.BatchEntrypoint.SetupAndTest -quit
    /// </summary>
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

        public static void BuildIosSimulator()
        {
            BuildScripts.BuildIosSimulator();
        }
    }
}
