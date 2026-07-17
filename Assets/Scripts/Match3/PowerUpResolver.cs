using System.Collections.Generic;

namespace LuminaMatch.Match3
{
    public static class PowerUpResolver
    {
        public static HashSet<(int x, int y)> ExpandActivation(Cell[,] grid, int x, int y)
        {
            var result = new HashSet<(int x, int y)>();
            if (grid == null) return result;
            int w = grid.GetLength(0);
            int h = grid.GetLength(1);
            if (x < 0 || y < 0 || x >= w || y >= h) return result;

            var power = grid[x, y].Power;
            if (power == BoardPowerType.None)
            {
                result.Add((x, y));
                return result;
            }

            switch (power)
            {
                case BoardPowerType.Rocket:
                    if (((x + y) & 1) == 0)
                    {
                        for (int i = 0; i < w; i++)
                            if (!grid[i, y].IsHole) result.Add((i, y));
                    }
                    else
                    {
                        for (int j = 0; j < h; j++)
                            if (!grid[x, j].IsHole) result.Add((x, j));
                    }
                    break;

                case BoardPowerType.Bomb:
                    for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int nx = x + dx, ny = y + dy;
                        if (nx >= 0 && ny >= 0 && nx < w && ny < h && !grid[nx, ny].IsHole)
                            result.Add((nx, ny));
                    }
                    break;

                case BoardPowerType.ColorDisk:
                {
                    var color = grid[x, y].Color;
                    if (color == GemColor.None)
                    {
                        result.Add((x, y));
                        break;
                    }
                    for (int ix = 0; ix < w; ix++)
                    for (int iy = 0; iy < h; iy++)
                    {
                        if (!grid[ix, iy].IsHole && grid[ix, iy].Color == color)
                            result.Add((ix, iy));
                    }
                    break;
                }
            }

            return result;
        }
    }
}
