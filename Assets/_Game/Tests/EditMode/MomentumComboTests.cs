using NUnit.Framework;
using Roguelite.Core;
namespace Roguelite.Tests
{
    public class MomentumComboTests
    {
        private static readonly CombatAction A = CombatAction.Attack, D = CombatAction.Dash;
        [Test]
        public void MomentumGraceDecayClampAndPartitionAreDeterministic()
        {
            var whole = new MomentumModel(100, 2, 10);
            var split = new MomentumModel(100, 2, 10);
            whole.Add(200, 0); split.Add(200, 0);
            Assert.That(whole.Value, Is.EqualTo(100));
            whole.Tick(4);
            for (int i = 0; i <= 40; i++) split.Tick(i / 10.0);
            Assert.That(whole.Value, Is.EqualTo(80).Within(0.001));
            Assert.That(split.Value, Is.EqualTo(whole.Value).Within(0.001));
            Assert.That(whole.Multiplier(0.5f), Is.EqualTo(1.4f).Within(0.001));
            whole.Tick(4); Assert.That(whole.Value, Is.EqualTo(80).Within(0.001));
            whole.Tick(100); Assert.That(whole.Value, Is.Zero);
        }
        [Test]
        public void BothCombosRecognizeAndTimeoutOrResetClearsProgress()
        {
            var combo = new ComboRecognizer(new ComboRule("dagger", new[] { A, A, D, A }, 1),
                new ComboRule("dash", new[] { D, A, A }, 1));
            Assert.That(combo.Push(A, 0), Is.Null);
            combo.Push(A, 0.2); combo.Push(D, 0.4);
            Assert.That(combo.Push(A, 0.6), Is.EqualTo("dagger"));
            combo.Push(D, 1); combo.Push(A, 1.2);
            Assert.That(combo.Push(A, 1.4), Is.EqualTo("dash"));
            combo.Push(D, 2); combo.Push(A, 4);
            Assert.That(combo.Push(A, 4.2), Is.Null);
            combo.Reset(); combo.Push(D, 5); combo.Push(A, 5.2); combo.Reset();
            Assert.That(combo.Push(A, 5.4), Is.Null);
        }
        [Test]
        public void PrefixRecoveryAndLongestThenPriorityThenIdChooseOneCompletion()
        {
            var combo = new ComboRecognizer(new ComboRule("long", new[] { A, A, D, A }, 1),
                new ComboRule("short", new[] { D, A }, 1, 100));
            combo.Push(A, 0); combo.Push(A, 0.1); combo.Push(A, 0.2); combo.Push(D, 0.3);
            Assert.That(combo.Push(A, 0.4), Is.EqualTo("long"));
            var priority = new ComboRecognizer(new ComboRule("a", new[] { A }, 1), new ComboRule("z", new[] { A }, 1, 1));
            Assert.That(priority.Push(A, 0), Is.EqualTo("z"));
            var stable = new ComboRecognizer(new ComboRule("z", new[] { A }, 1), new ComboRule("a", new[] { A }, 1));
            Assert.That(stable.Push(A, 0), Is.EqualTo("a"));
        }
    }
}
