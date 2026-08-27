using UnityEngine;

namespace LuminaMatch.Audio
{
    public static class Haptics
    {
        public static void PulseMove()
        {
            var p = Economy.PlayerProgress.Instance;
            if (p != null && !p.Data.VibrateOn) return;
#if UNITY_ANDROID || UNITY_IOS
            try { Handheld.Vibrate(); }
            catch { /* ignore */ }
#endif
        }
    }
}
