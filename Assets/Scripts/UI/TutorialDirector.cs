using LuminaMatch.Economy;

namespace LuminaMatch.UI
{
    public static class TutorialDirector
    {
        public static string HomeHint(PlayerProgress p)
        {
            if (p?.Data == null) return "";
            if (p.Data.TutorialStep >= 3) return "";
            return p.Data.TutorialStep switch
            {
                0 => "Toque em Jogar: combine 3 gemas iguais!",
                1 => "Complete o objetivo no topo da tela.",
                2 => "Vitórias restauram o Palácio de Luz.",
                _ => ""
            };
        }

        public static string GameplayHint(int levelId, int tutorialStep)
        {
            if (tutorialStep >= 3) return "";
            if (levelId == 1) return "Troque duas gemas vizinhas para fazer 3 iguais.";
            if (levelId == 2) return "Olhe o objetivo no topo — colete o que pedir.";
            if (levelId == 3) return "Cada vitória ajuda a restaurar o palácio.";
            return "";
        }

        public static void OnLevelCompleted(PlayerProgress p, int levelId, bool won)
        {
            if (p?.Data == null || !won) return;
            if (levelId <= 3 && p.Data.TutorialStep < levelId)
            {
                p.Data.TutorialStep = levelId;
                p.Save();
            }
        }
    }
}
