using UnityEngine;

namespace LuminaMatch.Audio
{
    /// <summary>
    /// Looping BGM. Prefers Resources/Audio/bgm if present; otherwise a procedural melody loop.
    /// </summary>
    public class MusicPlayer : MonoBehaviour
    {
        public static MusicPlayer Instance { get; private set; }

        const float TargetVolume = 1f;

        AudioSource _source;
        AudioClip _procedural;
        bool _started;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            AudioListener.volume = 1f;
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.loop = true;
            _source.volume = TargetVolume;
            _source.priority = 0;
            _source.spatialBlend = 0f;
            _source.mute = false;
            _source.bypassEffects = true;
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
            if (_source == null || _source.clip == null)
            {
                Debug.LogWarning("[LuminaMatch] BGM clip missing");
                return;
            }
            _source.mute = false;
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

        /// <summary>
        /// Light match-3 style melody: moving arpeggio + soft bass, no stuck drone, no clipping.
        /// </summary>
        static AudioClip BuildProceduralLoop()
        {
            const int sampleRate = 44100;
            const float bpm = 112f;
            const float beat = 60f / bpm;
            // 8 bars of 4/4
            const int beats = 32;
            float duration = beat * beats;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            var data = new float[samples];

            // Melody pattern (Hz) — steps through a cheerful scale, not a static chord.
            float[] melody =
            {
                392.00f, 493.88f, 587.33f, 659.25f, // G4 B4 D5 E5
                587.33f, 493.88f, 392.00f, 329.63f, // D5 B4 G4 E4
                349.23f, 440.00f, 523.25f, 659.25f, // F4 A4 C5 E5
                523.25f, 440.00f, 349.23f, 293.66f, // C5 A4 F4 D4
                392.00f, 493.88f, 587.33f, 783.99f, // G4 B4 D5 G5
                659.25f, 587.33f, 493.88f, 392.00f,
                440.00f, 523.25f, 659.25f, 783.99f,
                659.25f, 523.25f, 440.00f, 392.00f
            };

            float[] bass = { 98.00f, 87.31f, 110.00f, 98.00f }; // G2 F2 A2 G2 (per bar)

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float beatPos = t / beat;
                int beatIndex = Mathf.Clamp(Mathf.FloorToInt(beatPos), 0, melody.Length - 1);
                float beatFrac = beatPos - beatIndex;

                // Melody note with pluck envelope each beat
                float melEnv = Mathf.Exp(-beatFrac * 3.2f) * (1f - Mathf.SmoothStep(0.85f, 1f, beatFrac));
                float melHz = melody[beatIndex];
                float mel = Mathf.Sin(2f * Mathf.PI * melHz * t) * 0.34f * melEnv;
                // Soft octave sparkle
                mel += Mathf.Sin(2f * Mathf.PI * melHz * 2f * t) * 0.08f * melEnv;

                // Bass pulse every 2 beats
                int bar = (beatIndex / 4) % bass.Length;
                float bassFrac = (beatPos * 0.5f) % 1f;
                float bassEnv = Mathf.Exp(-bassFrac * 2.4f) * 0.9f;
                float bas = Mathf.Sin(2f * Mathf.PI * bass[bar] * t) * 0.22f * bassEnv;

                // Very quiet moving pad (changes root with bar) — not a stuck chord
                float padHz = bass[bar] * 2f;
                float pad = Mathf.Sin(2f * Mathf.PI * padHz * t) * 0.05f;
                pad += Mathf.Sin(2f * Mathf.PI * (padHz * 1.5f) * t) * 0.03f;

                data[i] = mel + bas + pad;
            }

            // Peak normalize without hard clip
            float peak = 0.0001f;
            for (int i = 0; i < samples; i++)
            {
                float a = Mathf.Abs(data[i]);
                if (a > peak) peak = a;
            }
            float gain = 0.92f / peak;
            for (int i = 0; i < samples; i++)
                data[i] *= gain;

            // Seamless loop crossfade
            int fade = Mathf.Min(sampleRate / 8, samples / 16);
            for (int i = 0; i < fade; i++)
            {
                float k = i / (float)fade;
                float wOut = Mathf.Cos(k * Mathf.PI * 0.5f);
                float wIn = Mathf.Sin(k * Mathf.PI * 0.5f);
                float mixed = data[samples - fade + i] * wOut + data[i] * wIn;
                data[samples - fade + i] = mixed;
                data[i] = mixed;
            }

            var clip = AudioClip.Create("bgm_melody_v27", samples, 1, sampleRate, false);
            if (clip == null) return null;
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
