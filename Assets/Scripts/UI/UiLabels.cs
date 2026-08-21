using LuminaMatch.Economy;
using LuminaMatch.Match3;

namespace LuminaMatch.UI
{
    /// <summary>User-facing pt-BR labels (no i18n system yet).</summary>
    public static class UiLabels
    {
        public static UnityEngine.Color GemTint(GemColor color) => color switch
        {
            GemColor.Crystal => new UnityEngine.Color(0.95f, 0.85f, 0.45f, 1f),
            GemColor.Amber => new UnityEngine.Color(1f, 0.7f, 0.25f, 1f),
            GemColor.Sapphire => new UnityEngine.Color(0.35f, 0.55f, 1f, 1f),
            GemColor.Emerald => new UnityEngine.Color(0.35f, 0.9f, 0.5f, 1f),
            GemColor.Ruby => new UnityEngine.Color(1f, 0.35f, 0.4f, 1f),
            GemColor.Amethyst => new UnityEngine.Color(0.75f, 0.45f, 1f, 1f),
            _ => UnityEngine.Color.white
        };

        public static string Gem(GemColor color) => color switch
        {
            GemColor.Crystal => "Topázio",
            GemColor.Amber => "Âmbar",
            GemColor.Sapphire => "Safira",
            GemColor.Emerald => "Esmeralda",
            GemColor.Ruby => "Rubi",
            GemColor.Amethyst => "Ametista",
            _ => "Gema"
        };

        public static string Booster(BoosterType type) => type switch
        {
            BoosterType.Hammer => "Martelo",
            BoosterType.Swap => "Troca",
            BoosterType.LineBlast => "Linha",
            _ => "Booster"
        };
    }
}
