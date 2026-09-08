# Vertical Slice Progress

## Current Milestone
M6 — Weapons + Evolution (next). M0–M5 automated gates passed.

## Completed Work
- Read AGENTS.md and PLAN.md; confirmed Unity 6000.3.23f1, Input System and Test Framework packages.
- Confirmed Windows standalone module installed. Existing Editor has this project open.
- Added Unity cache/build/test-workspace exclusions.
- Created zero-gravity 2D foundation scene, original square sprite, Core/Runtime/Editor/test assemblies, batch validation tooling.
- Initialized development branch; M0 checkpoint prepared.
- M0 committed as a859db1. M1 fixed input actions, separated player components, data asset, prefab and PlayerArena scene implemented; independent review completed.
- M1 committed as 3706a6a. M2 damage/health, hitbox/hurtbox, attack timing/feedback and CombatArena implemented. Independent review found target knockback and combat-disable cleanup gaps; both fixes are under regression validation.
- M2 review fixes passed regression, including target knockback, combat disable cleanup, attack-speed lifetime and canonical script GUID handling.
- M2 committed as ad5bc2b. M3 Chaser/Ranged/Charger, enemy data/prefabs, telegraphs, projectile lifecycle, enemy health/motor and EnemyArena implemented and reviewed.
- M3 committed as f2f9f37. M4 seeded graph, room definitions/prefabs, locked exits, one-time Gold rewards, new RunState, actor-owned effect cleanup and DungeonArena with New Run UI implemented.
- M4 committed as 4f4e2ae. M5 Momentum model/config, action-event Combo definitions/recognizer, attack-speed/Gold bonuses, dash-hit bonus and HUD implemented. Independent review fixes include final-hit reward ordering. Active dash transition regression confirmed and corrected; final tests pending.
- M5 final regression passed, including the dash transition correction, exact final-hit payout, both combos with actual hits and injected-clock pause/resume.

## Tests Executed
M0: EditMode 1/1 passed; PlayMode 1/1 passed. Red tests and passing XML retained in docs/qa/evidence.
M1: cumulative EditMode 10/10 passed; cumulative PlayMode 6/6 passed, including actual virtual-device input and physical travel comparisons.
M2: cumulative EditMode 18/18 passed; PlayMode 13/13 passed. All game scene/prefab missing-script checks passed; actual mouse click damages the target.
M3: cumulative EditMode 26/26 passed; PlayMode 19/19 passed. Verified retreat, chase, charge direction/recovery/active visual, projectile deduplication/wall/lifetime and owner death cleanup.
M4: cumulative EditMode 33/33 passed; PlayMode 23/23 passed. Verified three combat rooms, physical exit trigger, duplicate reward rejection, invalid spawn rejection, active projectile cleanup and fresh-run HP/Gold.
M5: cumulative EditMode 36/36 passed; PlayMode 30/30 passed. See docs/qa/m05.md for exact payout, action filtering, dash consumption and clock verification.

## Validation Results
Unity compilation and scene generation succeeded. Automated scene configuration and physical wall collision passed. See docs/qa/m00.md.
M1 compile, fixed inputs, diagonal normalization, Shift hold release, dash timing/distance/walls, aim and focus loss passed. See docs/qa/m01.md.
M2 compile/physics/feedback state/lifecycle regression and independent follow-up review passed. See docs/qa/m02.md.
M3 compile and all automated regressions passed without missing-script warnings or game exceptions. See docs/qa/m03.md.
M4 compile, generated scenes/prefabs and all automated regressions passed. See docs/qa/m04.md.
M5 compile, assets, independent review and all automated regressions passed. See docs/qa/m05.md.

## Manual Playtest Required
MANUAL_PLAYTEST_REQUIRED — visual readability, input responsiveness and combat feel.
Per user instruction, human-only checks will not prevent automated implementation progress.

## Known Issues
No known M0–M5 code/test failure. Rendering/control/combat feel need manual review. Dash data changes require scene restart. Referenced .github/agents prompt files are still absent; their contents are not inferred. M6 onward is not implemented yet.

## Remaining Work
M6–M13 in order, each with tests and commit; outstanding human playtests.

## Next Action
Implement M6 three weapon styles, three run items, seeded reward selection and Dagger combo evolution; validate before M7.
