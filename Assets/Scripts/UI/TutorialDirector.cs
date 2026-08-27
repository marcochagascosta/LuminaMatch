using LuminaMatch.Economy;

namespace LuminaMatch.UI
{
    public static class TutorialDirector
    {
        public const int TutorialCompleteStep = 7;

        public static string HomeHint(PlayerProgress p)
        {
            if (p?.Data == null) return "";
            if (p.Data.TutorialStep >= TutorialCompleteStep) return "";
            return p.Data.TutorialStep switch
            {
                0 => "Toque em Jogar: combine 3 gemas iguais!",
                1 => "Complete o objetivo no topo da tela.",
                2 => "Vitórias restauram o Palácio de Luz.",
                3 => "Combine 4 em linha → foguete!",
                4 => "Combine 5 → bomba de área!",
                5 => "Forma em L ou T → disco de cor!",
                6 => "Troque dois poderes para um combo poderoso!",
                _ => ""
            };
        }

        public static string GameplayHint(int levelId, int tutorialStep)
        {
            // Level-gated teaching for early content (works even if tutorial step skipped).
            if (levelId == 1)
                return "Arraste uma gema para a vizinha e faça 3 iguais. Depois elas caem e novas entram no topo.";
            if (levelId == 2)
                return "Olhe o objetivo no topo — colete o que pedir.";
            if (levelId == 3)
                return "Cada vitória ajuda a restaurar o palácio.";
            if (levelId == 4)
                return "Combine 4 gemas em linha para criar um foguete. Troque-o para limpar a linha!";
            if (levelId == 5)
                return "Combine 5 em linha para uma bomba — explode a área ao redor.";
            if (levelId == 6)
                return "Forme um L ou T de 5 gemas para o disco de cor — limpa toda aquela cor!";
            if (levelId == 7)
                return "Troque dois poderes (foguete, bomba ou disco) para um combo especial!";
            if (levelId == 8)
                return "Gelo protege gemas: combine ao lado ou sob o gelo para quebrá-lo.";
            if (levelId == 16)
                return "Quebre gelo com matches ao lado — ou com foguete/bomba na linha!";
            if (levelId == 17)
                return "Caixas bloqueiam a gema. Combine ao lado ou acerte com um poder!";
            if (levelId == 18)
                return "Limpe bloqueios e colete gemas — use combos de poderes!";
            if (levelId == 21)
                return "Caixas voltam com força — priorize obstáculos no objetivo!";
            if (levelId == 30)
                return "Marco 30: 6 cores e muitos obstáculos. Combos salvam a partida!";
            if (levelId == 31)
                return "Ametista entrou no tabuleiro — 6 cores! Planeje bem cada jogada.";
            if (levelId == 35)
                return "Marco 35: pressão alta. Use foguete+bomba e disco com sabedoria.";
            if (levelId == 40)
                return "Marco 40: quase no fim do arco. Combos e boosters valem ouro!";
            if (levelId == 45)
                return "Marco 45: tabuleiro denso. Limpe obstáculos cedo.";
            if (levelId == 50)
                return "Marco 50: metade final. Guarde o disco de cor para o momento certo.";
            if (levelId == 55)
                return "Marco 55: pouquíssimos movimentos — cada troca conta.";
            if (levelId == 60)
                return "Último nível! Restaure a Coroa do Palácio de Luz.";
            if (tutorialStep < TutorialCompleteStep && levelId <= 3)
                return "";
            return "";
        }

        public static void OnLevelCompleted(PlayerProgress p, int levelId, bool won)
        {
            if (p?.Data == null || !won) return;
            // Advance tutorial with early levels (1→7 maps to steps).
            int target = levelId switch
            {
                1 => 1,
                2 => 2,
                3 => 3,
                4 => 4,
                5 => 5,
                6 => 6,
                >= 7 => TutorialCompleteStep,
                _ => p.Data.TutorialStep
            };
            if (target > p.Data.TutorialStep)
            {
                p.Data.TutorialStep = target;
                p.Save();
            }
        }
    }
}
