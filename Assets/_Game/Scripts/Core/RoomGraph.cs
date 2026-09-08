namespace Roguelite.Core
{
    public enum RoomKind { Combat, Reward, Elite, Shortcut, Boss }
    public readonly struct RoomNode
    {
        public readonly int Id, Variant;
        public readonly RoomKind Kind;
        public RoomNode(int id, RoomKind kind, int variant) { Id = id; Kind = kind; Variant = variant; }
    }
    public sealed class RoomGraph
    {
        public RoomNode[] Nodes { get; }
        public RoomGraph(int seed)
        {
            var random = new System.Random(seed);
            var kinds = new[] { RoomKind.Combat, RoomKind.Combat, RoomKind.Reward,
                RoomKind.Shortcut, RoomKind.Combat, RoomKind.Combat, RoomKind.Boss, RoomKind.Elite };
            Nodes = new RoomNode[kinds.Length];
            for (int i = 0; i < kinds.Length; i++) Nodes[i] = new RoomNode(i, kinds[i], random.Next(2));
        }
        public int Next(int node, bool shortcut = false)
        {
            switch (node)
            {
                case 0: return 1;
                case 1: return 2;
                case 2: return 3;
                case 3: return shortcut ? 7 : 4;
                case 4: return 5;
                case 5: case 7: return 6;
                case 6: return -1;
                default: throw new System.ArgumentOutOfRangeException(nameof(node));
            }
        }
    }
}
