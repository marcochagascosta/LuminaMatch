using System.Collections.Generic;

namespace LuminaMatch.Match3
{
    /// <summary>
    /// Pure logic for finding matches of 3+ in a rectangular grid.
    /// </summary>
    public static class MatchFinder
    {
        public static HashSet<(int x, int y)> FindMatches(Cell[,] grid)
        {
            int w = grid.GetLength(0);
            int h = grid.GetLength(1);
            var matched = new HashSet<(int x, int y)>();

            for (int y = 0; y < h; y++)
            {
                int run = 1;
                for (int x = 1; x <= w; x++)
                {
                    bool same = x < w
                        && !grid[x, y].IsHole
                        && !grid[x - 1, y].IsHole
                        && grid[x, y].Color != GemColor.None
                        && grid[x, y].Color == grid[x - 1, y].Color
                        && grid[x, y].Blocker != BlockerType.Box
                        && grid[x - 1, y].Blocker != BlockerType.Box;

                    if (same)
                    {
                        run++;
                    }
                    else
                    {
                        if (run >= 3)
                        {
                            for (int i = 0; i < run; i++)
                                matched.Add((x - 1 - i, y));
                        }
                        run = 1;
                    }
                }
            }

            for (int x = 0; x < w; x++)
            {
                int run = 1;
                for (int y = 1; y <= h; y++)
                {
                    bool same = y < h
                        && !grid[x, y].IsHole
                        && !grid[x, y - 1].IsHole
                        && grid[x, y].Color != GemColor.None
                        && grid[x, y].Color == grid[x, y - 1].Color
                        && grid[x, y].Blocker != BlockerType.Box
                        && grid[x, y - 1].Blocker != BlockerType.Box;

                    if (same)
                    {
                        run++;
                    }
                    else
                    {
                        if (run >= 3)
                        {
                            for (int i = 0; i < run; i++)
                                matched.Add((x, y - 1 - i));
                        }
                        run = 1;
                    }
                }
            }

            return matched;
        }

        public static bool WouldCreateMatch(Cell[,] grid, int x1, int y1, int x2, int y2)
        {
            Swap(grid, x1, y1, x2, y2);
            bool ok = FindMatches(grid).Count > 0;
            Swap(grid, x1, y1, x2, y2);
            return ok;
        }

        public static void Swap(Cell[,] grid, int x1, int y1, int x2, int y2)
        {
            (grid[x1, y1], grid[x2, y2]) = (grid[x2, y2], grid[x1, y1]);
        }
    }
}
