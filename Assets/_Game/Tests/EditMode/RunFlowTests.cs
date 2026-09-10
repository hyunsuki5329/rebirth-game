using NUnit.Framework;
using Roguelite.Core;
namespace Roguelite.Tests
{
    public class RunFlowTests
    {
        [Test]
        public void VictoryAndDeathSettleOnceAndOldEventsCannotPayNewRun()
        {
            var profile = new ProfileData(); var flow = new RunFlow(profile);
            Assert.That(flow.EnterHub(), Is.True); Assert.That(flow.EnterHub(), Is.False);
            var first = new RunState(1); first.Award("room", 51);
            Assert.That(flow.Begin(first), Is.True); Assert.That(flow.Finish(first, RunOutcome.Victory), Is.True);
            Assert.That(profile.bankGold, Is.EqualTo(51)); Assert.That(profile.totalClears, Is.EqualTo(1));
            Assert.That(flow.Finish(first, RunOutcome.Victory), Is.False);
            flow.EnterHub(); var second = new RunState(2); second.Award("room", 51); flow.Begin(second);
            Assert.That(flow.Finish(first, RunOutcome.Victory), Is.False);
            Assert.That(flow.Finish(second, RunOutcome.Death), Is.True);
            Assert.That(profile.bankGold, Is.EqualTo(76)); Assert.That(flow.Result.RetainedGold, Is.EqualTo(25));
            Assert.That(profile.totalClears, Is.EqualTo(1)); Assert.That(flow.Finish(second, RunOutcome.Death), Is.False);
        }
    }
}
