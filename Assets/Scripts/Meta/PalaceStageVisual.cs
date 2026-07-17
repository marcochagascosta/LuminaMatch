namespace LuminaMatch.Meta
{
    /// <summary>
    /// Maps castle piece progress to Meshy palace keyframes (0–4) and resource paths.
    /// </summary>
    public static class PalaceStageVisual
    {
        public const int KeyframeCount = 5;

        static readonly string[] StageLabels =
        {
            "Ruínas",
            "Portão de Cristal",
            "Torres",
            "Cúpula",
            "Coroa do Palácio"
        };

        /// <summary>Maps pieces 0–20 to keyframe index 0–4.</summary>
        public static int StageIndexFromPieces(int pieces)
        {
            pieces = System.Math.Clamp(pieces, 0, CastleProgress.TotalPieces);
            if (pieces >= CastleProgress.TotalPieces)
                return KeyframeCount - 1;
            return pieces / (CastleProgress.TotalPieces / (KeyframeCount - 1));
        }

        public static string StageLabel(int stageIndex)
        {
            if (stageIndex < 0 || stageIndex >= StageLabels.Length)
                return $"Estágio {stageIndex}";
            return StageLabels[stageIndex];
        }

        public static string StageLabelFromPieces(int pieces)
            => StageLabel(StageIndexFromPieces(pieces));

        /// <summary>Resources path pattern without extension, e.g. Art/Palace/stage_2.</summary>
        public static string ResourcePath(int stageIndex)
            => $"Art/Palace/stage_{stageIndex}";

        public static string ResourcePathFromPieces(int pieces)
            => ResourcePath(StageIndexFromPieces(pieces));
    }
}
