using UnityEditor;
using UnityEngine;

namespace LuminaMatch.Editor
{
    /// <summary>
    /// Unity Ads mediation prompts for Mobile Dependency Resolver via DisplayDialog,
    /// which aborts batchmode builds. Pref runs before InitializeOnLoadMethod of Ads.
    /// </summary>
    [InitializeOnLoad]
    static class SuppressMdrPrompt
    {
        const string DoNotAskAgain = "Unity.Mediation.MobileDependencyResolver.DoNotAskAgain";

        static SuppressMdrPrompt()
        {
            if (Application.isBatchMode)
                EditorPrefs.SetBool(DoNotAskAgain, true);
        }
    }
}
