namespace Roguelite.Core
{
    public enum RunOutcome { InProgress, Victory, Death }
    public sealed class RunConclusion
    {
        public RunOutcome Outcome { get; private set; }
        public bool Resolve(bool playerDead, bool bossDead)
        {
            if (Outcome != RunOutcome.InProgress || (!playerDead && !bossDead)) return false;
            Outcome = playerDead ? RunOutcome.Death : RunOutcome.Victory; return true;
        }
    }
}
