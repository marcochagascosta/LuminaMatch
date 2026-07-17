using UnityEngine;

namespace LuminaMatch.Audio
{
    /// <summary>Minimal feedback SFX using procedural one-shots (no external assets).</summary>
    public class SfxPlayer : MonoBehaviour
    {
        public static SfxPlayer Instance { get; private set; }
        AudioSource _source;

        void Awake()
        {
            if (Instance != null) { Destroy(this); return; }
            Instance = this;
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
        }

        public void PlayClick() => PlayTone(660f, 0.05f, 0.2f);
        public void PlayMatch()
        {
            float pitch = Random.Range(0.92f, 1.08f);
            PlayTone(880f * pitch, 0.08f, 0.25f);
        }
        public void PlayWin() => PlayTone(1046f, 0.2f, 0.3f);
        public void PlayFail() => PlayTone(220f, 0.15f, 0.3f);
        public void PlayPower() => PlayTone(1320f, 0.1f, 0.28f);
        public void PlayHint() => PlayTone(740f, 0.06f, 0.18f);

        void PlayTone(float hz, float duration, float volume)
        {
            try
            {
                if (_source == null) return;
                int sampleRate = 22050;
                int samples = Mathf.Max(1, Mathf.CeilToInt(sampleRate * duration));
                var clip = AudioClip.Create("sfx", samples, 1, sampleRate, false);
                if (clip == null) return;
                var data = new float[samples];
                for (int i = 0; i < samples; i++)
                {
                    float t = i / (float)sampleRate;
                    float env = 1f - (i / (float)samples);
                    data[i] = Mathf.Sin(2f * Mathf.PI * hz * t) * env * volume;
                }
                clip.SetData(data, 0);
                _source.PlayOneShot(clip);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[LuminaMatch] SFX skipped: {ex.Message}");
            }
        }
    }
}
