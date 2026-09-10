using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Roguelite.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Roguelite.Tests
{
    public class BossTests
    {
        private DungeonManager dungeon;
        private IEnumerator EnterBoss()
        {
            yield return SceneManager.LoadSceneAsync("DungeonArena"); yield return null;
            dungeon = Object.FindFirstObjectByType<DungeonManager>();
            while (dungeon.Run.RoomIndex != 6)
            {
                foreach (var enemy in dungeon.Room.Enemies) enemy.Receive(10000, Vector2.zero);
                yield return null;
                if (dungeon.PendingRewards.Length > 0) dungeon.ChooseReward(0);
                Assert.That(dungeon.Advance(true), Is.True); yield return null;
            }
        }
        [UnityTest]
        public IEnumerator BothPatternsRepeatWithLockedChargeDirectionAndRecoveryWindows()
        {
            yield return EnterBoss();
            var boss = dungeon.Boss; Assert.That(boss, Is.Not.Null);
            int charge = 0, blast = 0, recovery = 0;
            boss.PhaseChanged += phase => { if (phase == BossPhase.ChargeWarning) charge++; if (phase == BossPhase.BlastWarning) blast++; if (phase == BossPhase.Recovery) recovery++; };
            float recoveryStarted = -1, expectedRecovery = 0;
            int verifiedRecovery = 0;
            BossPhase previous = boss.Phase;
            boss.PhaseChanged += phase =>
            {
                if (phase == BossPhase.Recovery)
                {
                    recoveryStarted = Time.time;
                    expectedRecovery = previous == BossPhase.Charging ? boss.data.attack.recovery : boss.patterns.blastRecovery;
                }
                else if (previous == BossPhase.Recovery && recoveryStarted >= 0)
                {
                    Assert.That(Time.time - recoveryStarted, Is.InRange(expectedRecovery, expectedRecovery + 0.1f));
                    verifiedRecovery++;
                }
                previous = phase;
            };
            float deadline = Time.time + 20;
            Vector2 locked = Vector2.zero;
            while ((charge < 2 || blast < 2 || recovery < 3) && Time.time < deadline && !dungeon.Player.IsDead)
            {
                if (boss.Phase == BossPhase.ChargeWarning)
                {
                    var direction = boss.GetComponent<EnemyAttack>().Direction;
                    if (locked == Vector2.zero) locked = direction;
                    Assert.That(Vector2.Distance(direction, locked), Is.LessThan(0.001f));
                    dungeon.Player.GetComponent<PlayerMotor2D>().Teleport(new Vector2(6, 4));
                }
                else locked = Vector2.zero;
                if (boss.Phase == BossPhase.BlastWarning) dungeon.Player.GetComponent<PlayerMotor2D>().Teleport(new Vector2(-6, -4));
                if (boss.Phase == BossPhase.Recovery && recoveryStarted >= 0 && Time.time - recoveryStarted > 0.04f)
                {
                    Assert.That(Object.FindObjectsByType<Hitbox>(FindObjectsSortMode.None), Is.Empty);
                    Assert.That(Object.FindObjectsByType<TelegraphedHazard>(FindObjectsSortMode.None), Is.Empty);
                    Assert.That(boss.GetComponent<EnemyMotor2D>().DesiredVelocity, Is.EqualTo(Vector2.zero));
                }
                yield return null;
            }
            Assert.That(charge, Is.GreaterThanOrEqualTo(2)); Assert.That(blast, Is.GreaterThanOrEqualTo(2));
            Assert.That(recovery, Is.GreaterThanOrEqualTo(3)); Assert.That(dungeon.Player.IsDead, Is.False);
            Assert.That(verifiedRecovery, Is.GreaterThanOrEqualTo(2));
        }
        [UnityTest]
        public IEnumerator EachWeaponCanKillActiveBossDuringRecoveryAndVictoryCleansEffects()
        {
            for (int weapon = 0; weapon < 4; weapon++)
            {
                yield return EnterBoss();
                var boss = dungeon.Boss; var health = boss.GetComponent<EnemyHealth>();
                var player = dungeon.Player; var motor = player.GetComponent<PlayerMotor2D>();
                Assert.That(player.GetComponent<WeaponController>().Select(weapon), Is.True);
                int attacks = 0, hits = 0;
                player.GetComponent<PlayerCombat>().AttackStarted += () => attacks++;
                player.GetComponent<PlayerCombat>().Hit += (target, amount) => hits++;
                float deadline = Time.time + 90; int ended = 0;
                dungeon.RunEnded += result => ended++;
                while (!health.IsDead && !player.IsDead && Time.time < deadline)
                {
                    if (boss.Phase == BossPhase.Recovery)
                    {
                        // Approach from the arena interior, never spawn the player/arrow inside a wall.
                        Vector3 stand = boss.transform.position + (boss.transform.position.y < 0 ? Vector3.up : Vector3.down) * 1.15f;
                        stand.x = Mathf.Clamp(stand.x, -7.4f, 7.4f); stand.y = Mathf.Clamp(stand.y, -3.9f, 3.9f);
                        Assert.That(Physics2D.OverlapBox(stand, Vector2.one * 0.65f, 0, LayerMask.GetMask("World")), Is.Null);
                        motor.Teleport(stand);
                        player.GetComponent<PlayerCombat>().RequestAttack(boss.transform.position - stand);
                    }
                    else
                    {
                        // Scripted positioning isolates usable recovery windows; this is not a human playtest.
                        var hazards = Object.FindObjectsByType<TelegraphedHazard>(FindObjectsSortMode.None);
                        Vector2 safe = new Vector2(6, -4);
                        foreach (var hazard in hazards) if (Vector2.Distance(hazard.transform.position, safe) < 2) safe = new Vector2(-6, 4);
                        if (boss.Phase == BossPhase.ChargeWarning) safe = new Vector2(6, 4);
                        motor.Teleport(safe);
                    }
                    yield return null;
                }
                TestContext.WriteLine($"BOSS_WEAPON weapon={weapon} bossHP={health.Current} playerHP={player.Current} attacks={attacks} hits={hits} seconds={90 - (deadline - Time.time):F2}");
                Assert.That(health.IsDead, Is.True, $"Weapon {weapon}; bossHP={health.Current}, playerHP={player.Current}, attacks={attacks}, hits={hits}"); yield return null;
                Assert.That(dungeon.Outcome, Is.EqualTo(RunOutcome.Victory)); Assert.That(ended, Is.EqualTo(1));
                Assert.That(Object.FindObjectsByType<Hitbox>(FindObjectsSortMode.None), Is.Empty);
                Assert.That(Object.FindObjectsByType<TelegraphedHazard>(FindObjectsSortMode.None), Is.Empty);
                yield return new WaitForSeconds(0.4f); Assert.That(ended, Is.EqualTo(1));
            }
        }
        [UnityTest]
        public IEnumerator PhysicalDashAvoidsLockedChargeAndAllowsBowCounterattack()
        {
            yield return EnterBoss();
            var boss = dungeon.Boss; var player = dungeon.Player;
            player.GetComponent<PlayerMotor2D>().Teleport(new Vector2(-3, 0));
            Assert.That(player.GetComponent<WeaponController>().Select(3), Is.True);
            float deadline = Time.time + 3;
            while (boss.Phase != BossPhase.ChargeWarning && Time.time < deadline) yield return null;
            Assert.That(boss.Phase, Is.EqualTo(BossPhase.ChargeWarning));
            float hp = player.Current;
            Assert.That(player.GetComponent<PlayerDash>().RequestDash(Vector2.up), Is.True);
            yield return new WaitForSeconds(0.3f);
            Assert.That(player.transform.position.y, Is.GreaterThan(2.5f));
            deadline = Time.time + 3;
            while (boss.Phase != BossPhase.Recovery && Time.time < deadline) yield return null;
            Assert.That(boss.Phase, Is.EqualTo(BossPhase.Recovery)); Assert.That(player.Current, Is.EqualTo(hp));
            float bossHP = boss.GetComponent<EnemyHealth>().Current;
            Assert.That(player.GetComponent<PlayerCombat>().RequestAttack(boss.transform.position - player.transform.position), Is.True);
            yield return new WaitForSeconds(0.65f);
            Assert.That(boss.GetComponent<EnemyHealth>().Current, Is.LessThan(bossHP));
        }
        [UnityTest]
        public IEnumerator SameFrameBossAndPlayerDeathEmitsOnlyDeath()
        {
            yield return EnterBoss(); int count = 0; RunOutcome result = RunOutcome.InProgress;
            dungeon.RunEnded += outcome => { count++; result = outcome; };
            dungeon.Boss.GetComponent<EnemyHealth>().Receive(10000, Vector2.zero);
            dungeon.Player.Receive(10000, Vector2.zero);
            Assert.That(count, Is.Zero, "Conclusion is deferred until both deaths can be observed.");
            yield return null;
            Assert.That(result, Is.EqualTo(RunOutcome.Death)); Assert.That(count, Is.EqualTo(1));
            Assert.That(dungeon.Advance(), Is.False); yield return new WaitForSeconds(0.3f); Assert.That(count, Is.EqualTo(1));
        }
    }
}
