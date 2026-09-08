using Roguelite.Core;
namespace Roguelite
{
    public sealed class RewardService
    {
        public float GoldMultiplier { get; set; } = 1;
        public bool GrantRoom(RunState run, int node, int baseGold) =>
            run.Award(node.ToString(System.Globalization.CultureInfo.InvariantCulture), RewardCalculator.Gold(baseGold, GoldMultiplier));
    }
}
