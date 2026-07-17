using System.Text;
using LuminaMatch.Economy;
using LuminaMatch.Match3;
using UnityEngine;

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
                    ObjectiveType.CollectColor => $"• Coletar {o.Amount} gemas {o.Color}",
                    ObjectiveType.Score => $"• Alcançar {o.Amount} pontos",
                    ObjectiveType.ClearBlockers => $"• Limpar {o.Amount} bloqueios",
                    _ => $"• {o.Type} {o.Amount}"
                });
            }
            sb.Append($"Movimentos: {level.Moves}");
            return sb.ToString();
        }

        public static string BoosterSummary(PlayerProgress p)
            => $"Boosters — H:{p.Data.Hammers}  S:{p.Data.Swaps}  L:{p.Data.LineBlasts}";
    }
}
