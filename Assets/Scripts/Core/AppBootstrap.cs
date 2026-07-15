using LuminaMatch.Audio;
using LuminaMatch.Economy;
using LuminaMatch.Monetization;
using LuminaMatch.UI;
using UnityEngine;

namespace LuminaMatch.Core
{
    public class AppBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoBoot()
        {
            if (FindFirstObjectByType<AppBootstrap>() != null) return;

            var go = new GameObject("LuminaMatchApp");
            go.AddComponent<AppBootstrap>();
        }

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            if (FindFirstObjectByType<MonetizationHub>() == null)
                gameObject.AddComponent<MonetizationHub>();

            if (FindFirstObjectByType<SfxPlayer>() == null)
                gameObject.AddComponent<SfxPlayer>();

            if (PlayerProgress.Instance == null)
                _ = new PlayerProgress();

            if (FindFirstObjectByType<UiRoot>() == null)
                gameObject.AddComponent<UiRoot>();
        }
    }
}
