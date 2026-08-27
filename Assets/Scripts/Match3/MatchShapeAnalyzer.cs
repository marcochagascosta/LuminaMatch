using System.Collections.Generic;

namespace LuminaMatch.Match3
{
    public struct PowerSpawn
    {
        public int X;
        public int Y;
        public BoardPowerType Type;
        public GemColor Color;
    }

    /// <summary>
    /// Classifies a matched set into rocket (4), bomb (5), or color disk (L/T).
    /// Priority: bomb &gt; color disk &gt; rocket.
    /// </summary>
    public static class MatchShapeAnalyzer
    {
        public static List<PowerSpawn> Analyze(Cell[,] grid, HashSet<(int x, int y)> matched)
        {
            var spawns = new List<PowerSpawn>();
            if (matched == null || matched.Count < 4)
                return spawns;

            int w = grid.GetLength(0);
            int h = grid.GetLength(1);

            // Group matched cells by color
            var byColor = new Dictionary<GemColor, List<(int x, int y)>>();
            foreach (var (x, y) in matched)
            {
                var c = grid[x, y].Color;
                if (c == GemColor.None) continue;
                if (!byColor.TryGetValue(c, out var list))
                {
                    list = new List<(int x, int y)>();
                    byColor[c] = list;
                }
                list.Add((x, y));
            }

            foreach (var kv in byColor)
            {
                var cells = kv.Value;
                if (cells.Count < 4) continue;

                bool isL = IsLOrT(cells);
                int maxRun = MaxStraightRun(cells);

                BoardPowerType type;
                if (maxRun >= 5)
                    type = BoardPowerType.Bomb;
                else if (isL && cells.Count >= 5)
                    type = BoardPowerType.ColorDisk;
                else if (maxRun >= 4)
                    type = BoardPowerType.Rocket;
                else if (isL)
                    type = BoardPowerType.ColorDisk;
                else
                    continue;

                var center = cells[cells.Count / 2];
                spawns.Add(new PowerSpawn
                {
                    X = center.x,
                    Y = center.y,
                    Type = type,
                    Color = kv.Key
                });
            }

            return spawns;
        }

        static int MaxStraightRun(List<(int x, int y)> cells)
        {
            int best = 1;
            var set = new HashSet<(int x, int y)>(cells);
            foreach (var (x, y) in cells)
            {
                // horizontal run through this cell
                int lx = x;
                while (set.Contains((lx - 1, y))) lx--;
                int rx = x;
                while (set.Contains((rx + 1, y))) rx++;
                best = System.Math.Max(best, rx - lx + 1);

                int by = y;
                while (set.Contains((x, by - 1))) by--;
                int ty = y;
                while (set.Contains((x, ty + 1))) ty++;
                best = System.Math.Max(best, ty - by + 1);
            }
            return best;
        }

        static bool IsLOrT(List<(int x, int y)> cells)
        {
            if (cells.Count < 5) return false;
            var set = new HashSet<(int x, int y)>(cells);
            bool hasHoriz3 = false, hasVert3 = false;
            foreach (var (x, y) in cells)
            {
                int hRun = 1 + (set.Contains((x - 1, y)) ? 1 : 0) + (set.Contains((x + 1, y)) ? 1 : 0);
                // better: count full runs
                int lx = x; while (set.Contains((lx - 1, y))) lx--;
                int rx = x; while (set.Contains((rx + 1, y))) rx++;
                if (rx - lx + 1 >= 3) hasHoriz3 = true;
                int by = y; while (set.Contains((x, by - 1))) by--;
                int ty = y; while (set.Contains((x, ty + 1))) ty++;
                if (ty - by + 1 >= 3) hasVert3 = true;
            }
            return hasHoriz3 && hasVert3;
        }
    }
}
