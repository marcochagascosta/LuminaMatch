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
                Objectives = BuildObjectives(levelId, colors),
                CoinReward = 55 + levelId * 3,
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
                    def = Easy(4, moves: 28, colors: 4, collect: 12, title: "Foguete!", ice: 0);
                    return true;
                case 5:
                    def = Easy(5, moves: 26, colors: 4, collect: 14, title: "Bomba!", score: 400, ice: 0);
                    return true;
                case 6:
                    def = Easy(6, moves: 26, colors: 4, collect: 15, title: "Disco de cor", ice: 0);
                    return true;
                case 7:
                    def = Easy(7, moves: 25, colors: 4, collect: 16, title: "Combo de poderes", ice: 0);
                    return true;
                case 8:
                    def = Easy(8, moves: 25, colors: 4, collect: 18, title: "Gelo chega", ice: 4);
                    return true;
                case 9:
                    def = Easy(9, moves: 24, colors: 5, collect: 18, title: "Marco 9", ice: 4);
                    return true;
                case 10:
                    def = Easy(10, moves: 24, colors: 5, collect: 20, title: "Luz 10", ice: 4);
                    return true;
                case 11:
                    def = Easy(11, moves: 24, colors: 5, collect: 20, title: "Luz 11", ice: 5);
                    return true;
                case 12:
                    def = Easy(12, moves: 24, colors: 5, collect: 21, title: "Marco 12", ice: 5, score: 550);
                    return true;
                case 13:
                    def = Easy(13, moves: 23, colors: 5, collect: 22, title: "Luz 13", ice: 6);
                    return true;
                case 14:
                    def = Easy(14, moves: 23, colors: 5, collect: 23, title: "Luz 14", ice: 6, score: 600);
                    return true;
                case 15:
                    def = Easy(15, moves: 22, colors: 5, collect: 24, title: "Marco 15", ice: 7, score: 700);
                    return true;
                case 16:
                    def = Easy(16, moves: 24, colors: 5, collect: 20, title: "Mais gelo", ice: 8, clearBlockers: 4);
                    return true;
                case 17:
                    def = Easy(17, moves: 24, colors: 5, collect: 21, title: "Caixas!", ice: 4, box: 6, clearBlockers: 3);
                    return true;
                case 18:
                    def = Easy(18, moves: 22, colors: 5, collect: 22, title: "Marco 18", ice: 6, box: 5, clearBlockers: 4, score: 650);
                    return true;
                case 19:
                    def = Easy(19, moves: 22, colors: 5, collect: 23, title: "Luz 19", ice: 7, box: 6, clearBlockers: 5);
                    return true;
                case 20:
                    def = Easy(20, moves: 22, colors: 5, collect: 24, title: "Marco 20", ice: 8, box: 7, clearBlockers: 6, score: 800);
                    return true;
                case 21:
                    def = Easy(21, moves: 23, colors: 5, collect: 22, title: "Ritmo de caixas", ice: 6, box: 5, clearBlockers: 4);
                    return true;
                case 22:
                    def = Easy(22, moves: 23, colors: 5, collect: 23, title: "Gelo e madeira", ice: 7, box: 6, clearBlockers: 5);
                    return true;
                case 23:
                    def = Easy(23, moves: 22, colors: 5, collect: 24, title: "Pressão 23", ice: 7, box: 6, clearBlockers: 5, score: 750);
                    return true;
                case 24:
                    def = Easy(24, moves: 21, colors: 5, collect: 26, title: "Marco 24", ice: 9, box: 8, clearBlockers: 6, score: 900);
                    return true;
                case 25:
                    def = Easy(25, moves: 22, colors: 5, collect: 24, title: "Luz 25", ice: 7, box: 6, clearBlockers: 5);
                    return true;
                case 26:
                    def = Easy(26, moves: 22, colors: 5, collect: 25, title: "Luz 26", ice: 8, box: 7, clearBlockers: 6, score: 850);
                    return true;
                case 27:
                    def = Easy(27, moves: 21, colors: 5, collect: 26, title: "Luz 27", ice: 8, box: 7, clearBlockers: 6);
                    return true;
                case 28:
                    def = Easy(28, moves: 21, colors: 5, collect: 26, title: "Luz 28", ice: 9, box: 7, clearBlockers: 7, score: 950);
                    return true;
                case 29:
                    def = Easy(29, moves: 20, colors: 6, collect: 27, title: "Quase lá", ice: 9, box: 8, clearBlockers: 7);
                    return true;
                case 30:
                    def = Easy(30, moves: 20, colors: 6, collect: 28, title: "Marco 30", ice: 10, box: 8, clearBlockers: 8, score: 1100);
                    return true;
                case 31:
                    def = Easy(31, moves: 22, colors: 6, collect: 26, title: "Nova cor", ice: 8, box: 6, clearBlockers: 5);
                    return true;
                case 32:
                    def = Easy(32, moves: 21, colors: 6, collect: 27, title: "Luz 32", ice: 9, box: 7, clearBlockers: 6);
                    return true;
                case 33:
                    def = Easy(33, moves: 21, colors: 6, collect: 28, title: "Luz 33", ice: 9, box: 7, clearBlockers: 6, score: 1000);
                    return true;
                case 34:
                    def = Easy(34, moves: 20, colors: 6, collect: 28, title: "Luz 34", ice: 10, box: 8, clearBlockers: 7);
                    return true;
                case 35:
                    def = Easy(35, moves: 20, colors: 6, collect: 30, title: "Marco 35", ice: 10, box: 8, clearBlockers: 8, score: 1200);
                    return true;
                case 36:
                    def = Easy(36, moves: 20, colors: 6, collect: 30, title: "Luz 36", ice: 10, box: 8, clearBlockers: 7);
                    return true;
                case 37:
                    def = Easy(37, moves: 19, colors: 6, collect: 31, title: "Luz 37", ice: 11, box: 8, clearBlockers: 8, score: 1250);
                    return true;
                case 38:
                    def = Easy(38, moves: 19, colors: 6, collect: 32, title: "Luz 38", ice: 11, box: 9, clearBlockers: 8);
                    return true;
                case 39:
                    def = Easy(39, moves: 19, colors: 6, collect: 32, title: "Luz 39", ice: 11, box: 9, clearBlockers: 9);
                    return true;
                case 40:
                    def = Easy(40, moves: 18, colors: 6, collect: 34, title: "Marco 40", ice: 12, box: 10, clearBlockers: 10, score: 1400);
                    return true;
                case 41:
                    def = Easy(41, moves: 20, colors: 6, collect: 32, title: "Luz 41", ice: 11, box: 9, clearBlockers: 8);
                    return true;
                case 42:
                    def = Easy(42, moves: 19, colors: 6, collect: 33, title: "Luz 42", ice: 11, box: 9, clearBlockers: 8, score: 1300);
                    return true;
                case 43:
                    def = Easy(43, moves: 19, colors: 6, collect: 34, title: "Luz 43", ice: 12, box: 10, clearBlockers: 9);
                    return true;
                case 44:
                    def = Easy(44, moves: 18, colors: 6, collect: 34, title: "Luz 44", ice: 12, box: 10, clearBlockers: 9);
                    return true;
                case 45:
                    def = Easy(45, moves: 18, colors: 6, collect: 36, title: "Marco 45", ice: 12, box: 10, clearBlockers: 10, score: 1500);
                    return true;
                case 46:
                    def = Easy(46, moves: 18, colors: 6, collect: 35, title: "Luz 46", ice: 12, box: 11, clearBlockers: 10);
                    return true;
                case 47:
                    def = Easy(47, moves: 17, colors: 6, collect: 36, title: "Luz 47", ice: 13, box: 11, clearBlockers: 10, score: 1550);
                    return true;
                case 48:
                    def = Easy(48, moves: 17, colors: 6, collect: 37, title: "Luz 48", ice: 13, box: 11, clearBlockers: 11);
                    return true;
                case 49:
                    def = Easy(49, moves: 17, colors: 6, collect: 38, title: "Luz 49", ice: 13, box: 12, clearBlockers: 11);
                    return true;
                case 50:
                    def = Easy(50, moves: 16, colors: 6, collect: 40, title: "Marco 50", ice: 14, box: 12, clearBlockers: 12, score: 1700);
                    return true;
                case 51:
                    def = Easy(51, moves: 17, colors: 6, collect: 38, title: "Luz 51", ice: 13, box: 11, clearBlockers: 11);
                    return true;
                case 52:
                    def = Easy(52, moves: 16, colors: 6, collect: 39, title: "Luz 52", ice: 14, box: 12, clearBlockers: 11, score: 1750);
                    return true;
                case 53:
                    def = Easy(53, moves: 16, colors: 6, collect: 40, title: "Luz 53", ice: 14, box: 12, clearBlockers: 12);
                    return true;
                case 54:
                    def = Easy(54, moves: 16, colors: 6, collect: 40, title: "Luz 54", ice: 14, box: 12, clearBlockers: 12);
                    return true;
                case 55:
                    def = Easy(55, moves: 15, colors: 6, collect: 42, title: "Marco 55", ice: 15, box: 13, clearBlockers: 13, score: 1900);
                    return true;
                case 56:
                    def = Easy(56, moves: 15, colors: 6, collect: 41, title: "Luz 56", ice: 15, box: 13, clearBlockers: 12);
                    return true;
                case 57:
                    def = Easy(57, moves: 15, colors: 6, collect: 42, title: "Luz 57", ice: 15, box: 13, clearBlockers: 13, score: 1950);
                    return true;
                case 58:
                    def = Easy(58, moves: 14, colors: 6, collect: 43, title: "Luz 58", ice: 15, box: 14, clearBlockers: 13);
                    return true;
                case 59:
                    def = Easy(59, moves: 14, colors: 6, collect: 44, title: "Quase o fim", ice: 16, box: 14, clearBlockers: 14);
                    return true;
                case 60:
                    def = Easy(60, moves: 14, colors: 6, collect: 45, title: "Coroa do Palácio", ice: 16, box: 14, clearBlockers: 15, score: 2200);
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Objective color must be within ColorCount (1..colors).
        /// Old formula used id%5 and could ask for Ruby while only spawning 4 colors.
        /// </summary>
        public static GemColor ObjectiveColorFor(int levelId, int colorCount)
        {
            int n = System.Math.Clamp(colorCount, 3, 6);
            int idx = 1 + ((System.Math.Max(1, levelId) - 1) % n);
            return (GemColor)idx;
        }

        static LevelDefinition Easy(
            int id,
            int moves,
            int colors,
            int collect,
            string title,
            int score = 0,
            int ice = -1,
            int box = 0,
            int clearBlockers = 0)
        {
            var objectives = new System.Collections.Generic.List<LevelObjective>
            {
                new LevelObjective
                {
                    Type = ObjectiveType.CollectColor,
                    Color = ObjectiveColorFor(id, colors),
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
            if (clearBlockers > 0)
            {
                objectives.Add(new LevelObjective
                {
                    Type = ObjectiveType.ClearBlockers,
                    Amount = clearBlockers
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
                IceChance = ice >= 0 ? ice : (id < 8 ? 0 : 4),
                BoxChance = box,
                Objectives = objectives.ToArray(),
                CoinReward = 55 + id * 3,
                Title = title
            };
        }

        static LevelObjective[] BuildObjectives(int levelId, int colors)
        {
            var list = new System.Collections.Generic.List<LevelObjective>();
            int collect = 12 + levelId;
            list.Add(new LevelObjective
            {
                Type = ObjectiveType.CollectColor,
                Color = ObjectiveColorFor(levelId, colors),
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
