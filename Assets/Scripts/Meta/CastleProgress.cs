using LuminaMatch.Economy;
using LuminaMatch.Match3;

namespace LuminaMatch.Meta
{
    public static class CastleProgress
    {
        public const int TotalPieces = 20;

        public static int UnlockedPieces(PlayerProgress progress)
            => System.Math.Clamp(progress.Data.CastlePieces, 0, TotalPieces);

        public static float Completion01(PlayerProgress progress)
            => UnlockedPieces(progress) / (float)TotalPieces;

        public static string StatusText(PlayerProgress progress)
        {
            int pieces = UnlockedPieces(progress);
            return pieces >= TotalPieces
                ? "Palácio de Luz restaurado!"
                : $"Palácio de Luz: {pieces}/{TotalPieces} peças";
        }

        public static string PieceName(int index)
        {
            string[] names =
            {
                "Portão de Cristal", "Torre Norte", "Torre Sul", "Jardim Âmbar",
                "Salão Safira", "Fonte Esmeralda", "Altar Rubi", "Cúpula Ametista",
                "Pontes de Luz", "Biblioteca", "Observatório", "Capela",
                "Muralha Leste", "Muralha Oeste", "Trono Lúmina", "Espelhos",
                "Jardim Noturno", "Torre do Relógio", "Baluarte", "Coroa do Palácio"
            };
            if (index < 0 || index >= names.Length) return $"Peça {index + 1}";
            return names[index];
        }
    }

    public static class LevelMap
    {
        public static bool IsUnlocked(PlayerProgress progress, int levelId)
            => levelId >= 1 && levelId <= LevelCatalog.TotalLevels
               && levelId <= progress.Data.HighestUnlockedLevel;
    }
}
