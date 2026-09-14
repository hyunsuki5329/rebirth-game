# M13 test inventory

Individual current-source results. Explicit relaunch cases are skipped by the full PlayMode suite and executed separately in two processes.

## m13-edit

| Test | Result |
|---|---|
| Roguelite.Tests.AssetReferenceTests.AllGameScenesAndPrefabsHaveResolvableScripts | Passed |
| Roguelite.Tests.BowAssetTests.BowHasProjectileDataAndPointFilteredTransparentArt | Passed |
| Roguelite.Tests.DamageTests.DamageIsNonnegativeAndAppliesMultiplierBeforeArmor(10,1.5f,2,13) | Passed |
| Roguelite.Tests.DamageTests.DamageIsNonnegativeAndAppliesMultiplierBeforeArmor(10,1,50,0) | Passed |
| Roguelite.Tests.DamageTests.DamageIsNonnegativeAndAppliesMultiplierBeforeArmor(-10,1,0,0) | Passed |
| Roguelite.Tests.DamageTests.DamageIsNonnegativeAndAppliesMultiplierBeforeArmor(10,-1,0,0) | Passed |
| Roguelite.Tests.DamageTests.DamageIsNonnegativeAndAppliesMultiplierBeforeArmor(10,1,-5,10) | Passed |
| Roguelite.Tests.DamageTests.OverkillClampsHealthAndDeathOccursOnlyOnce | Passed |
| Roguelite.Tests.DamageTests.PlayerPrefabContainsCombatAndHealthContracts | Passed |
| Roguelite.Tests.DungeonTests.BothRoutesReachBossWithoutCycles(False,4) | Passed |
| Roguelite.Tests.DungeonTests.BothRoutesReachBossWithoutCycles(True,3) | Passed |
| Roguelite.Tests.DungeonTests.RoomRewardsAreOneTimeAndNewRunResetsGold | Passed |
| Roguelite.Tests.DungeonTests.SameSeedProducesSameRoomChoices | Passed |
| Roguelite.Tests.EnemyDefinitionTests.EachEnemyPrefabHasItsOwnBehaviorAndCombatComponents("Chaser") | Passed |
| Roguelite.Tests.EnemyDefinitionTests.EachEnemyPrefabHasItsOwnBehaviorAndCombatComponents("Ranged") | Passed |
| Roguelite.Tests.EnemyDefinitionTests.EachEnemyPrefabHasItsOwnBehaviorAndCombatComponents("Charger") | Passed |
| Roguelite.Tests.EnemyDefinitionTests.RangedEnemyRetreatsHoldsAndApproaches(1,-1) | Passed |
| Roguelite.Tests.EnemyDefinitionTests.RangedEnemyRetreatsHoldsAndApproaches(3.5f,0) | Passed |
| Roguelite.Tests.EnemyDefinitionTests.RangedEnemyRetreatsHoldsAndApproaches(4,0) | Passed |
| Roguelite.Tests.EnemyDefinitionTests.RangedEnemyRetreatsHoldsAndApproaches(4.5f,0) | Passed |
| Roguelite.Tests.EnemyDefinitionTests.RangedEnemyRetreatsHoldsAndApproaches(6,1) | Passed |
| Roguelite.Tests.FoundationTests.PlayerTestSceneSupportsVisibleZeroGravity2DPhysics | Passed |
| Roguelite.Tests.MomentumComboTests.BothCombosRecognizeAndTimeoutOrResetClearsProgress | Passed |
| Roguelite.Tests.MomentumComboTests.MomentumGraceDecayClampAndPartitionAreDeterministic | Passed |
| Roguelite.Tests.MomentumComboTests.PrefixRecoveryAndLongestThenPriorityThenIdChooseOneCompletion | Passed |
| Roguelite.Tests.MovementRulesTests.DashCooldownStartsAtEndAndRejectsRepeatedStarts | Passed |
| Roguelite.Tests.MovementRulesTests.InvalidDashDurationCannotCreateInfiniteSpeed | Passed |
| Roguelite.Tests.MovementRulesTests.MovementClampsDiagonalButPreservesAnalogMagnitude(1,0,False,5) | Passed |
| Roguelite.Tests.MovementRulesTests.MovementClampsDiagonalButPreservesAnalogMagnitude(1,1,False,5) | Passed |
| Roguelite.Tests.MovementRulesTests.MovementClampsDiagonalButPreservesAnalogMagnitude(1,1,True,7.5f) | Passed |
| Roguelite.Tests.MovementRulesTests.MovementClampsDiagonalButPreservesAnalogMagnitude(0,0,True,0) | Passed |
| Roguelite.Tests.MovementRulesTests.MovementClampsDiagonalButPreservesAnalogMagnitude(0.5f,0,False,2.5f) | Passed |
| Roguelite.Tests.MovementRulesTests.SprintReleaseImmediatelyRestoresWalkingSpeed | Passed |
| Roguelite.Tests.PauseContractTests.MenuTransitionCannotBypassAnActiveRunOrDiscardProfile | Passed |
| Roguelite.Tests.PauseContractTests.SessionExposesOwnedPauseController | Passed |
| Roguelite.Tests.PixelArtTests.ActorArtAndAllHeldWeaponsAreBoundWithoutChangingColliders | Passed |
| Roguelite.Tests.PlayerPrefabTests.PlayerPrefabHasSeparatedInputAndSinglePhysicsMotor | Passed |
| Roguelite.Tests.QaProfilePathTests.NormalLaunchUsesDefaultPathAndDevelopmentProfileStaysUnderQaRoot | Passed |
| Roguelite.Tests.RebirthProfileTests.ProfileOwnsPersistentRebirthAndUpgradeState | Passed |
| Roguelite.Tests.RebirthTests.EligibilityExactCostPointsAndPersistentUpgradesAreAtomic | Passed |
| Roguelite.Tests.RebirthTests.PermanentGrowthAndThreatRewardAreIndependentlyMeasurable | Passed |
| Roguelite.Tests.RoomClearTests.DoorWaitsForEveryRegisteredEnemyAndClearFiresOnce | Passed |
| Roguelite.Tests.RoomClearTests.EmptyRewardRoomIsAlreadyClear | Passed |
| Roguelite.Tests.RoomDefinitionTests.EmptyCombatAndMissingSpawnAreInvalidButEmptyRewardIsValid | Passed |
| Roguelite.Tests.RunConclusionTests.SimultaneousDeathWinsAndResultCannotBeAppliedTwice | Passed |
| Roguelite.Tests.RunFlowTests.VictoryAndDeathSettleOnceAndOldEventsCannotPayNewRun | Passed |
| Roguelite.Tests.RunLoopAssetTests.MainMenuHubAndDungeonScenesExist | Passed |
| Roguelite.Tests.SaveContractTests.ProfileIncludesSettingsAndSaveCodecExists | Passed |
| Roguelite.Tests.SaveSerializationTests.AllRequiredFieldsRoundTripAndMissingFileCreatesDefault | Passed |
| Roguelite.Tests.SaveSerializationTests.FailedReplacementKeepsPreviousFileAndAllowsRetryWithoutChangingProfile | Passed |
| Roguelite.Tests.SaveSerializationTests.InvalidOrUnsupportedFileIsPreservedWithSafeDefault("{}") | Passed |
| Roguelite.Tests.SaveSerializationTests.InvalidOrUnsupportedFileIsPreservedWithSafeDefault("not json") | Passed |
| Roguelite.Tests.SaveSerializationTests.InvalidOrUnsupportedFileIsPreservedWithSafeDefault("{\"schemaVersion\":99,\"profile\":{}}") | Passed |
| Roguelite.Tests.SaveSerializationTests.InvalidPrimaryAndBackupBothSurviveFallbackAndLaterSaves | Passed |
| Roguelite.Tests.SaveSerializationTests.UnknownIdsAndInvalidNumbersAreRejectedRatherThanSilentlyApplied | Passed |
| Roguelite.Tests.SaveSerializationTests.UnwritableLocationReportsFailureWithoutMutatingProfile | Passed |
| Roguelite.Tests.SaveSerializationTests.ValidBackupRecoversAfterCorruptionAndSurvivesNextWrite | Passed |
| Roguelite.Tests.ShortcutRewardTests.SeededRareRollHonorsChanceBoundsAndElitePremium | Passed |
| Roguelite.Tests.VerticalSliceReferenceTests.GamePrefabsAndDataHaveNoMissingScriptsOrBrokenSerializedReferences | Passed |
| Roguelite.Tests.WeaponTests.EvolutionStartsOnThirdQualifyingComboAndRemainsEvolved | Passed |
| Roguelite.Tests.WeaponTests.ItemStatsStackAdditivelyAndChoicesAreDistinctAndSeeded | Passed |

## m13-play-final

| Test | Result |
|---|---|
| Roguelite.Tests.BossEntryTests.BossEntranceContainsLockedLivingBoss | Passed |
| Roguelite.Tests.BossTests.BothPatternsRepeatWithLockedChargeDirectionAndRecoveryWindows | Passed |
| Roguelite.Tests.BossTests.EachWeaponCanKillActiveBossDuringRecoveryAndVictoryCleansEffects | Passed |
| Roguelite.Tests.BossTests.PhysicalDashAvoidsLockedChargeAndAllowsBowCounterattack | Passed |
| Roguelite.Tests.BossTests.SameFrameBossAndPlayerDeathEmitsOnlyDeath | Passed |
| Roguelite.Tests.BowInputTests.MouseSelectsBowWithoutAttackingThenAimsAndFires | Passed |
| Roguelite.Tests.BowTests.ArrowLifetimeAndNewRunCleanupAreBounded | Passed |
| Roguelite.Tests.BowTests.ArrowStopsAtWallAndWeaponSwitchRemovesFlyingArrow | Passed |
| Roguelite.Tests.BowTests.BowIsFourthWeaponAndArrowDealsOneHitWithMomentum | Passed |
| Roguelite.Tests.CombatTests.AttackHitsMultipleHurtboxesOnlyOnceAndCannotHitSelf | Passed |
| Roguelite.Tests.CombatTests.AttackRejectsTargetBehindAimAndCleansUpHitbox | Passed |
| Roguelite.Tests.CombatTests.AttackSpeedScalesHitboxLifetimeAlongWithWindup | Passed |
| Roguelite.Tests.CombatTests.DisablingCombatCancelsWindupAndRestoresMovement | Passed |
| Roguelite.Tests.CombatTests.PlayerKnockbackStopsAtWallAndDeathStopsAttacks | Passed |
| Roguelite.Tests.CombatTests.RealAttackKnocksTargetBack | Passed |
| Roguelite.Tests.EliteHazardTests.EliteHazardCancelsOnOwnerDeathAndRestart | Passed |
| Roguelite.Tests.EliteHazardTests.WarningDealsNoDamageThenHitsOnceAndCanBeDodged | Passed |
| Roguelite.Tests.EnemyCombatTests.ChargerTelegraphsLocksDirectionThenRecovers | Passed |
| Roguelite.Tests.EnemyCombatTests.ChaserApproachesAndStopsWhenTargetDies | Passed |
| Roguelite.Tests.EnemyCombatTests.EnemyDeathRemovesItsProjectilesAndEmitsDeathOnce | Passed |
| Roguelite.Tests.EnemyCombatTests.ProjectileStopsAtWallsAndExpiresAwayFromTargets | Passed |
| Roguelite.Tests.EnemyCombatTests.RangedRetreatsWhenPlayerIsTooClose | Passed |
| Roguelite.Tests.EnemyCombatTests.RangedTelegraphsAndOneProjectileHitsMultipleHurtboxesOnce | Passed |
| Roguelite.Tests.FoundationPlayTests.FoundationBodyDoesNotFallAndCannotCrossWall | Passed |
| Roguelite.Tests.MomentumComboPlayTests.AcceptedActionsRecognizeBothCombosButAirSpamGivesNoMomentum | Passed |
| Roguelite.Tests.MomentumComboPlayTests.BothCombosCompleteAlongsideRealEnemyHits | Passed |
| Roguelite.Tests.MomentumComboPlayTests.DashBonusRequiresPostDashHitAndIsConsumedOnce | Passed |
| Roguelite.Tests.MomentumComboPlayTests.FinalRealHitContributesToExactRoomPayout | Passed |
| Roguelite.Tests.MomentumComboPlayTests.InjectedClockFreezesAndResumesMomentumDecay | Passed |
| Roguelite.Tests.MomentumComboPlayTests.RealHitsRaiseAttackSpeedAndGoldAndNewRunResets | Passed |
| Roguelite.Tests.MomentumComboPlayTests.RoomTransitionCancelsActiveDashWithoutResettingCooldown | Passed |
| Roguelite.Tests.PermanentUpgradeTests.HubRebirthAppliesBothUpgradesWithoutMutatingPlayerAsset | Passed |
| Roguelite.Tests.PiercingWaveTests.WavePiercesTwoTargetsOnceAndReportsFinalKillReward | Passed |
| Roguelite.Tests.PiercingWaveTests.WaveStopsAtWallAndExpiresWithoutTargets | Passed |
| Roguelite.Tests.PixelVisualTests.EveryWeaponRendersAndColoredCharacterStillFlashesOnHit | Passed |
| Roguelite.Tests.PlayerControllerTests.ActualWasdAndShiftHoldReachTheSingleMotor | Passed |
| Roguelite.Tests.PlayerControllerTests.EqualTimeAxisAndDiagonalTravelHaveEqualDistance | Passed |
| Roguelite.Tests.PlayerControllerTests.IdleDashUsesMousePositionAimAndAttackIsLeftClick | Passed |
| Roguelite.Tests.PlayerControllerTests.LeftMouseClickReachesCombatAndDamagesTarget | Passed |
| Roguelite.Tests.PlayerControllerTests.SprintDoesNotMultiplyDashAndWallStopsIt | Passed |
| Roguelite.Tests.PlayerControllerTests.WalkingAndSprintingDashCoverTheSameDistance | Passed |
| Roguelite.Tests.RebirthSessionTests.ThreeFreshProfilesReachFirstRebirthThenEarnFasterOnNextRunWithoutRewardCheats | Passed |
| Roguelite.Tests.RoomTransitionTests.ExitTriggerAdvancesOnlyAfterClearAndDeathPreventsAdvance | Passed |
| Roguelite.Tests.RoomTransitionTests.RestartRemovesEnemyProjectileAndOldActors | Passed |
| Roguelite.Tests.RoomTransitionTests.ThreeCombatRoomsRequireKillsPayOnceAndRestartResets | Passed |
| Roguelite.Tests.RoomTransitionTests.TransitionDisablesPlayerAttackEffectsImmediately | Passed |
| Roguelite.Tests.RouteComparisonTests.SameBowBuildRunsEachRouteThreeTimesAndRecordsRiskReward | Passed |
| Roguelite.Tests.RunLoopInputTests.MouseChangesVolumeAndPersistsSetting | Passed |
| Roguelite.Tests.RunLoopInputTests.MouseNavigatesMenuWeaponRewardAndResultButtons | Passed |
| Roguelite.Tests.RunLoopInputTests.PauseFreezesCombatTimersAndInputsThenResumesAndReturnsToMenu | Passed |
| Roguelite.Tests.RunLoopTests.MenuVictoryHubSecondRunDeathHubRetainsBankAndResetsRunState | Passed |
| Roguelite.Tests.SaveRelaunchTests.ReadProfileWrittenByPreviousUnityProcess | Skipped |
| Roguelite.Tests.SaveRelaunchTests.WriteProfileForNextUnityProcess | Skipped |
| Roguelite.Tests.SaveSessionTests.RecreatedSessionLoadsSettlementRebirthUpgradeAndSettingsWithoutResumingRun | Passed |
| Roguelite.Tests.SaveSessionTests.SaveFailureKeepsLiveProfileAndExplicitRetryWritesIt | Passed |
| Roguelite.Tests.ShortcutTests.ChoiceRoomHasTwoDistinctPhysicalExitsAndDangerLeadsToElite | Passed |
| Roguelite.Tests.ShortcutTests.RareRewardGrantsTwoStacksAndConvertsCappedStacksOnlyOnce | Passed |
| Roguelite.Tests.VerticalSliceTests.ActualEnemyDamageEndsRunAndReturnsToHub | Passed |
| Roguelite.Tests.VerticalSliceTests.DaggerEvolvesThroughCombatActionsAgainstActiveEnemies | Passed |
| Roguelite.Tests.VerticalSliceTests.PhysicalSafeAndShortcutVictoriesRebirthNextRunAndReload | Passed |
| Roguelite.Tests.WeaponGripTests.BowFacesForwardAndGripFollowsHand | Passed |
| Roguelite.Tests.WeaponGripTests.MeleePoseUsesAcceptedAttackDirectionThenReturnsToIdle | Passed |
| Roguelite.Tests.WeaponPlayTests.ThreeCombatCombosEvolveDaggerAndSpawnPiercingWaveInOneRun | Passed |
| Roguelite.Tests.WeaponPlayTests.WeaponSelectionChangesCombatAndItemsRespectCapAndReset | Passed |
| Roguelite.Tests.WorldThreatTests.SameSeedRangedEnemyChangesFromOneToThreeTelegraphedShots | Passed |

## m13-relaunch-write

| Test | Result |
|---|---|
| Roguelite.Tests.SaveRelaunchTests.WriteProfileForNextUnityProcess | Passed |

## m13-relaunch-read

| Test | Result |
|---|---|
| Roguelite.Tests.SaveRelaunchTests.ReadProfileWrittenByPreviousUnityProcess | Passed |
