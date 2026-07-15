namespace LuminaMatch.Match3
{
    public static class LevelCatalog
    {
        public const int TotalLevels = 60;

        public static LevelDefinition Get(int levelId)
        {
            levelId = System.Math.Clamp(levelId, 1, TotalLevels);
            int seed = 1000 + levelId * 97;
            int moves = 22 + (levelId % 5);
            int colors = levelId < 10 ? 4 : levelId < 30 ? 5 : 6;
            int ice = levelId < 5 ? 0 : System.Math.Min(12, 2 + levelId / 5);
            int box = levelId < 15 ? 0 : System.Math.Min(8, (levelId - 14) / 4);

            var objectives = BuildObjectives(levelId);
            return new LevelDefinition
            {
                LevelId = levelId,
                Width = 8,
                Height = 8,
                Moves = moves,
                ColorCount = colors,
                Seed = seed,
                IceChance = ice,
                BoxChance = box,
                Objectives = objectives,
                CoinReward = 40 + levelId * 2,
                Title = $"Luz {levelId}"
            };
        }

        static LevelObjective[] BuildObjectives(int levelId)
        {
            var list = new System.Collections.Generic.List<LevelObjective>();
            int collect = 12 + levelId;
            var color = (GemColor)(1 + (levelId % 5));
            list.Add(new LevelObjective
            {
                Type = ObjectiveType.CollectColor,
                Color = color,
                Amount = collect
            });

            if (levelId >= 5 && levelId % 3 == 0)
            {
                list.Add(new LevelObjective
                {
                    Type = ObjectiveType.Score,
                    Amount = 400 + levelId * 40
                });
            }

            if (levelId >= 8 && levelId % 4 == 0)
            {
                list.Add(new LevelObjective
                {
                    Type = ObjectiveType.ClearBlockers,
                    Amount = 3 + levelId / 10
                });
            }

            return list.ToArray();
        }
    }
}
