namespace LuminaMatch.Match3
{
    public static class LevelCatalog
    {
        public const int TotalLevels = 60;

        public static LevelDefinition Get(int levelId)
        {
            levelId = System.Math.Clamp(levelId, 1, TotalLevels);
            if (TryGetOverride(levelId, out var over))
                return over;

            int seed = 1000 + levelId * 97;
            int moves = 22 + (levelId % 5);
            int colors = levelId < 10 ? 4 : levelId < 30 ? 5 : 6;
            int ice = levelId < 5 ? 0 : System.Math.Min(12, 2 + levelId / 5);
            int box = levelId < 15 ? 0 : System.Math.Min(8, (levelId - 14) / 4);

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
                Objectives = BuildObjectives(levelId),
                CoinReward = 40 + levelId * 2,
                Title = $"Luz {levelId}"
            };
        }

        public static bool TryGetOverride(int levelId, out LevelDefinition def)
        {
            def = null;
            switch (levelId)
            {
                case 1:
                    def = Easy(1, moves: 35, colors: 4, collect: 8, title: "Primeiros passos");
                    return true;
                case 2:
                    def = Easy(2, moves: 30, colors: 4, collect: 10, title: "Objetivo claro");
                    return true;
                case 3:
                    def = Easy(3, moves: 28, colors: 4, collect: 12, title: "Luz no palácio");
                    return true;
                case 4:
                    def = Easy(4, moves: 26, colors: 4, collect: 14, title: "Luz 4");
                    return true;
                case 5:
                    def = Easy(5, moves: 26, colors: 4, collect: 15, title: "Luz 5", score: 500);
                    return true;
                case 6:
                case 9:
                case 12:
                case 15:
                case 18:
                    def = Easy(levelId, moves: 24, colors: 5, collect: 14 + levelId / 2, title: $"Marco {levelId}");
                    return true;
                default:
                    if (levelId <= 10)
                    {
                        def = Easy(levelId, moves: 25, colors: 4, collect: 12 + levelId, title: $"Luz {levelId}");
                        return true;
                    }
                    return false;
            }
        }

        static LevelDefinition Easy(int id, int moves, int colors, int collect, string title, int score = 0)
        {
            var objectives = new System.Collections.Generic.List<LevelObjective>
            {
                new LevelObjective
                {
                    Type = ObjectiveType.CollectColor,
                    Color = (GemColor)(1 + (id % 5)),
                    Amount = collect
                }
            };
            if (score > 0)
            {
                objectives.Add(new LevelObjective
                {
                    Type = ObjectiveType.Score,
                    Amount = score
                });
            }

            return new LevelDefinition
            {
                LevelId = id,
                Width = 8,
                Height = 8,
                Moves = moves,
                ColorCount = colors,
                Seed = 2000 + id * 13,
                IceChance = id < 8 ? 0 : 4,
                BoxChance = 0,
                Objectives = objectives.ToArray(),
                CoinReward = 40 + id * 2,
                Title = title
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
