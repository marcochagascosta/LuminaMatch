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
                    ExpandRocket(grid, x, y, result);
                    break;

                case BoardPowerType.Bomb:
                    ExpandBomb(grid, x, y, result, radius: 1);
                    break;

                case BoardPowerType.ColorDisk:
                    ExpandColorDisk(grid, x, y, result);
                    break;
            }

            return result;
        }

        /// <summary>
        /// Special clear set when two board powers are swapped together.
        /// Returns null if either cell has no power (caller uses normal activation).
        /// </summary>
        public static HashSet<(int x, int y)> ExpandCombo(Cell[,] grid, int x1, int y1, int x2, int y2)
        {
            if (grid == null) return null;
            var a = grid[x1, y1].Power;
            var b = grid[x2, y2].Power;
            if (a == BoardPowerType.None || b == BoardPowerType.None)
                return null;

            var result = new HashSet<(int x, int y)>();
            int w = grid.GetLength(0);
            int h = grid.GetLength(1);

            // Order types for stable pairing (higher enum = "stronger" for naming).
            BoardPowerType t1 = a, t2 = b;
            int ox1 = x1, oy1 = y1, ox2 = x2, oy2 = y2;
            if ((int)t1 > (int)t2)
            {
                (t1, t2) = (t2, t1);
                (ox1, oy1, ox2, oy2) = (ox2, oy2, ox1, oy1);
            }

            if (t1 == BoardPowerType.Rocket && t2 == BoardPowerType.Rocket)
            {
                // Double cross: row+col at both origins.
                ExpandCross(grid, ox1, oy1, result);
                ExpandCross(grid, ox2, oy2, result);
            }
            else if (t1 == BoardPowerType.Rocket && t2 == BoardPowerType.Bomb)
            {
                // Wide stripe: 3 rows + 3 cols through the midpoint.
                int cx = (ox1 + ox2) / 2;
                int cy = (oy1 + oy2) / 2;
                for (int d = -1; d <= 1; d++)
                {
                    ExpandRow(grid, cy + d, result);
                    ExpandCol(grid, cx + d, result);
                }
            }
            else if (t1 == BoardPowerType.Bomb && t2 == BoardPowerType.Bomb)
            {
                ExpandBomb(grid, ox1, oy1, result, radius: 2);
                ExpandBomb(grid, ox2, oy2, result, radius: 2);
            }
            else if (t1 == BoardPowerType.ColorDisk && t2 == BoardPowerType.ColorDisk)
            {
                // Wipe the board.
                for (int ix = 0; ix < w; ix++)
                for (int iy = 0; iy < h; iy++)
                    if (!grid[ix, iy].IsHole)
                        result.Add((ix, iy));
            }
            else if (t1 == BoardPowerType.Rocket && t2 == BoardPowerType.ColorDisk)
            {
                // Color → rockets: clear color, then row/col through each of that color.
                var color = grid[ox2, oy2].Color;
                var colored = CollectColorCells(grid, color);
                foreach (var c in colored)
                {
                    result.Add(c);
                    ExpandRocket(grid, c.x, c.y, result);
                }
                result.Add((ox1, oy1));
                result.Add((ox2, oy2));
            }
            else if (t1 == BoardPowerType.Bomb && t2 == BoardPowerType.ColorDisk)
            {
                // Color → bombs: clear color + 3×3 around each.
                var color = grid[ox2, oy2].Color;
                var colored = CollectColorCells(grid, color);
                foreach (var c in colored)
                {
                    result.Add(c);
                    ExpandBomb(grid, c.x, c.y, result, radius: 1);
                }
                result.Add((ox1, oy1));
                result.Add((ox2, oy2));
            }
            else
            {
                // Fallback: union of both activations.
                foreach (var c in ExpandActivation(grid, x1, y1)) result.Add(c);
                foreach (var c in ExpandActivation(grid, x2, y2)) result.Add(c);
            }

            return result;
        }

        static List<(int x, int y)> CollectColorCells(Cell[,] grid, GemColor color)
        {
            var list = new List<(int x, int y)>();
            if (color == GemColor.None) return list;
            int w = grid.GetLength(0);
            int h = grid.GetLength(1);
            for (int ix = 0; ix < w; ix++)
            for (int iy = 0; iy < h; iy++)
            {
                if (!grid[ix, iy].IsHole && grid[ix, iy].Color == color)
                    list.Add((ix, iy));
            }
            return list;
        }

        static void ExpandRocket(Cell[,] grid, int x, int y, HashSet<(int x, int y)> result)
        {
            if (((x + y) & 1) == 0)
                ExpandRow(grid, y, result);
            else
                ExpandCol(grid, x, result);
        }

        static void ExpandCross(Cell[,] grid, int x, int y, HashSet<(int x, int y)> result)
        {
            ExpandRow(grid, y, result);
            ExpandCol(grid, x, result);
        }

        static void ExpandRow(Cell[,] grid, int y, HashSet<(int x, int y)> result)
        {
            int w = grid.GetLength(0);
            int h = grid.GetLength(1);
            if (y < 0 || y >= h) return;
            for (int i = 0; i < w; i++)
                if (!grid[i, y].IsHole) result.Add((i, y));
        }

        static void ExpandCol(Cell[,] grid, int x, HashSet<(int x, int y)> result)
        {
            int w = grid.GetLength(0);
            int h = grid.GetLength(1);
            if (x < 0 || x >= w) return;
            for (int j = 0; j < h; j++)
                if (!grid[x, j].IsHole) result.Add((x, j));
        }

        static void ExpandBomb(Cell[,] grid, int x, int y, HashSet<(int x, int y)> result, int radius)
        {
            int w = grid.GetLength(0);
            int h = grid.GetLength(1);
            for (int dx = -radius; dx <= radius; dx++)
            for (int dy = -radius; dy <= radius; dy++)
            {
                int nx = x + dx, ny = y + dy;
                if (nx >= 0 && ny >= 0 && nx < w && ny < h && !grid[nx, ny].IsHole)
                    result.Add((nx, ny));
            }
        }

        static void ExpandColorDisk(Cell[,] grid, int x, int y, HashSet<(int x, int y)> result)
        {
            var color = grid[x, y].Color;
            if (color == GemColor.None)
            {
                result.Add((x, y));
                return;
            }
            int w = grid.GetLength(0);
            int h = grid.GetLength(1);
            for (int ix = 0; ix < w; ix++)
            for (int iy = 0; iy < h; iy++)
            {
                if (!grid[ix, iy].IsHole && grid[ix, iy].Color == color)
                    result.Add((ix, iy));
            }
        }
    }
}
