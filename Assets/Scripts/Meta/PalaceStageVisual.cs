using LuminaMatch.Economy;
using UnityEngine;

namespace LuminaMatch.Meta
{
    public static class PalaceStageVisual
    {
        /// <summary>Maps 0..20 pieces to 0..2 keyframe art.</summary>
        public static int KeyframeIndex(int piecesUnlocked)
        {
            int p = Mathf.Clamp(piecesUnlocked, 0, CastleProgress.TotalPieces);
            if (p <= 0) return 0;
            if (p < 10) return 1;
            return 2;
        }

        public static string ResourcesPath(int piecesUnlocked)
            => $"Art/Palace/palace_stage_{KeyframeIndex(piecesUnlocked)}";

        public static Sprite LoadSprite(int piecesUnlocked)
        {
            var tex = Resources.Load<Texture2D>(ResourcesPath(piecesUnlocked));
            if (tex == null) return null;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }

        public static string StageCaption(PlayerProgress progress)
        {
            int pieces = CastleProgress.UnlockedPieces(progress);
            return KeyframeIndex(pieces) switch
            {
                0 => "Ruínas do Palácio de Luz",
                1 => "O palácio desperta",
                _ => "Palácio resplandecente"
            };
        }
    }
}
