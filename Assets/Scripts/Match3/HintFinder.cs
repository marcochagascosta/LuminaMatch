using System.Collections.Generic;
using LuminaMatch.Match3;

namespace LuminaMatch.Match3
{
    /// <summary>Finds one valid adjacent swap that creates a match (for hint button).</summary>
    public static class HintFinder
    {
        public static bool TryFindHint(Cell[,] grid, out int x1, out int y1, out int x2, out int y2)
        {
            x1 = y1 = x2 = y2 = 0;
            int w = grid.GetLength(0);
            int h = grid.GetLength(1);
            var deltas = new (int dx, int dy)[] { (1, 0), (0, 1) };

            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                if (!grid[x, y].CanSwap) continue;
                foreach (var (dx, dy) in deltas)
                {
                    int nx = x + dx, ny = y + dy;
                    if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                    if (!grid[nx, ny].CanSwap) continue;
                    if (MatchFinder.WouldCreateMatch(grid, x, y, nx, ny))
                    {
                        x1 = x; y1 = y; x2 = nx; y2 = ny;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
