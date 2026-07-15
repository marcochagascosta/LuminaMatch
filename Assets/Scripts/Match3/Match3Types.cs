namespace LuminaMatch.Match3
{
    public enum GemColor
    {
        None = 0,
        Crystal = 1,
        Amber = 2,
        Sapphire = 3,
        Emerald = 4,
        Ruby = 5,
        Amethyst = 6
    }

    public enum BlockerType
    {
        None = 0,
        Ice = 1,
        Box = 2
    }

    public struct Cell
    {
        public GemColor Color;
        public BlockerType Blocker;
        public bool IsHole;

        public bool HasGem => !IsHole && Color != GemColor.None && Blocker != BlockerType.Box;
        public bool CanSwap => !IsHole && Blocker == BlockerType.None && Color != GemColor.None;
    }

    public enum ObjectiveType
    {
        CollectColor,
        Score,
        ClearBlockers
    }

    [System.Serializable]
    public class LevelObjective
    {
        public ObjectiveType Type;
        public GemColor Color;
        public int Amount;
    }

    [System.Serializable]
    public class LevelDefinition
    {
        public int LevelId;
        public int Width = 8;
        public int Height = 8;
        public int Moves = 25;
        public int ColorCount = 5;
        public int Seed;
        public int IceChance;
        public int BoxChance;
        public LevelObjective[] Objectives;
        public int CoinReward = 50;
        public string Title;
    }
}
