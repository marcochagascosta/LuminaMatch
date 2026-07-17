namespace LuminaMatch.UI
{
    /// <summary>FTUE copy for levels 1–3 (pt-BR).</summary>
    public static class TutorialDirector
    {
        public static string GetHintForLevel(int levelId, int tutorialStep)
        {
            if (tutorialStep >= 3)
                return null;

            return levelId switch
            {
                1 => "Troque duas gemas para fazer 3 iguais.",
                2 => "Complete o objetivo no topo.",
                3 => "Vitórias restauram o Palácio de Luz.",
                _ => null
            };
        }
    }
}
