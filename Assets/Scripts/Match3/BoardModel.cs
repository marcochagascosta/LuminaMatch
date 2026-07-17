using System;
using System.Collections.Generic;

namespace LuminaMatch.Match3
{
    public class BoardModel
    {
        public Cell[,] Grid { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int ColorCount { get; private set; }

        readonly Random _rng;

        public BoardModel(int width, int height, int colorCount, int seed)
        {
            Width = width;
            Height = height;
            ColorCount = Math.Clamp(colorCount, 3, 6);
            _rng = new Random(seed);
            Grid = new Cell[width, height];
            FillWithoutInitialMatches();
        }

        public BoardModel(Cell[,] grid, int colorCount, int seed = 1)
        {
            Width = grid.GetLength(0);
            Height = grid.GetLength(1);
            ColorCount = colorCount;
            _rng = new Random(seed);
            Grid = grid;
        }

        void FillWithoutInitialMatches()
        {
            for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                Grid[x, y] = new Cell { Color = RandomColorAvoidingMatch(x, y) };
            }
        }

        GemColor RandomColorAvoidingMatch(int x, int y)
        {
            for (int attempt = 0; attempt < 20; attempt++)
            {
                var c = (GemColor)(_rng.Next(1, ColorCount + 1));
                bool horiz = x >= 2
                    && Grid[x - 1, y].Color == c
                    && Grid[x - 2, y].Color == c;
                bool vert = y >= 2
                    && Grid[x, y - 1].Color == c
                    && Grid[x, y - 2].Color == c;
                if (!horiz && !vert)
                    return c;
            }
            return (GemColor)(_rng.Next(1, ColorCount + 1));
        }

        public void ApplyIceAndBoxes(int iceChance, int boxChance)
        {
            for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                if (Grid[x, y].IsHole) continue;
                int roll = _rng.Next(100);
                if (roll < boxChance)
                    Grid[x, y].Blocker = BlockerType.Box;
                else if (roll < boxChance + iceChance)
                    Grid[x, y].Blocker = BlockerType.Ice;
            }
        }

        public bool TrySwap(int x1, int y1, int x2, int y2)
        {
            if (!InBounds(x1, y1) || !InBounds(x2, y2)) return false;
            if (!AreAdjacent(x1, y1, x2, y2)) return false;
            if (!Grid[x1, y1].CanSwap || !Grid[x2, y2].CanSwap) return false;
            if (!MatchFinder.WouldCreateMatch(Grid, x1, y1, x2, y2)) return false;

            MatchFinder.Swap(Grid, x1, y1, x2, y2);
            return true;
        }

        public HashSet<(int x, int y)> FindMatches() => MatchFinder.FindMatches(Grid);

        /// <summary>
        /// Clears matched gems, damages adjacent ice/box, applies gravity and refills.
        /// Returns collected gem counts by color and blockers cleared.
        /// </summary>
        public ResolveResult ResolveMatches(HashSet<(int x, int y)> matches)
        {
            var result = new ResolveResult();
            if (matches == null || matches.Count == 0)
                return result;

            var originalMatches = new HashSet<(int x, int y)>(matches);
            var toClear = new HashSet<(int x, int y)>(matches);

            // Expand any board powers included in the clear set (chain reactions).
            bool expanded;
            do
            {
                expanded = false;
                foreach (var (px, py) in new List<(int x, int y)>(toClear))
                {
                    if (!InBounds(px, py)) continue;
                    if (Grid[px, py].Power == BoardPowerType.None) continue;
                    foreach (var extra in PowerUpResolver.ExpandActivation(Grid, px, py))
                    {
                        if (toClear.Add(extra))
                            expanded = true;
                    }
                }
            } while (expanded);

            foreach (var (x, y) in originalMatches)
                DamageNeighbors(x, y, toClear, result);

            var spawns = MatchShapeAnalyzer.Analyze(Grid, originalMatches);

            foreach (var (x, y) in toClear)
            {
                if (Grid[x, y].IsHole) continue;
                if (Grid[x, y].Blocker == BlockerType.Ice)
                {
                    Grid[x, y].Blocker = BlockerType.None;
                    result.BlockersCleared++;
                    continue;
                }
                if (Grid[x, y].Blocker == BlockerType.Box)
                    continue;

                if (Grid[x, y].Color != GemColor.None)
                {
                    result.AddCollected(Grid[x, y].Color);
                    result.Score += 10;
                }
                Grid[x, y].Color = GemColor.None;
                Grid[x, y].Power = BoardPowerType.None;
            }

            foreach (var spawn in spawns)
            {
                if (!InBounds(spawn.X, spawn.Y) || Grid[spawn.X, spawn.Y].IsHole)
                    continue;
                Grid[spawn.X, spawn.Y].Color = spawn.Color;
                Grid[spawn.X, spawn.Y].Power = spawn.Type;
                Grid[spawn.X, spawn.Y].Blocker = BlockerType.None;
            }

            ApplyGravity();
            Refill();
            return result;
        }

        /// <summary>
        /// Swap that also activates if either cell holds a board power.
        /// </summary>
        public bool TrySwapOrActivatePower(int x1, int y1, int x2, int y2, out ResolveResult result)
        {
            result = new ResolveResult();
            if (!InBounds(x1, y1) || !InBounds(x2, y2)) return false;
            if (!AreAdjacent(x1, y1, x2, y2)) return false;
            if (Grid[x1, y1].IsHole || Grid[x2, y2].IsHole) return false;
            if (Grid[x1, y1].Blocker != BlockerType.None || Grid[x2, y2].Blocker != BlockerType.None)
                return false;

            bool powerPlay = Grid[x1, y1].HasPower || Grid[x2, y2].HasPower;
            if (!powerPlay)
            {
                if (!TrySwap(x1, y1, x2, y2))
                    return false;
                result = ResolveMatches(FindMatches());
                // cascade
                var more = FindMatches();
                int guard = 0;
                while (more.Count > 0 && guard++ < 20)
                {
                    var r = ResolveMatches(more);
                    result.Score += r.Score;
                    result.BlockersCleared += r.BlockersCleared;
                    foreach (var kv in r.Collected)
                        for (int i = 0; i < kv.Value; i++)
                            result.AddCollected(kv.Key);
                    more = FindMatches();
                }
                return true;
            }

            MatchFinder.Swap(Grid, x1, y1, x2, y2);
            var clear = new HashSet<(int x, int y)>();
            if (Grid[x1, y1].HasPower)
                foreach (var c in PowerUpResolver.ExpandActivation(Grid, x1, y1)) clear.Add(c);
            if (Grid[x2, y2].HasPower)
                foreach (var c in PowerUpResolver.ExpandActivation(Grid, x2, y2)) clear.Add(c);
            foreach (var m in FindMatches()) clear.Add(m);
            result = ResolveMatches(clear);
            return true;
        }

        void DamageNeighbors(int x, int y, HashSet<(int x, int y)> toClear, ResolveResult result)
        {
            TryDamage(x + 1, y, toClear, result);
            TryDamage(x - 1, y, toClear, result);
            TryDamage(x, y + 1, toClear, result);
            TryDamage(x, y - 1, toClear, result);
        }

        void TryDamage(int x, int y, HashSet<(int x, int y)> toClear, ResolveResult result)
        {
            if (!InBounds(x, y) || Grid[x, y].IsHole) return;
            if (Grid[x, y].Blocker == BlockerType.Box)
            {
                Grid[x, y].Blocker = BlockerType.None;
                result.BlockersCleared++;
                toClear.Add((x, y));
            }
        }

        public void ApplyGravity()
        {
            for (int x = 0; x < Width; x++)
            {
                int writeY = 0;
                for (int y = 0; y < Height; y++)
                {
                    if (Grid[x, y].IsHole)
                    {
                        writeY = y + 1;
                        continue;
                    }
                    if (Grid[x, y].Color != GemColor.None || Grid[x, y].Blocker == BlockerType.Box)
                    {
                        if (writeY != y)
                        {
                            Grid[x, writeY] = Grid[x, y];
                            Grid[x, y] = new Cell();
                        }
                        writeY++;
                    }
                }
            }
        }

        public void Refill()
        {
            for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                if (Grid[x, y].IsHole) continue;
                if (Grid[x, y].Color == GemColor.None && Grid[x, y].Blocker != BlockerType.Box)
                    Grid[x, y].Color = (GemColor)(_rng.Next(1, ColorCount + 1));
            }
        }

        public bool UseHammer(int x, int y, out ResolveResult result)
        {
            result = new ResolveResult();
            if (!InBounds(x, y) || Grid[x, y].IsHole) return false;
            var set = new HashSet<(int x, int y)> { (x, y) };
            if (Grid[x, y].Blocker == BlockerType.Box)
            {
                Grid[x, y].Blocker = BlockerType.None;
                result.BlockersCleared++;
            }
            else if (Grid[x, y].Blocker == BlockerType.Ice)
            {
                Grid[x, y].Blocker = BlockerType.None;
                result.BlockersCleared++;
            }
            else if (Grid[x, y].Color != GemColor.None)
            {
                result.AddCollected(Grid[x, y].Color);
                result.Score += 10;
                Grid[x, y].Color = GemColor.None;
            }
            ApplyGravity();
            Refill();
            return true;
        }

        public bool UseLineBlast(int y, out ResolveResult result)
        {
            result = new ResolveResult();
            if (y < 0 || y >= Height) return false;
            var set = new HashSet<(int x, int y)>();
            for (int x = 0; x < Width; x++)
            {
                if (!Grid[x, y].IsHole)
                    set.Add((x, y));
            }
            result = ResolveMatches(set);
            return true;
        }

        bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;

        static bool AreAdjacent(int x1, int y1, int x2, int y2)
            => Math.Abs(x1 - x2) + Math.Abs(y1 - y2) == 1;
    }

    public class ResolveResult
    {
        public int Score;
        public int BlockersCleared;
        public readonly Dictionary<GemColor, int> Collected = new();

        public void AddCollected(GemColor color)
        {
            if (!Collected.ContainsKey(color)) Collected[color] = 0;
            Collected[color]++;
        }
    }
}
