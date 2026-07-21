using LuminaMatch.Economy;
using LuminaMatch.Match3;

namespace LuminaMatch.UI
{
    /// <summary>User-facing pt-BR labels (no i18n system yet).</summary>
    public static class UiLabels
    {
        public static string Gem(GemColor color) => color switch
        {
            GemColor.Crystal => "Cristal",
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
