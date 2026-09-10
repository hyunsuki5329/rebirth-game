using NUnit.Framework;
using UnityEngine;
namespace Roguelite.Tests
{
    public class ShortcutRewardTests
    {
        [Test]
        public void SeededRareRollHonorsChanceBoundsAndElitePremium()
        {
            var rules = ScriptableObject.CreateInstance<ShortcutRules>();
            rules.safeRareChance = 0; rules.eliteRareChance = 1;
            for (int seed = 0; seed < 50; seed++)
            {
                Assert.That(rules.IsRare(seed, 5, false), Is.False);
                Assert.That(rules.IsRare(seed, 7, true), Is.True);
            }
            rules.safeRareChance = 0.2f; rules.eliteRareChance = 0.7f;
            int safe = 0, danger = 0;
            for (int seed = 0; seed < 1000; seed++)
            {
                bool roll = rules.IsRare(seed, 7, true);
                Assert.That(rules.IsRare(seed, 7, true), Is.EqualTo(roll));
                if (roll) danger++; if (rules.IsRare(seed, 5, false)) safe++;
            }
            Assert.That(danger, Is.GreaterThan(safe)); Assert.That(rules.rareStacks, Is.GreaterThan(1));
            Assert.That(10 * rules.eliteGoldMultiplier, Is.GreaterThan(20)); Object.DestroyImmediate(rules);
        }
    }
}
