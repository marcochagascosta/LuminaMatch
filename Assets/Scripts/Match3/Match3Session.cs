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
        public bool TimedOut { get; private set; }
        public float TimeLeft { get; private set; }
        public int TimeLimitSeconds { get; }
        public bool IsLost => !IsWon && (TimedOut || (MovesLeft <= 0 && !ObjectivesComplete()));

        readonly Dictionary<GemColor, int> _collected = new();
        int _blockersCleared;
        int _blockersTarget;
        readonly List<RocketFxEvent> _pendingRocketFx = new();
        readonly List<BombFxEvent> _pendingBombFx = new();
        readonly List<ColorDiskFxEvent> _pendingColorDiskFx = new();

        public Match3Session(LevelDefinition level)
        {
            Level = level;
            MovesLeft = level.Moves;
            TimeLimitSeconds = level.ResolvedTimeLimitSeconds;
            TimeLeft = TimeLimitSeconds;
            Board = new BoardModel(level.Width, level.Height, level.ColorCount, level.Seed);
            Board.ApplyIceAndBoxes(level.IceChance, level.BoxChance);
            foreach (var o in level.Objectives)
            {
                if (o.Type == ObjectiveType.ClearBlockers)
                    _blockersTarget += o.Amount;
                if (o.Type == ObjectiveType.CollectColor)
                    Board.EnsureMinColorPresence(o.Color, System.Math.Max(8, o.Amount / 2));
            }
        }

        public IReadOnlyDictionary<GemColor, int> Collected => _collected;
        public int BlockersCleared => _blockersCleared;

        public void TickTime(float deltaSeconds)
        {
            if (IsWon || TimedOut || deltaSeconds <= 0f) return;
            TimeLeft -= deltaSeconds;
            if (TimeLeft <= 0f)
            {
                TimeLeft = 0f;
                TimedOut = true;
            }
        }

        public static string FormatTime(float seconds)
        {
            int s = System.Math.Max(0, (int)System.Math.Ceiling(seconds));
            return $"{s / 60}:{s % 60:00}";
        }

        /// <summary>Rocket blasts from the last move (cleared after UI consumes them).</summary>
        public List<RocketFxEvent> ConsumeRocketFx()
        {
            var copy = new List<RocketFxEvent>(_pendingRocketFx);
            _pendingRocketFx.Clear();
            return copy;
        }

        public List<BombFxEvent> ConsumeBombFx()
        {
            var copy = new List<BombFxEvent>(_pendingBombFx);
            _pendingBombFx.Clear();
            return copy;
        }

        public List<ColorDiskFxEvent> ConsumeColorDiskFx()
        {
            var copy = new List<ColorDiskFxEvent>(_pendingColorDiskFx);
            _pendingColorDiskFx.Clear();
            return copy;
        }

        public void ClearRocketFx()
        {
            _pendingRocketFx.Clear();
            _pendingBombFx.Clear();
            _pendingColorDiskFx.Clear();
        }

        void ClearPendingFx()
        {
            _pendingRocketFx.Clear();
            _pendingBombFx.Clear();
            _pendingColorDiskFx.Clear();
        }

        public bool TrySwap(int x1, int y1, int x2, int y2)
        {
            if (IsWon || MovesLeft <= 0) return false;
            ClearPendingFx();

            bool powerPlay = Board.Grid[x1, y1].HasPower || Board.Grid[x2, y2].HasPower;
            if (powerPlay)
            {
                if (System.Math.Abs(x1 - x2) + System.Math.Abs(y1 - y2) != 1) return false;
                if (Board.Grid[x1, y1].IsHole || Board.Grid[x2, y2].IsHole) return false;
                if (Board.Grid[x1, y1].Blocker != BlockerType.None || Board.Grid[x2, y2].Blocker != BlockerType.None)
                    return false;

                MatchFinder.Swap(Board.Grid, x1, y1, x2, y2);
                MovesLeft--;
                var clear = PowerUpResolver.ExpandCombo(Board.Grid, x1, y1, x2, y2);
                if (clear == null)
                {
                    clear = new HashSet<(int x, int y)>();
                    if (Board.Grid[x1, y1].HasPower)
                        foreach (var c in PowerUpResolver.ExpandActivation(Board.Grid, x1, y1))
                            clear.Add(c);
                    if (Board.Grid[x2, y2].HasPower)
                        foreach (var c in PowerUpResolver.ExpandActivation(Board.Grid, x2, y2))
                            clear.Add(c);
                }
                foreach (var m in Board.FindMatches())
                    clear.Add(m);
                // Power blasts must not be re-classified as match shapes (was causing fake wins).
                ApplyResolve(Board.ResolveMatches(clear, allowPowerSpawns: false));
                Cascade();
                CheckWin();
                return true;
            }

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
            if (r.RocketEvents != null && r.RocketEvents.Count > 0)
                _pendingRocketFx.AddRange(r.RocketEvents);
            if (r.BombEvents != null && r.BombEvents.Count > 0)
                _pendingBombFx.AddRange(r.BombEvents);
            if (r.ColorDiskEvents != null && r.ColorDiskEvents.Count > 0)
                _pendingColorDiskFx.AddRange(r.ColorDiskEvents);
        }

        public bool TryHammer(int x, int y)
        {
            if (IsWon) return false;
            ClearPendingFx();
            if (!Board.UseHammer(x, y, out var r)) return false;
            ApplyResolve(r);
            Cascade();
            CheckWin();
            return true;
        }

        public bool TryLineBlast(int y)
        {
            if (IsWon) return false;
            ClearPendingFx();
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

        /// <summary>Resume after timeout continue: clear flag and grant extra clock.</summary>
        public void RecoverFromTimeout(float extraSeconds = 60f)
        {
            TimedOut = false;
            if (extraSeconds > 0f)
                TimeLeft += extraSeconds;
        }

        /// <summary>Forced adjacent swap (booster) that does not require a match.</summary>
        public bool ForceSwap(int x1, int y1, int x2, int y2)
        {
            if (IsWon) return false;
            ClearPendingFx();
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
