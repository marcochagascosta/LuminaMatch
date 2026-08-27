using System.Text;
using LuminaMatch.Economy;
using LuminaMatch.Match3;

namespace LuminaMatch.UI.Screens
{
    public static class PreLevelCopy
    {
        public static string Title(LevelDefinition level)
            => $"Nível {level.LevelId} — {level.Title}";

        public static string Objectives(LevelDefinition level)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Objetivos:");
            foreach (var o in level.Objectives)
            {
                sb.AppendLine(o.Type switch
                {
                    ObjectiveType.CollectColor => $"• Coletar {o.Amount} gemas {UiLabels.Gem(o.Color)}",
                    ObjectiveType.Score => $"• Alcançar {o.Amount} pontos",
                    ObjectiveType.ClearBlockers => $"• Quebrar {o.Amount} caixas ou gelo",
                    _ => $"• Objetivo {o.Amount}"
                });
            }
            sb.Append($"Movimentos: {level.Moves}   ·   Tempo: {Match3Session.FormatTime(level.ResolvedTimeLimitSeconds)}");
            return sb.ToString();
        }

        public static string BoosterSummary(PlayerProgress p)
            => $"Poderes — Martelo:{p.Data.Hammers}  Troca:{p.Data.Swaps}  Linha:{p.Data.LineBlasts}";
    }
}
