using NUnit.Framework;
using Roguelite.Core;
namespace Roguelite.Tests
{
    public class PauseContractTests
    {
        [Test] public void SessionExposesOwnedPauseController()
        {
            Assert.That(typeof(GameBootstrap).GetProperty("Pause"), Is.Not.Null, "M12 requires an owned pause controller.");
        }
        [Test] public void MenuTransitionCannotBypassAnActiveRunOrDiscardProfile()
        {
            var profile = new ProfileData { bankGold = 75 }; var flow = new RunFlow(profile);
            Assert.That(flow.EnterMenu(), Is.False); Assert.That(flow.EnterHub(), Is.True);
            Assert.That(flow.EnterMenu(), Is.True); Assert.That(flow.Profile, Is.SameAs(profile));
            Assert.That(flow.EnterHub(), Is.True); Assert.That(flow.Begin(new RunState(1)), Is.True);
            Assert.That(flow.EnterMenu(), Is.False); Assert.That(flow.Phase, Is.EqualTo(RunFlowPhase.Dungeon));
            Assert.That(profile.bankGold, Is.EqualTo(75));
        }
    }
}
