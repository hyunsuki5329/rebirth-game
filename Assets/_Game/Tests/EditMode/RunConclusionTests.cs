using NUnit.Framework;
using Roguelite.Core;
namespace Roguelite.Tests
{
    public class RunConclusionTests
    {
        [Test]
        public void SimultaneousDeathWinsAndResultCannotBeAppliedTwice()
        {
            var result = new RunConclusion(); Assert.That(result.Resolve(false, false), Is.False);
            Assert.That(result.Resolve(true, true), Is.True); Assert.That(result.Outcome, Is.EqualTo(RunOutcome.Death));
            Assert.That(result.Resolve(false, true), Is.False); Assert.That(result.Outcome, Is.EqualTo(RunOutcome.Death));
            var victory = new RunConclusion(); Assert.That(victory.Resolve(false, true), Is.True);
            Assert.That(victory.Outcome, Is.EqualTo(RunOutcome.Victory)); Assert.That(victory.Resolve(true, true), Is.False);
        }
    }
}
