using UnityEngine;

namespace LuminaMatch.UI
{
    /// <summary>Procedural white icons for settings (speaker / note / vibrate / gear).</summary>
    public static class SettingsIcons
    {
        static Sprite _speaker, _music, _vibrate, _gear, _speakerOff, _musicOff, _vibrateOff;

        public static Sprite Speaker(bool on) { Ensure(); return on ? _speaker : _speakerOff; }
        public static Sprite Music(bool on) { Ensure(); return on ? _music : _musicOff; }
        public static Sprite Vibrate(bool on) { Ensure(); return on ? _vibrate : _vibrateOff; }
        public static Sprite Gear { get { Ensure(); return _gear; } }

        static void Ensure()
        {
            if (_speaker != null) return;
            _speaker = DrawSpeaker(true);
            _speakerOff = DrawSpeaker(false);
            _music = DrawMusic(true);
            _musicOff = DrawMusic(false);
            _vibrate = DrawVibrate(true);
            _vibrateOff = DrawVibrate(false);
            _gear = DrawGear();
        }

        static Sprite DrawSpeaker(bool on)
        {
            const int s = 128;
            var tex = NewTex(s);
            // cone body
            FillTri(tex, s, 28, 40, 28, 88, 62, 64, Color.white);
            FillRect(tex, s, 18, 48, 28, 80, Color.white);
            if (on)
            {
                // sound waves
                StrokeArc(tex, s, 70, 64, 18, -50, 50, 4, Color.white);
                StrokeArc(tex, s, 70, 64, 30, -55, 55, 4, Color.white);
                StrokeArc(tex, s, 70, 64, 42, -58, 58, 4, Color.white);
            }
            else
            {
                // X
                StrokeLine(tex, s, 78, 48, 108, 78, 5, Color.white);
                StrokeLine(tex, s, 78, 78, 108, 48, 5, Color.white);
            }
            return ToSprite(tex);
        }

        static Sprite DrawMusic(bool on)
        {
            const int s = 128;
            var tex = NewTex(s);
            // stem
            FillRect(tex, s, 72, 28, 82, 88, Color.white);
            // note head
            FillEllipse(tex, s, 48, 88, 28, 20, Color.white);
            // flag
            FillTri(tex, s, 82, 28, 108, 40, 82, 52, Color.white);
            if (!on)
            {
                StrokeLine(tex, s, 24, 24, 104, 104, 6, new Color(1f, 0.35f, 0.35f, 1f));
            }
            return ToSprite(tex);
        }

        static Sprite DrawVibrate(bool on)
        {
            const int s = 128;
            var tex = NewTex(s);
            // phone body
            FillRoundRect(tex, s, 44, 28, 84, 100, 8, Color.white);
            FillRect(tex, s, 52, 36, 76, 88, new Color(0.55f, 0.35f, 0.85f, 1f));
            FillEllipse(tex, s, 64, 94, 8, 4, new Color(0.55f, 0.35f, 0.85f, 1f));
            if (on)
            {
                // side waves
                StrokeArc(tex, s, 36, 64, 14, 110, 250, 4, Color.white);
                StrokeArc(tex, s, 92, 64, 14, -70, 70, 4, Color.white);
                StrokeArc(tex, s, 28, 64, 22, 110, 250, 3, Color.white);
                StrokeArc(tex, s, 100, 64, 22, -70, 70, 3, Color.white);
            }
            else
            {
                StrokeLine(tex, s, 30, 30, 98, 98, 5, new Color(1f, 0.35f, 0.35f, 1f));
            }
            return ToSprite(tex);
        }

        static Sprite DrawGear()
        {
            const int s = 128;
            var tex = NewTex(s);
            FillEllipse(tex, s, 64, 64, 28, 28, Color.white);
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI * 2f / 8f;
                int cx = 64 + Mathf.RoundToInt(Mathf.Cos(a) * 38);
                int cy = 64 + Mathf.RoundToInt(Mathf.Sin(a) * 38);
                FillRect(tex, s, cx - 8, cy - 10, cx + 8, cy + 10, Color.white);
            }
            FillEllipse(tex, s, 64, 64, 14, 14, new Color(0.22f, 0.38f, 0.72f, 1f));
            return ToSprite(tex);
        }

        static Texture2D NewTex(int s)
        {
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            var clear = new Color(0, 0, 0, 0);
            var px = new Color[s * s];
            for (int i = 0; i < px.Length; i++) px[i] = clear;
            tex.SetPixels(px);
            return tex;
        }

        static Sprite ToSprite(Texture2D tex)
        {
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }

        static void FillRect(Texture2D tex, int s, int x0, int y0, int x1, int y1, Color c)
        {
            x0 = Mathf.Clamp(x0, 0, s - 1); x1 = Mathf.Clamp(x1, 0, s - 1);
            y0 = Mathf.Clamp(y0, 0, s - 1); y1 = Mathf.Clamp(y1, 0, s - 1);
            if (x0 > x1) (x0, x1) = (x1, x0);
            if (y0 > y1) (y0, y1) = (y1, y0);
            for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
                tex.SetPixel(x, s - 1 - y, c);
        }

        static void FillEllipse(Texture2D tex, int s, int cx, int cy, int rx, int ry, Color c)
        {
            for (int y = cy - ry; y <= cy + ry; y++)
            for (int x = cx - rx; x <= cx + rx; x++)
            {
                if (x < 0 || y < 0 || x >= s || y >= s) continue;
                float dx = (x - cx) / (float)rx;
                float dy = (y - cy) / (float)ry;
                if (dx * dx + dy * dy <= 1f)
                    tex.SetPixel(x, s - 1 - y, c);
            }
        }

        static void FillRoundRect(Texture2D tex, int s, int x0, int y0, int x1, int y1, int r, Color c)
        {
            FillRect(tex, s, x0 + r, y0, x1 - r, y1, c);
            FillRect(tex, s, x0, y0 + r, x1, y1 - r, c);
            FillEllipse(tex, s, x0 + r, y0 + r, r, r, c);
            FillEllipse(tex, s, x1 - r, y0 + r, r, r, c);
            FillEllipse(tex, s, x0 + r, y1 - r, r, r, c);
            FillEllipse(tex, s, x1 - r, y1 - r, r, r, c);
        }

        static void FillTri(Texture2D tex, int s, int x0, int y0, int x1, int y1, int x2, int y2, Color c)
        {
            int minX = Mathf.Min(x0, Mathf.Min(x1, x2));
            int maxX = Mathf.Max(x0, Mathf.Max(x1, x2));
            int minY = Mathf.Min(y0, Mathf.Min(y1, y2));
            int maxY = Mathf.Max(y0, Mathf.Max(y1, y2));
            for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
            {
                if (x < 0 || y < 0 || x >= s || y >= s) continue;
                if (PointInTri(x, y, x0, y0, x1, y1, x2, y2))
                    tex.SetPixel(x, s - 1 - y, c);
            }
        }

        static bool PointInTri(int px, int py, int x0, int y0, int x1, int y1, int x2, int y2)
        {
            float d1 = Sign(px, py, x0, y0, x1, y1);
            float d2 = Sign(px, py, x1, y1, x2, y2);
            float d3 = Sign(px, py, x2, y2, x0, y0);
            bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
            return !(hasNeg && hasPos);
        }

        static float Sign(int px, int py, int x0, int y0, int x1, int y1)
            => (px - x1) * (y0 - y1) - (x0 - x1) * (py - y1);

        static void StrokeLine(Texture2D tex, int s, int x0, int y0, int x1, int y1, int thick, Color c)
        {
            int steps = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(new Vector2(x0, y0), new Vector2(x1, y1))));
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                int x = Mathf.RoundToInt(Mathf.Lerp(x0, x1, t));
                int y = Mathf.RoundToInt(Mathf.Lerp(y0, y1, t));
                FillEllipse(tex, s, x, y, thick, thick, c);
            }
        }

        static void StrokeArc(Texture2D tex, int s, int cx, int cy, int radius, float a0, float a1, int thick, Color c)
        {
            float start = Mathf.Min(a0, a1);
            float end = Mathf.Max(a0, a1);
            for (float a = start; a <= end; a += 2f)
            {
                float rad = a * Mathf.Deg2Rad;
                int x = cx + Mathf.RoundToInt(Mathf.Cos(rad) * radius);
                int y = cy + Mathf.RoundToInt(Mathf.Sin(rad) * radius);
                FillEllipse(tex, s, x, y, thick, thick, c);
            }
        }
    }
}
