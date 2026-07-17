using LuminaMatch.Match3;
using UnityEngine;

namespace LuminaMatch.UI
{
    /// <summary>
    /// Loads match-3 art from Resources at runtime (no ScriptableObject wiring required).
    /// </summary>
    public static class ArtCatalog
    {
        static bool _loaded;
        static Sprite _crystal, _amber, _sapphire, _emerald, _ruby, _amethyst;
        static Sprite _cell, _frame, _bg, _ice, _box;
        static Sprite _rocket, _bomb, _colorDisk;
        static Sprite _fallback;

        public static Sprite Cell => Ensure()._cell;
        public static Sprite Frame => Ensure()._frame;
        public static Sprite Background => Ensure()._bg;
        public static Sprite Ice => Ensure()._ice;
        public static Sprite Box => Ensure()._box;
        public static Sprite Rocket => Ensure()._rocket;
        public static Sprite Bomb => Ensure()._bomb;
        public static Sprite ColorDisk => Ensure()._colorDisk;

        public static Sprite Gem(GemColor c)
        {
            Ensure();
            return c switch
            {
                GemColor.Crystal => _crystal,
                GemColor.Amber => _amber,
                GemColor.Sapphire => _sapphire,
                GemColor.Emerald => _emerald,
                GemColor.Ruby => _ruby,
                GemColor.Amethyst => _amethyst,
                _ => _fallback
            };
        }

        public static Sprite Power(BoardPowerType p) => p switch
        {
            BoardPowerType.Rocket => Rocket,
            BoardPowerType.Bomb => Bomb,
            BoardPowerType.ColorDisk => ColorDisk,
            _ => null
        };

        static ArtCatalog Ensure()
        {
            if (_loaded) return null;
            _fallback = WhiteSprite();
            _crystal = LoadSprite("Art/Gems/gem_crystal") ?? _fallback;
            _amber = LoadSprite("Art/Gems/gem_amber") ?? _fallback;
            _sapphire = LoadSprite("Art/Gems/gem_sapphire") ?? _fallback;
            _emerald = LoadSprite("Art/Gems/gem_emerald") ?? _fallback;
            _ruby = LoadSprite("Art/Gems/gem_ruby") ?? _fallback;
            _amethyst = LoadSprite("Art/Gems/gem_amethyst") ?? _fallback;
            _cell = LoadSprite("Art/Board/cell") ?? _fallback;
            _frame = LoadSprite("Art/Board/board_frame") ?? _fallback;
            _bg = LoadSprite("Art/Board/bg_night") ?? _fallback;
            _ice = LoadSprite("Art/Blockers/ice") ?? _fallback;
            _box = LoadSprite("Art/Blockers/box") ?? _fallback;
            _rocket = LoadSprite("Art/Vfx/rocket") ?? _fallback;
            _bomb = LoadSprite("Art/Vfx/bomb") ?? _fallback;
            _colorDisk = LoadSprite("Art/Vfx/color_disk") ?? _fallback;
            _loaded = true;
            return null;
        }

        static Sprite LoadSprite(string resourcesPath)
        {
            var tex = Resources.Load<Texture2D>(resourcesPath);
            if (tex == null) return null;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }

        static Sprite WhiteSprite()
        {
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 1f);
        }
    }
}
