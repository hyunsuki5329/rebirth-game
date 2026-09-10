# Vertical Slice Progress

## Current Milestone
M9 — Main Menu, Hub and run settlement validated. M6 committed as b5881e6; weapon-grip follow-up committed as dfb55d3.

## Completed Work
- M0 foundation (a859db1), M1 player/input (3706a6a), M2 combat (ad5bc2b), M3 enemies (f2f9f37), M4 rooms (4f4e2ae), M5 Momentum/Combo (65701e3).
- M6 Dagger/Sword/Greatsword/Bow data, weapon selection, three capped run items, seeded rewards and Dagger combo evolution with piercing wave.
- Bow actual input, projectile damage/Momentum, wall/lifetime and cleanup; UI clicks do not fire attacks.
- Generated and applied Explorer, four held weapons, Slime, Goblin and Boar. Explicit Sprite/Texture2D imports and colored-art hit flash. Dedicated evolution-wave sprite.
- Added Roguelite > Play Vertical Slice and docs/PLAYTEST.md. Independent review findings fixed and re-reviewed.

## Tests Executed
- M0: EditMode 1/1, PlayMode 1/1.
- M1: EditMode 10/10, PlayMode 6/6.
- M2: EditMode 18/18, PlayMode 13/13.
- M3: EditMode 26/26, PlayMode 19/19.
- M4: EditMode 33/33, PlayMode 23/23.
- M5: EditMode 36/36, PlayMode 30/30.
- M6 final: EditMode 40/40, PlayMode 39/39; zero failures. Evidence: docs/qa/evidence/m06-edit.xml and m06-play.xml.

## Validation Results
Final Unity tests compiled and passed, including actual Bow input, one-run evolution, wave hit/kill/reward integration, reset/cleanup, asset imports and hit feedback. Final logs contain no compile errors, missing scripts, NullReferenceException or MissingReferenceException. See docs/qa/m06.md.
Source Editor startup/screenshots confirmed applied artwork and Bow selection. This is automated visual verification, not completed human gameplay QA. Fixed Input Contract is unchanged.

## Manual Playtest Required
MANUAL_PLAYTEST_REQUIRED — responsiveness, combat feel, ordinary-player combo difficulty and visual readability. Art currently uses static idle sprites and rotating held weapons. Human-only checks do not prevent subsequent implementation under the user's instructions.

## Known Issues
No known failing M0–M6 automated check. Main Menu, Hub, Rebirth, Save and Windows build remain subsequent milestones. Referenced .github/agents prompt files remain absent; their contents are not inferred.

## Remaining Work
M10–M13 in order, each with tests and commit; outstanding human playtests. Full goal remains unfinished.

## Next Action
Commit M9, then implement Rebirth and World Threat in M10.

## Latest follow-up — weapon grip
Bow orientation corrected; all weapons use explicit grip anchoring with hand markers. Added melee swing, Bow drawing-hand/recoil and front/back layering. EditMode 40/40 and PlayMode 41/41 passed; no game exceptions or compile errors in final logs. Independent review completed. Source Editor shows updated rendering; final visual feel remains MANUAL_PLAYTEST_REQUIRED. M7 follow-up completed; see docs/qa/m07.md.

## M7 completed
Two physical routes, elite telegraphed ground attacks, seeded rare x2 rewards and capped-stack conversion implemented. EditMode 41/41, PlayMode 46/46 passed. Six same-build real-combat route comparisons reached Boss entrance; Dangerous traded 14 HP for less time and more Gold. Review completed. Human movement/avoidance and route readability remain MANUAL_PLAYTEST_REQUIRED. Boss combat begins at M8.

## M8 completed
Two-pattern Rift Warden, locked Boss room, HP/phase HUD and one-time Victory/Death signal implemented. Same-frame Death wins. EditMode 42/42 and PlayMode 51/51 passed; four weapons kill the active Boss, recovery windows are measured and physical Dash permits dodge/counterattack. Review completed. Ordinary play feel remains MANUAL_PLAYTEST_REQUIRED. M7 committed as 7080b4d.

## M9 completed
MainMenu/Hub/Dungeon scenes, owned in-memory session, active-Run-ID settlement, Victory 100% and Death 50% retention, result UI and Hub weapon selection implemented. New runs reset actor state and keep Bank Gold. Actual mouse UI tests pass. EditMode 44/44, PlayMode 53/53. Review complete. Save remains M11; M8 committed as d24b2b6.
