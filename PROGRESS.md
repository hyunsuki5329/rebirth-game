# Vertical Slice Progress

## Current Milestone
M13 — build and final QA in progress. M12 committed as c245e01 after EditMode 60/60 and PlayMode 63/63 ordinary tests passed; independent review complete.

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
No known failing M0–M12 automated check. Windows build/runtime smoke and final requirement audit remain M13. Human pacing/feel/readability checks remain MANUAL_PLAYTEST_REQUIRED. Referenced .github/agents prompt files remain absent; their contents are not inferred.

## Remaining Work
M13 Windows Development Build, build execution/full-loop smoke, relaunch-save verification, final requirement audit and commit; outstanding human playtests. Full goal remains unfinished.

## Next Action
Inspect the live m13-play Unity process/result, then complete the requirement audit, standalone visual inspection, final QA evidence and M13 commit before marking the goal complete. README and docs/qa/m13.md are now present.

## M13 verification update
Windows Development Build succeeded with zero errors/warnings. Actual standalone Safe Route Boss victory (Gold 116/HP 100), Rebirth, upgrade/settings save and next Threat 1 run passed using isolated QA profile m13-fd01721e95a9. After explicit termination of the held hidden player, a separate executable process verified every serialized Profile field and no mid-run resume. Reports are preserved in docs/qa/evidence/m13-smoke-*.json. Full EditMode 61/61 passed; latest full PlayMode m13-play was launched as PID 7100 and its terminal result still requires inspection. Independent M13 code review found no actionable blockers. Hidden standalone window was absent from native window inventory; visual verification is not claimed. Final audit, remaining verification and M13 commit are still pending. Goal remains active.

## Latest follow-up — weapon grip
Bow orientation corrected; all weapons use explicit grip anchoring with hand markers. Added melee swing, Bow drawing-hand/recoil and front/back layering. EditMode 40/40 and PlayMode 41/41 passed; no game exceptions or compile errors in final logs. Independent review completed. Source Editor shows updated rendering; final visual feel remains MANUAL_PLAYTEST_REQUIRED. M7 follow-up completed; see docs/qa/m07.md.

## M7 completed
Two physical routes, elite telegraphed ground attacks, seeded rare x2 rewards and capped-stack conversion implemented. EditMode 41/41, PlayMode 46/46 passed. Six same-build real-combat route comparisons reached Boss entrance; Dangerous traded 14 HP for less time and more Gold. Review completed. Human movement/avoidance and route readability remain MANUAL_PLAYTEST_REQUIRED. Boss combat begins at M8.

## M8 completed
Two-pattern Rift Warden, locked Boss room, HP/phase HUD and one-time Victory/Death signal implemented. Same-frame Death wins. EditMode 42/42 and PlayMode 51/51 passed; four weapons kill the active Boss, recovery windows are measured and physical Dash permits dodge/counterattack. Review completed. Ordinary play feel remains MANUAL_PLAYTEST_REQUIRED. M7 committed as 7080b4d.

## M9 completed
MainMenu/Hub/Dungeon scenes, owned in-memory session, active-Run-ID settlement, Victory 100% and Death 50% retention, result UI and Hub weapon selection implemented. New runs reset actor state and keep Bank Gold. Actual mouse UI tests pass. EditMode 44/44, PlayMode 53/53. Review complete. Save remains M11; M8 committed as d24b2b6.

## M10 completed
Data-driven Rebirth cost/multiplier, one-time HP/Combo upgrades, preserved meta state, Threat 1 three-arrow behavior and separate Threat Gold factor implemented. Three fresh-profile Safe Route sessions without reward/health cheats each reached first Rebirth after one win; actual earnings rose from 116 to 280 on the next run. Expected runs to a common 200 Gold goal decreased from two to one using the permanent factor alone. Full EditMode 47/47 and PlayMode 56/56 passed; no compile/critical game exceptions in final logs. Independent review complete. See docs/qa/m10.md for methodology and evidence. MANUAL_PLAYTEST_REQUIRED for human pacing, growth feel and UI readability; disk persistence remains M11.

## M11 completed
Added schemaVersion 1 envelope, Profile Settings, stable-ID/value validation and a file store with temporary-file flush/replace, backup recovery and preservation of corrupt/unsupported primary and backup originals. Expected I/O failures return an explicit error without changing the caller's Profile or previous valid file. Failed preservation blocks writes. GameBootstrap now loads/saves through persistentDataPath by default; settlement, Rebirth, purchases and volume changes save, with failure/retry and recovery notices. Tests use isolated QA profiles. Session tests 5/5 and separate Unity-process write/read checks 1/1 each passed. Review's backup-preservation issue reproduced, fixed and re-reviewed; final EditMode 57/57 passed. The first full PlayMode run exposed a volume-click test failure; the cause and final resolution are recorded below.

Final resolution: reproduced duplicate EventSystems in the test fixture, moved isolated session creation to sceneLoaded after old-scene cleanup, and added a one-EventSystem assertion. Affected sequence 9/9 and final full PlayMode 59/59 ordinary tests passed. Both explicit relaunch tests were then rerun in separate processes, 1/1 each. No compile/critical game errors or duplicate EventSystem warnings in final logs; independent review complete. M11 verifiable DoD is satisfied; docs/qa/m11.md records evidence and manual limitations.

## M12 completed
Added owned pause/time/audio restoration, Pause/Resume and pause-volume UI, Hub-to-MainMenu navigation, HUD Dash cooldown and controls. Actual mouse tests prove frozen actors/HP/cooldown, usable volume and successful resume/menu return. Fixed Input Contract is unchanged.

Test-driver navigation was corrected for waypoint tolerance, obstacles, exit triggers and wall-contact escape; all movement still uses physical input/collision. Full-suite physical Safe/Shortcut Bow victories, Rebirth/next run/session reload, natural enemy Death→Hub and active-enemy Dagger evolution + Safe victory passed. Dagger ended at Gold 118/HP36; Bow Safe 116/HP100 and Shortcut 122/HP100. No runtime combat/stat cheats were added. Full EditMode 60/60 and ordinary PlayMode 63/63 passed, including Prefab/data audit. Two explicit process-relaunch checks retain separate M11 passing evidence. Final logs are free of compile/critical game errors; independent review complete. docs/qa/m12.md records input/API scope, evidence and MANUAL_PLAYTEST_REQUIRED limitations.
