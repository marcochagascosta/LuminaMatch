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
        public void PlayMatch() => PlayTone(880f, 0.08f, 0.25f);
        public void PlayWin() => PlayTone(1046f, 0.2f, 0.3f);
        public void PlayFail() => PlayTone(220f, 0.15f, 0.3f);

        void PlayTone(float hz, float duration, float volume)
        {
            int sampleRate = 44100;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            var clip = AudioClip.Create("sfx", samples, 1, sampleRate, false);
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
    }
}
