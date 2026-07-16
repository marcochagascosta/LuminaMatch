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
            try
            {
                if (FindFirstObjectByType<AppBootstrap>() != null) return;

                var go = new GameObject("LuminaMatchApp");
                go.AddComponent<AppBootstrap>();
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        void Awake()
        {
            try
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

                Debug.Log("[LuminaMatch] AppBootstrap Awake OK");
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}
