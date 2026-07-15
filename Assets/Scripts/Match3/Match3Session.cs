using System.Collections.Generic;

namespace LuminaMatch.Match3
{
    public class Match3Session
    {
        public BoardModel Board { get; }
        public LevelDefinition Level { get; }
        public int MovesLeft { get; private set; }
        public int Score { get; private set; }
        public bool IsWon { get; private set; }
        public bool IsLost => !IsWon && MovesLeft <= 0 && !ObjectivesComplete();

        readonly Dictionary<GemColor, int> _collected = new();
        int _blockersCleared;
        int _blockersTarget;

        public Match3Session(LevelDefinition level)
        {
            Level = level;
            MovesLeft = level.Moves;
            Board = new BoardModel(level.Width, level.Height, level.ColorCount, level.Seed);
            Board.ApplyIceAndBoxes(level.IceChance, level.BoxChance);
            foreach (var o in level.Objectives)
            {
                if (o.Type == ObjectiveType.ClearBlockers)
                    _blockersTarget += o.Amount;
            }
        }

        public IReadOnlyDictionary<GemColor, int> Collected => _collected;
        public int BlockersCleared => _blockersCleared;

        public bool TrySwap(int x1, int y1, int x2, int y2)
        {
            if (IsWon || MovesLeft <= 0) return false;
            if (!Board.TrySwap(x1, y1, x2, y2)) return false;

            MovesLeft--;
            Cascade();
            CheckWin();
            return true;
        }

        public void Cascade()
        {
            int guard = 0;
            while (guard++ < 50)
            {
                var matches = Board.FindMatches();
                if (matches.Count == 0) break;
                ApplyResolve(Board.ResolveMatches(matches));
            }
        }

        void ApplyResolve(ResolveResult r)
        {
            Score += r.Score;
            _blockersCleared += r.BlockersCleared;
            foreach (var kv in r.Collected)
            {
                if (!_collected.ContainsKey(kv.Key)) _collected[kv.Key] = 0;
                _collected[kv.Key] += kv.Value;
            }
        }

        public bool TryHammer(int x, int y)
        {
            if (IsWon) return false;
            if (!Board.UseHammer(x, y, out var r)) return false;
            ApplyResolve(r);
            Cascade();
            CheckWin();
            return true;
        }

        public bool TryLineBlast(int y)
        {
            if (IsWon) return false;
            if (!Board.UseLineBlast(y, out var r)) return false;
            ApplyResolve(r);
            Cascade();
            CheckWin();
            return true;
        }

        public void AddExtraMoves(int n)
        {
            MovesLeft += n;
        }

        /// <summary>Forced adjacent swap (booster) that does not require a match.</summary>
        public bool ForceSwap(int x1, int y1, int x2, int y2)
        {
            if (IsWon) return false;
            if (System.Math.Abs(x1 - x2) + System.Math.Abs(y1 - y2) != 1) return false;
            if (!Board.Grid[x1, y1].CanSwap || !Board.Grid[x2, y2].CanSwap) return false;
            MatchFinder.Swap(Board.Grid, x1, y1, x2, y2);
            Cascade();
            CheckWin();
            return true;
        }

        public void EvaluateWin() => CheckWin();

        public int ObjectiveRemaining(LevelObjective o)
        {
            return o.Type switch
            {
                ObjectiveType.Score => System.Math.Max(0, o.Amount - Score),
                ObjectiveType.ClearBlockers => System.Math.Max(0, o.Amount - _blockersCleared),
                ObjectiveType.CollectColor => System.Math.Max(0, o.Amount - (_collected.TryGetValue(o.Color, out var c) ? c : 0)),
                _ => 0
            };
        }

        public bool ObjectivesComplete()
        {
            if (Level.Objectives == null || Level.Objectives.Length == 0)
                return Score >= 500;
            foreach (var o in Level.Objectives)
            {
                if (ObjectiveRemaining(o) > 0) return false;
            }
            return true;
        }

        void CheckWin()
        {
            if (ObjectivesComplete())
                IsWon = true;
        }
    }
}
