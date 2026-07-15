using LuminaMatch.Match3;
using NUnit.Framework;

namespace LuminaMatch.Tests
{
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
}
