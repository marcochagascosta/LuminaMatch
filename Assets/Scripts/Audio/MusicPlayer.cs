using UnityEngine;

namespace LuminaMatch.Audio
{
    /// <summary>
    /// Looping BGM. Prefers Resources/Audio/bgm if present; otherwise a procedural loop.
    /// </summary>
    public class MusicPlayer : MonoBehaviour
    {
        public static MusicPlayer Instance { get; private set; }

        const float TargetVolume = 0.92f;

        AudioSource _source;
        AudioClip _procedural;
        bool _started;

        void Awake()
        {
            if (Instance != null) { Destroy(this); return; }
            Instance = this;
            AudioListener.volume = 1f;
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.loop = true;
            _source.volume = TargetVolume;
            _source.priority = 0;
            _source.spatialBlend = 0f;
        }

        void Start() => ApplyFromSave();

        public void ApplyFromSave()
        {
            var p = Economy.PlayerProgress.Instance;
            bool on = p == null || p.Data.MusicOn;
            if (!on)
            {
                if (_source != null && _source.isPlaying) _source.Stop();
                return;
            }

            EnsureClip();
            if (_source.clip == null) return;
            _source.volume = TargetVolume;
            if (!_source.isPlaying) _source.Play();
            _started = true;
        }

        void EnsureClip()
        {
            if (_source.clip != null) return;

            var file = Resources.Load<AudioClip>("Audio/bgm");
            if (file != null)
            {
                _source.clip = file;
                return;
            }

            if (_procedural == null)
                _procedural = BuildProceduralLoop();
            _source.clip = _procedural;
        }

        /// <summary>Louder ambient pad loop (~4s) — placeholder until Resources/Audio/bgm exists.</summary>
        static AudioClip BuildProceduralLoop()
        {
            const int sampleRate = 22050;
            const float duration = 4f;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            var data = new float[samples];
            float[] freqs = { 196.00f, 246.94f, 293.66f, 369.99f };
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float env = 0.7f + 0.3f * Mathf.Sin(t / duration * Mathf.PI * 2f);
                float s = 0f;
                for (int f = 0; f < freqs.Length; f++)
                {
                    float phase = 2f * Mathf.PI * freqs[f] * t;
                    float noteEnv = 0.55f + 0.45f * Mathf.Sin((t * 0.5f + f * 0.37f) * Mathf.PI * 2f);
                    s += Mathf.Sin(phase) * 0.28f * noteEnv;
                    s += Mathf.Sin(phase * 2f) * 0.08f * noteEnv;
                    s += Mathf.Sin(phase * 0.5f) * 0.12f * noteEnv;
                }
                data[i] = Mathf.Clamp(s * env * 1.35f, -1f, 1f);
            }

            var clip = AudioClip.Create("bgm_procedural", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        void OnApplicationPause(bool pause)
        {
            if (_source == null) return;
            if (pause) _source.Pause();
            else if (_started && Economy.PlayerProgress.Instance != null && Economy.PlayerProgress.Instance.Data.MusicOn)
                _source.UnPause();
        }
    }
}
