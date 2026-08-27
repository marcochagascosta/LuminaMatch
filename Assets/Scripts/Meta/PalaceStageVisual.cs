using LuminaMatch.Economy;
using UnityEngine;

namespace LuminaMatch.Meta
{
    public static class PalaceStageVisual
    {
        /// <summary>
        /// Maps 0..20 pieces onto 3 keyframe arts with clearer mid-progress bands.
        /// 0 ruins · 1–6 awakens · 7–13 rising · 14–20 uses stage_2 (resplendent).
        /// </summary>
        public static int KeyframeIndex(int piecesUnlocked)
        {
            int p = Mathf.Clamp(piecesUnlocked, 0, CastleProgress.TotalPieces);
            if (p <= 0) return 0;
            if (p <= 6) return 1;
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
            if (pieces <= 0) return "Ruínas do Palácio de Luz";
            if (pieces <= 6) return "O palácio desperta";
            if (pieces <= 13) return "Torres de luz se erguem";
            return "Palácio resplandecente";
        }
    }
}
