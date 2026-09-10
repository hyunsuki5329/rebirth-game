using System;
namespace Roguelite.Core
{
    public enum RunFlowPhase { MainMenu, Hub, Dungeon, Result }
    public sealed class RunResult
    {
        public string RunId { get; }
        public RunOutcome Outcome { get; }
        public int EarnedGold { get; }
        public int RetainedGold { get; }
        public float Retention { get; }
        public RunResult(string id, RunOutcome outcome, int earned, int retained, float retention)
        { RunId = id; Outcome = outcome; EarnedGold = earned; RetainedGold = retained; Retention = retention; }
    }
    public sealed class RunFlow
    {
        public ProfileData Profile { get; }
        public RunFlowPhase Phase { get; private set; }
        public RunResult Result { get; private set; }
        public string ActiveRunId { get; private set; }
        private readonly float victoryRetention, deathRetention;
        public RunFlow(ProfileData profile, float victoryRetention = 1, float deathRetention = 0.5f)
        { Profile = profile ?? throw new ArgumentNullException(nameof(profile)); this.victoryRetention = victoryRetention; this.deathRetention = deathRetention; }
        public bool EnterHub()
        {
            if (Phase != RunFlowPhase.MainMenu && Phase != RunFlowPhase.Result) return false;
            Phase = RunFlowPhase.Hub; return true;
        }
        public bool Begin(RunState run)
        {
            if (Phase != RunFlowPhase.Hub || run == null) return false;
            ActiveRunId = run.Id; Result = null; Phase = RunFlowPhase.Dungeon; return true;
        }
        public bool Finish(RunState run, RunOutcome outcome)
        {
            if (run == null || Phase != RunFlowPhase.Dungeon || run.Id != ActiveRunId || outcome == RunOutcome.InProgress || Profile.lastSettledRunId == run.Id) return false;
            float retention = outcome == RunOutcome.Victory ? victoryRetention : deathRetention;
            int retained = RewardCalculator.Gold(run.Gold, retention);
            int bank = checked(Profile.bankGold + retained);
            int clears = checked(Profile.totalClears + (outcome == RunOutcome.Victory ? 1 : 0));
            Result = new RunResult(run.Id, outcome, run.Gold, retained, retention);
            Profile.bankGold = bank; Profile.totalClears = clears; Profile.lastSettledRunId = run.Id;
            Phase = RunFlowPhase.Result; return true;
        }
    }
}
