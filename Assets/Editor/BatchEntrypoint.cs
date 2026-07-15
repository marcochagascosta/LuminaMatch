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

            // Compile validation: if we got here, scripts compiled.
            Debug.Log("[Lumina Match] SetupAndTest OK — scenes configured.");
            // Note: full Test Runner in batchmode uses -runTests -testPlatform EditMode
        }

        public static void BuildAndroid()
        {
            ProjectSetup.SetupScenes();
            BuildScripts.BuildAndroidApk();
        }
    }
}
