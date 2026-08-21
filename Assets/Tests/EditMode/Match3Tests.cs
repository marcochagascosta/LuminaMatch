using LuminaMatch.Match3;
using NUnit.Framework;

namespace LuminaMatch.Tests
{
    public class CellPowerTests
    {
        [Test]
        public void Cell_DefaultPower_IsNone()
        {
            var c = new Cell { Color = GemColor.Ruby };
            Assert.AreEqual(BoardPowerType.None, c.Power);
        }
    }

    public class MatchShapeAnalyzerTests
    {
        [Test]
        public void Analyze_HorizontalFour_SpawnsRocket()
        {
            var grid = Empty(8, 8);
            for (int x = 0; x < 4; x++) grid[x, 0].Color = GemColor.Ruby;
            var matched = MatchFinder.FindMatches(grid);
            var spawns = MatchShapeAnalyzer.Analyze(grid, matched);
            Assert.AreEqual(1, spawns.Count);
            Assert.AreEqual(BoardPowerType.Rocket, spawns[0].Type);
        }

        [Test]
        public void Analyze_HorizontalFive_SpawnsBomb()
        {
            var grid = Empty(8, 8);
            for (int x = 0; x < 5; x++) grid[x, 0].Color = GemColor.Sapphire;
            var matched = MatchFinder.FindMatches(grid);
            var spawns = MatchShapeAnalyzer.Analyze(grid, matched);
            Assert.AreEqual(1, spawns.Count);
            Assert.AreEqual(BoardPowerType.Bomb, spawns[0].Type);
        }

        [Test]
        public void Analyze_LShape_SpawnsColorDisk()
        {
            var grid = Empty(8, 8);
            grid[0, 0].Color = grid[1, 0].Color = grid[2, 0].Color = GemColor.Emerald;
            grid[0, 1].Color = grid[0, 2].Color = GemColor.Emerald;
            var matched = MatchFinder.FindMatches(grid);
            var spawns = MatchShapeAnalyzer.Analyze(grid, matched);
            Assert.IsTrue(spawns.Exists(s => s.Type == BoardPowerType.ColorDisk));
        }

        [Test]
        public void PowerUpResolver_Rocket_ClearsRowOrColumn()
        {
            var grid = Empty(5, 5);
            for (int x = 0; x < 5; x++) grid[x, 2].Color = GemColor.Ruby;
            grid[2, 2].Power = BoardPowerType.Rocket;
            var cleared = PowerUpResolver.ExpandActivation(grid, 2, 2);
            Assert.GreaterOrEqual(cleared.Count, 5);
        }

        [Test]
        public void ExpandCombo_RocketRocket_ClearsCross()
        {
            var grid = Empty(5, 5);
            for (int x = 0; x < 5; x++)
            for (int y = 0; y < 5; y++)
                grid[x, y].Color = GemColor.Amber;
            grid[1, 2].Power = BoardPowerType.Rocket;
            grid[2, 2].Power = BoardPowerType.Rocket;
            var cleared = PowerUpResolver.ExpandCombo(grid, 1, 2, 2, 2);
            Assert.IsNotNull(cleared);
            // Same-row rockets: full row + two columns = 5+4+4.
            Assert.AreEqual(13, cleared.Count);
            Assert.IsTrue(cleared.Contains((0, 2)));
            Assert.IsTrue(cleared.Contains((1, 0)));
            Assert.IsTrue(cleared.Contains((2, 4)));
        }

        [Test]
        public void ExpandCombo_ColorDiskRocket_ClearsColorAndLines()
        {
            var grid = Empty(5, 5);
            for (int x = 0; x < 5; x++)
            for (int y = 0; y < 5; y++)
                grid[x, y].Color = GemColor.Sapphire;
            grid[0, 0].Color = GemColor.Ruby;
            grid[0, 0].Power = BoardPowerType.ColorDisk;
            grid[1, 0].Power = BoardPowerType.Rocket;
            grid[1, 0].Color = GemColor.Amber;
            var cleared = PowerUpResolver.ExpandCombo(grid, 0, 0, 1, 0);
            Assert.IsNotNull(cleared);
            Assert.IsTrue(cleared.Contains((0, 0)));
            // All rubies cleared (only 0,0) plus rocket lines through ruby cells.
            Assert.GreaterOrEqual(cleared.Count, 5);
        }

        [Test]
        public void Levels_1_To_15_HaveOverrides()
        {
            for (int i = 1; i <= 15; i++)
            {
                Assert.IsTrue(LevelCatalog.TryGetOverride(i, out var def), $"level {i}");
                Assert.AreEqual(i, def.LevelId);
                Assert.Greater(def.Moves, 0);
            }
        }

        [Test]
        public void Levels_16_To_20_IntroduceBoxesConsistently()
        {
            Assert.IsTrue(LevelCatalog.TryGetOverride(16, out var l16));
            Assert.Greater(l16.IceChance, 0);
            Assert.AreEqual(0, l16.BoxChance);

            Assert.IsTrue(LevelCatalog.TryGetOverride(17, out var l17));
            Assert.Greater(l17.BoxChance, 0);
            Assert.IsTrue(System.Array.Exists(l17.Objectives, o => o.Type == ObjectiveType.ClearBlockers));

            Assert.IsTrue(LevelCatalog.TryGetOverride(20, out var l20));
            Assert.Greater(l20.BoxChance, 0);
        }

        [Test]
        public void ExpandCombo_BombBomb_ClearsLargeArea()
        {
            var grid = Empty(7, 7);
            for (int x = 0; x < 7; x++)
            for (int y = 0; y < 7; y++)
                grid[x, y].Color = GemColor.Ruby;
            grid[3, 3].Power = BoardPowerType.Bomb;
            grid[3, 4].Power = BoardPowerType.Bomb;
            var cleared = PowerUpResolver.ExpandCombo(grid, 3, 3, 3, 4);
            Assert.IsNotNull(cleared);
            Assert.GreaterOrEqual(cleared.Count, 25);
        }

        [Test]
        public void ExpandCombo_DiskDisk_ClearsWholeBoard()
        {
            var grid = Empty(4, 4);
            for (int x = 0; x < 4; x++)
            for (int y = 0; y < 4; y++)
                grid[x, y].Color = GemColor.Amber;
            grid[0, 0].Power = BoardPowerType.ColorDisk;
            grid[1, 0].Power = BoardPowerType.ColorDisk;
            var cleared = PowerUpResolver.ExpandCombo(grid, 0, 0, 1, 0);
            Assert.AreEqual(16, cleared.Count);
        }

        [Test]
        public void Levels_21_To_30_HaveOverrides()
        {
            for (int i = 21; i <= 30; i++)
            {
                Assert.IsTrue(LevelCatalog.TryGetOverride(i, out var def), $"level {i}");
                Assert.Greater(def.BoxChance, 0, $"level {i} boxes");
            }
        }

        [Test]
        public void Levels_31_To_40_HaveOverrides()
        {
            for (int i = 31; i <= 40; i++)
            {
                Assert.IsTrue(LevelCatalog.TryGetOverride(i, out var def), $"level {i}");
                Assert.AreEqual(6, def.ColorCount, $"level {i} colors");
                Assert.Greater(def.BoxChance, 0, $"level {i} boxes");
            }
        }

        [Test]
        public void Levels_41_To_60_HaveOverrides()
        {
            for (int i = 41; i <= 60; i++)
            {
                Assert.IsTrue(LevelCatalog.TryGetOverride(i, out var def), $"level {i}");
                Assert.AreEqual(i, def.LevelId);
                Assert.AreEqual(6, def.ColorCount, $"level {i}");
                Assert.Greater(def.Moves, 0);
            }
            var finale = LevelCatalog.Get(60);
            Assert.AreEqual("Coroa do Palácio", finale.Title);
        }

        [Test]
        public void AdjacentMatch_CracksIceWithoutClearingGem()
        {
            var board = new BoardModel(5, 5, 5, 2);
            board.Grid[0, 0].Color = GemColor.Ruby;
            board.Grid[1, 0].Color = GemColor.Ruby;
            board.Grid[2, 0].Color = GemColor.Ruby;
            board.Grid[2, 1].Color = GemColor.Emerald;
            board.Grid[2, 1].Blocker = BlockerType.Ice;
            var matches = board.FindMatches();
            Assert.GreaterOrEqual(matches.Count, 3);
            var result = board.ResolveMatches(matches);
            Assert.GreaterOrEqual(result.BlockersCleared, 1);
            // Ice cracked; emerald survives (may fall with gravity).
            bool emeraldAlive = false;
            for (int y = 0; y < 5; y++)
                if (board.Grid[2, y].Color == GemColor.Emerald)
                    emeraldAlive = true;
            Assert.IsTrue(emeraldAlive);
        }

        [Test]
        public void ResolveMatches_RocketBreaksBoxOnLine()
        {
            var board = new BoardModel(5, 5, 5, 9);
            for (int x = 0; x < 5; x++)
            {
                board.Grid[x, 2].Color = GemColor.Sapphire;
                board.Grid[x, 2].Blocker = BlockerType.None;
                board.Grid[x, 2].Power = BoardPowerType.None;
            }
            board.Grid[4, 2].Color = GemColor.None;
            board.Grid[4, 2].Blocker = BlockerType.Box;
            board.Grid[0, 2].Power = BoardPowerType.Rocket;
            // Force horizontal rocket: parity (0+2) even → row
            var clear = PowerUpResolver.ExpandActivation(board.Grid, 0, 2);
            Assert.IsTrue(clear.Contains((4, 2)));
            var result = board.ResolveMatches(clear, allowPowerSpawns: false);
            Assert.AreEqual(BlockerType.None, board.Grid[4, 2].Blocker);
            Assert.GreaterOrEqual(result.BlockersCleared, 1);
        }

        [Test]
        public void Level1_Override_IsEasy()
        {
            var level = LevelCatalog.Get(1);
            Assert.AreEqual(35, level.Moves);
            Assert.AreEqual(4, level.ColorCount);
            Assert.AreEqual(0, level.IceChance);
        }

        [Test]
        public void HintFinder_FindsValidSwap()
        {
            var grid = Empty(5, 5);
            // Two rubies and third that becomes match after swap
            grid[0, 0].Color = GemColor.Ruby;
            grid[1, 0].Color = GemColor.Ruby;
            grid[2, 1].Color = GemColor.Ruby;
            grid[2, 0].Color = GemColor.Emerald;
            Assert.IsTrue(HintFinder.TryFindHint(grid, out int x1, out int y1, out int x2, out int y2));
            Assert.IsTrue(MatchFinder.WouldCreateMatch(grid, x1, y1, x2, y2));
        }

        static Cell[,] Empty(int w, int h)
        {
            var g = new Cell[w, h];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                g[x, y] = new Cell { Color = GemColor.None };
            return g;
        }
    }

    public class MatchFinderTests
    {
        [Test]
        public void FindMatches_HorizontalThree_Detected()
        {
            var grid = Empty(5, 5);
            grid[1, 2].Color = GemColor.Ruby;
            grid[2, 2].Color = GemColor.Ruby;
            grid[3, 2].Color = GemColor.Ruby;
            var matches = MatchFinder.FindMatches(grid);
            Assert.AreEqual(3, matches.Count);
            Assert.IsTrue(matches.Contains((1, 2)));
            Assert.IsTrue(matches.Contains((2, 2)));
            Assert.IsTrue(matches.Contains((3, 2)));
        }

        [Test]
        public void FindMatches_VerticalThree_Detected()
        {
            var grid = Empty(4, 4);
            grid[0, 0].Color = GemColor.Amber;
            grid[0, 1].Color = GemColor.Amber;
            grid[0, 2].Color = GemColor.Amber;
            Assert.AreEqual(3, MatchFinder.FindMatches(grid).Count);
        }

        [Test]
        public void FindMatches_TwoOnly_NotMatched()
        {
            var grid = Empty(4, 4);
            grid[0, 0].Color = GemColor.Sapphire;
            grid[1, 0].Color = GemColor.Sapphire;
            Assert.AreEqual(0, MatchFinder.FindMatches(grid).Count);
        }

        [Test]
        public void WouldCreateMatch_ValidSwap_ReturnsTrue()
        {
            var grid = Empty(5, 5);
            // Two rubies horizontal, third nearby after swap
            grid[0, 0].Color = GemColor.Ruby;
            grid[1, 0].Color = GemColor.Ruby;
            grid[2, 1].Color = GemColor.Ruby;
            grid[2, 0].Color = GemColor.Emerald;
            Assert.IsTrue(MatchFinder.WouldCreateMatch(grid, 2, 0, 2, 1));
        }

        static Cell[,] Empty(int w, int h)
        {
            var g = new Cell[w, h];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                g[x, y] = new Cell { Color = GemColor.None };
            return g;
        }
    }

    public class BoardModelTests
    {
        [Test]
        public void ApplyGravity_GemsFallDown()
        {
            var grid = new Cell[3, 3];
            for (int y = 0; y < 3; y++)
            for (int x = 0; x < 3; x++)
                grid[x, y] = new Cell { Color = GemColor.None };

            grid[0, 2].Color = GemColor.Crystal;
            var board = new BoardModel(grid, 5, 1);
            board.ApplyGravity();
            Assert.AreEqual(GemColor.Crystal, board.Grid[0, 0].Color);
            Assert.AreEqual(GemColor.None, board.Grid[0, 2].Color);
        }

        [Test]
        public void NewBoard_HasNoInitialMatches()
        {
            var board = new BoardModel(8, 8, 5, 42);
            Assert.AreEqual(0, board.FindMatches().Count);
        }

        [Test]
        public void ResolveMatches_IncreasesScore()
        {
            var grid = new Cell[3, 3];
            for (int y = 0; y < 3; y++)
            for (int x = 0; x < 3; x++)
                grid[x, y] = new Cell { Color = GemColor.Emerald };

            // Fill so only bottom row is ruby match... simpler: set full board emerald then resolve 3
            var board = new BoardModel(8, 8, 5, 7);
            // Force a match manually
            board.Grid[0, 0].Color = GemColor.Ruby;
            board.Grid[1, 0].Color = GemColor.Ruby;
            board.Grid[2, 0].Color = GemColor.Ruby;
            var matches = board.FindMatches();
            Assert.GreaterOrEqual(matches.Count, 3);
            var result = board.ResolveMatches(matches);
            Assert.Greater(result.Score, 0);
        }

        [Test]
        public void Refill_DoesNotRecolorOccupiedCells()
        {
            var grid = new Cell[3, 3];
            for (int y = 0; y < 3; y++)
            for (int x = 0; x < 3; x++)
                grid[x, y] = new Cell { Color = GemColor.Sapphire };

            grid[1, 1].Color = GemColor.None;
            var board = new BoardModel(grid, 5, 11);
            board.Refill();
            Assert.AreEqual(GemColor.Sapphire, board.Grid[0, 0].Color);
            Assert.AreEqual(GemColor.Sapphire, board.Grid[2, 2].Color);
            Assert.AreNotEqual(GemColor.None, board.Grid[1, 1].Color);
        }

        [Test]
        public void ResolveMatches_PowerBlast_DoesNotSpawnExtraWeapons()
        {
            var board = new BoardModel(8, 8, 5, 3);
            // Paint a full row of ruby + a color disk
            for (int x = 0; x < 8; x++)
            {
                board.Grid[x, 0].Color = GemColor.Ruby;
                board.Grid[x, 0].Power = BoardPowerType.None;
            }
            board.Grid[3, 0].Power = BoardPowerType.ColorDisk;
            var clear = PowerUpResolver.ExpandActivation(board.Grid, 3, 0);
            Assert.GreaterOrEqual(clear.Count, 8);
            board.ResolveMatches(clear, allowPowerSpawns: false);
            int powers = 0;
            for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
                if (board.Grid[x, y].HasPower) powers++;
            Assert.AreEqual(0, powers);
        }
    }

    public class LevelCatalogTests
    {
        [Test]
        public void Get_ReturnsSixtyUniqueSeeds()
        {
            var seeds = new System.Collections.Generic.HashSet<int>();
            for (int i = 1; i <= LevelCatalog.TotalLevels; i++)
            {
                var level = LevelCatalog.Get(i);
                Assert.AreEqual(i, level.LevelId);
                Assert.IsNotNull(level.Objectives);
                Assert.Greater(level.Objectives.Length, 0);
                seeds.Add(level.Seed);
            }
            Assert.AreEqual(LevelCatalog.TotalLevels, seeds.Count);
        }

        [Test]
        public void CollectObjectives_OnlyUseSpawnableColors()
        {
            for (int i = 1; i <= LevelCatalog.TotalLevels; i++)
            {
                var level = LevelCatalog.Get(i);
                foreach (var o in level.Objectives)
                {
                    if (o.Type != ObjectiveType.CollectColor) continue;
                    Assert.GreaterOrEqual((int)o.Color, 1, $"level {i}");
                    Assert.LessOrEqual((int)o.Color, level.ColorCount, $"level {i} asked for {o.Color} with ColorCount={level.ColorCount}");
                }
            }
        }

        [Test]
        public void Level4_DoesNotAskForRubyWithFourColors()
        {
            var level = LevelCatalog.Get(4);
            Assert.AreEqual(4, level.ColorCount);
            var collect = System.Array.Find(level.Objectives, o => o.Type == ObjectiveType.CollectColor);
            Assert.IsNotNull(collect);
            Assert.AreNotEqual(GemColor.Ruby, collect.Color);
            Assert.LessOrEqual((int)collect.Color, 4);
        }
    }

    public class BoardEnsureColorTests
    {
        [Test]
        public void EnsureMinColorPresence_AddsMissingObjectiveColor()
        {
            var board = new BoardModel(8, 8, 4, 99);
            // Paint everything crystal
            for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
                board.Grid[x, y].Color = GemColor.Crystal;

            board.EnsureMinColorPresence(GemColor.Emerald, 10);
            int count = 0;
            for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
                if (board.Grid[x, y].Color == GemColor.Emerald) count++;
            Assert.GreaterOrEqual(count, 10);
        }
    }

    public class Match3SessionTests
    {
        [Test]
        public void Session_StartsWithConfiguredMoves()
        {
            var level = LevelCatalog.Get(1);
            var session = new Match3Session(level);
            Assert.AreEqual(level.Moves, session.MovesLeft);
            Assert.IsFalse(session.IsWon);
        }
    }

    public class SoftShopTests
    {
        [Test]
        public void SoftShop_BuyLife_SpendsCoins()
        {
            var p = new LuminaMatch.Economy.PlayerProgress();
            p.Data.Coins = 1000;
            p.Data.Lives = 2;
            p.Data.MaxLives = 5;
            Assert.IsTrue(LuminaMatch.Economy.SoftShop.TryBuyLife(p));
            Assert.AreEqual(3, p.Data.Lives);
            Assert.AreEqual(1000 - LuminaMatch.Economy.SoftShop.LifeCost, p.Data.Coins);
        }

        [Test]
        public void SoftShop_BuyLife_FailsWhenBroke()
        {
            var p = new LuminaMatch.Economy.PlayerProgress();
            p.Data.Coins = 10;
            p.Data.Lives = 1;
            Assert.IsFalse(LuminaMatch.Economy.SoftShop.TryBuyLife(p));
            Assert.AreEqual(1, p.Data.Lives);
        }
    }
}
