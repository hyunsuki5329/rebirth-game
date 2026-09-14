# 2D Action Roguelite Vertical Slice Plan

**작성일:** 2026-09-08
**상태:** M0–M13 구현·자동 검증 완료. 최종 Windows 개발 빌드와 별도 프로세스 Save 재실행 검증 통과. 사람 조작 체감 항목은 MANUAL_PLAYTEST_REQUIRED로 유지한다. 아래 초기 분석과 설계 기록은 작성 당시의 내용이며 현재 증빙은 PROGRESS.md 및 docs/qa/vertical-slice-checklist.md를 따른다.
**Goal:** 작은 던전 한 런과 Rebirth 이후 다음 런까지 연결되는 플레이 가능한 Vertical Slice를 만든다.
**Architecture:** C# 핵심 규칙, Unity 컴포넌트, ScriptableObject 설정을 분리한다. 기능별 책임을 나누되 범용 프레임워크를 먼저 만들지 않는다.
**Tech Stack:** Unity 6000.3.23f1 / C# / Input System / URP / Unity Test Framework / Git / VS Code.
**Spec:** 사용자가 제공한 개발 요청 및 루트 `AGENTS.md`의 12개 프로젝트 규칙. 본 문서에 첫 구현 범위와 설계 결정을 함께 기록한다.
**실행 방법:** 후속 구현 요청 시 executing-plans 절차로 Milestone별 작업과 검증을 진행한다. 이 문서 작성은 구현 착수나 완료를 의미하지 않는다.

## 1. 초기 프로젝트 분석 (2026-09-08 기록)

파일을 직접 확인한 결과이며 Editor 실행, Console 정상 여부, 실제 빌드 성공은 이번 단계에서 검증하지 않았다.

| 항목 | 관찰 결과 | 계획에 미치는 영향 |
|---|---|---|
| Unity | `ProjectSettings/ProjectVersion.txt`: 6000.3.23f1 | 기존 버전을 유지한다. |
| 입력 | manifest: Input System 1.20.0, activeInputHandler=1 | 새 Input System 기반은 있으나 액션 재구성이 필요하다. |
| 렌더링 | URP 17.3.0, PC/Mobile 렌더러 설정 | 기존 URP 유지 가능. 2D Renderer 전환은 선택 사항이다. |
| 씬 | 빌드 등록은 `Assets/Scenes/SampleScene.unity` 하나 | Main Menu/Hub/Dungeon 흐름이 없다. |
| 카메라 | SampleScene orthographic=0, Directional Light 존재 | 현재 씬은 3D 템플릿 성격이다. XY 평면과 직교 카메라로 정리한다. |
| Editor | m_DefaultBehaviorMode=0 | 기본 2D 작업 모드 설정을 검토한다. |
| 코드 | C#은 TutorialInfo의 Readme/ReadmeEditor만 확인 | Player, 전투, 던전, 저장 등 게임 시스템이 없다. |
| 테스트 | Test Framework 1.6.0 선언, 게임 테스트 없음 | 패키지 선언과 테스트 통과는 별개다. 테스트 어셈블리를 구성한다. |
| 기타 패키지 | AI Navigation, Multiplayer Center, Visual Scripting 등 선언 | 설치되어 있다는 이유로 사용하지 않는다. 제거는 참조 조사 후 결정한다. |
| Git | git status가 not a git repository 반환, .gitignore 없음 | Milestone 0에서 초기화와 Unity 제외 규칙이 필요하다. |
| 지침 | `AGENTS.md` 존재 | 사용자 정정에 따라 AGENT.md를 AGENTS.md로 이름 변경했다. |
| 기존 지침 연결 | `.github/agents/main_instruction.prompt.md`, part_agent, work_agent 파일 없음 | 실제 assertion/이벤트 형식을 추정해 만들지 않는다. 지침 연결 정리는 M0 제안이다. |

기존 Input Actions의 Player 맵에는 Move/Look/Attack/Interact/Crouch/Jump/Previous/Next/Sprint가 있다. Space는 Jump에 연결되고 Look은 Pointer delta를 읽는다. 고정 계약의 Dash와 Mouse Position Aim으로 그대로 사용할 수 없다. 새 액션으로 수정할 때 기존 UI 맵과 프로젝트 설정의 GUID 참조를 보존·검증한다.

## 2. 고정 제약과 설계 방향

- WASD 이동, Space Dash, Shift Hold Sprint, Mouse Position Aim, Left Mouse Button Primary Attack을 고정한다. 예약 키에 다른 기능을 배정하지 않는다.
- Sprint는 스태미나 없이 배율 1.5를 초기 데이터로 사용한다. 대각선 키보드 입력은 정규화하고 향후 아날로그 입력 크기는 ClampMagnitude로 보존한다.
- 입력은 PlayerInputReader만 읽는다. UI 열림, 포커스 상실, 비활성화 시 누름 상태를 해제한다.
- 필수 2D 기반은 Orthographic Camera, XY Plane, Rigidbody2D, Collider2D, SpriteRenderer, Gravity 0이다. 플레이어 물리 이동의 최종 적용자는 하나다. URP 2D Renderer 전환은 필수가 아니며 불필요한 복잡성을 늘리면 현재 URP 설정을 유지한다.
- MonoBehaviour에 모든 기능을 몰아넣지 않는다. 테스트 가능한 계산은 Unity 수명 주기와 분리한다.
- 직접 제작한 도형·색상·간단한 효과를 사용한다. 다른 게임의 자산과 고유 표현은 복제하지 않는다.
- 오류를 숨기지 않는다. Compile/Console/참조/Prefab 오류가 있는 Milestone은 완료 처리하지 않는다.
- 첫 검증 플랫폼은 현재 환경에 맞춰 Windows 데스크톱을 계획상 기본값으로 둔다. 배포 플랫폼 확대는 후속 범위다.

### 접근 방법 비교

1. **권장: 제작한 Room Prefab + 작은 Seed 기반 경로 선택.** 분기와 반복 플레이를 검증하기 쉽고 완전한 생성기의 비용을 피한다.
2. 완전히 고정된 단일 던전: 제작은 빠르지만 런마다 변하는 경험의 검증이 약하다.
3. 완전 절차 생성 던전: 다양성은 크지만 연결성·배치·난이도 검증 부담이 초기 목표를 넘는다.

### 첫 Slice의 콘텐츠 상한

| 범주 | 첫 완료 목표 |
|---|---|
| 공간 | 1개 테마, MainMenu/Hub/Dungeon 씬, Room Prefab 6개 내외 재사용 |
| 적 | Chaser/Ranged/Charger 3종, 기존 적 기반 Elite modifier 1종 |
| Boss | 1종, 예고가 있는 서로 다른 공격 패턴 2개 |
| 무기 | Dagger/Sword/Greatsword/Bow 4개, 공격 주기·범위·이동 제약·Combo 차이. Bow는 사용자의 추가 요청으로 범위 확장 |
| 아이템 | 데이터 기반 런 전용 아이템 3개, 보상 선택으로 빌드 변화 |
| Combo | 행동 순서 기반 정의 2개, 무기별 정의 연결 |
| Evolution | Dagger 진화 1개, 조건 달성 후 관통 파동 등 공격 동작 변화 |
| 메타 | Rebirth 1회 이상, 영구 Upgrade 2개, Threat 0/1 차이 실제 체험 |
| UI | 필수 조작/상태/보상/결과/설정 화면, 최종 아트 제외 |

이는 작지만 다수 시스템이 연결되는 범위다. 세부 콘텐츠를 늘리지 않고 M1~3의 조작·전투 감각이 확인된 뒤 진행한다. 사용자 요구의 최소 적/무기/Boss/진화 개수는 줄이지 않는다.

## 3. 플레이 루프와 상태 소유권

Main Menu → Hub(무기 선택) → Dungeon 진입 → Combat A → Combat B → Reward → 경로 선택.

- Safe Route: Combat C → Combat D → Boss.
- Dangerous Shortcut: Elite Room → Boss. 일반 전투방 하나를 줄이되 Elite를 강제하고 별도 보상을 준다.
- Boss 처치 또는 플레이어 사망 → Run Result → Hub → 재화 확인 → 조건 충족 시 Rebirth → 다음 런.
- Combat A/B 및 Safe Route의 방 변형과 보상은 Seed에 따라 선택한다. Reward/Elite/Shortcut/Boss Room Type을 명시한다. Shortcut은 선택용 공간, Elite는 위험 전투 공간이다.
- Boss와 플레이어가 같은 프레임에 사망하면 Death를 우선한다. 결과 확정은 한 번만 가능하다.

**RunState:** Seed, 현재 HP, 무기/진화, 아이템, Momentum, Combo 진행, 획득 Run Gold, Room 방문 상태. 새 런과 Rebirth 시 초기화한다.

**ProfileData:** Bank Gold, Rebirth Count/Point, 업그레이드 ID, Unlock ID, 누적 클리어, 영구 배율, 설정, 마지막 정산 ID, schemaVersion. Hub 복귀 시 Victory는 Run Gold 100%, Death는 50%를 정산하는 초기 데이터를 사용한다.

**Rebirth 초안:** 최초 Boss 클리어 이후 데이터로 정한 Bank Gold 비용을 충족하면 Hub에서 선택 가능하다. 비용 100은 초기 데이터값일 뿐 고정 조건이 아니다. 비용을 지불하고 Count +1, Point +1, World Threat +1을 적용한다. 일반적인 플레이로 첫 Rebirth를 약 1~2회의 성공적인 Dungeon Run 안에서 경험하도록 비용과 보상을 함께 조정한다. 일반 Run Reset은 Count/Point/Threat를 바꾸지 않는다. Unlock과 구입한 영구 Upgrade는 유지한다.

**Permanent Gold Multiplier:** Rebirth 단계별 데이터로 관리한다. `1 + 0.10 × Count`는 초기 후보일 뿐 고정 공식이 아니다. 첫 Rebirth 직후 동일 기본 Gold 보상에 적용된 실제 정산액과 다음 메타 목표까지 필요한 성공 런 수를 비교하여, 이전보다 명확하게 빠른 성장을 체험하도록 조정한다. HUD/결과 화면에 기본 보상과 배율 적용액을 표시한다. Threat 보상 변화와 배율의 효과는 계산 테스트에서 따로 검증한다.

**영구 Upgrade 초안:** Point로 최대 HP 보너스 또는 Combo 허용 시간 증가를 선택한다. 재화 배율만 증가하는 메타가 되지 않도록 선택을 제공한다. 하나의 구매는 한 번만 적용한다.

**World Threat 초안:** Count에서 Threat를 계산한다. Threat 1부터 Ranged가 예고 후 3방향 발사를 하도록 modifier를 적용하고 위험 보상도 증가한다. Slice에서는 1 이상에 같은 패턴을 사용하며 무한 패턴 확장은 하지 않는다.

**저장 경계:** 룸 중간 이어하기는 제외한다. 재실행하면 마지막 확정 Profile로 Hub/Main Menu에 진입하고 진행 중 Run은 종료된다. 결과 정산, Rebirth, Upgrade 구매 후 Profile 스냅샷을 단일 파일로 저장한다. 가능하면 임시 파일 작성 후 교체한다. 중복 정산은 Run ID로 방지하며 별도 트랜잭션 프레임워크는 만들지 않는다. 파일이 없으면 기본 Profile을 생성한다. 손상 파일은 보존하고 유효한 백업이 있으면 복원하며, 없으면 복구 안내와 기본 Profile로 안전하게 시작한다. schemaVersion에 따른 읽기 분기 확장 지점만 두고 실제 이전 버전이 없는 v1→v2 migration은 구현하지 않는다.

## 4. 핵심 시스템 계약

### Player와 Combat

- PlayerInputReader는 Move 벡터, Aim의 화면 좌표, SprintHeld, DashPressed, AttackPressed를 제공한다. 포인터 화면 좌표는 카메라를 통해 XY 월드 조준점으로 변환한다.
- PlayerMovement는 기본 속도, PlayerSprint는 Hold 배율, PlayerDash는 Dash 상태와 속도 요청을 담당한다. PlayerMotor2D가 Dash > 일반 이동/Sprint 순으로 한 번만 물리를 적용한다. Sprint 중 Dash에도 Sprint 배율을 다시 곱하지 않는다.
- 무입력 Dash는 유효한 마우스 조준 방향을 사용하고, 조준 벡터가 0이면 마지막 유효 방향, 초기에는 오른쪽을 사용한다.
- Dash 거리/시간/쿨다운을 데이터화하고 속도는 거리/시간으로 산출한다. 벽을 통과하지 않으며 쿨다운은 Dash 종료부터 시작한다. 첫 Slice에는 무적 프레임과 Dash 전용 공격을 넣지 않는다.
- 한 클릭은 한 공격 요청이다. 무기별 recovery 동안 새 요청은 거절한다. 미래 입력 버퍼는 체감 검증 후 별도 범위로 둔다.
- Hitbox는 공격 소유자와 attackId를 가진다. Hurtbox는 Health로 피해를 전달한다. 한 공격은 대상당 한 번만 피해를 주고 자기 자신을 때리지 않는다. 넉백과 이동 우선권을 정의하고 피격/죽음 이벤트를 중복 발행하지 않는다.

### Momentum와 Combo

- Momentum은 0~100 초기 범위로 Hit/Kill/Combo/빠른 Room Clear에 증가하고, 마지막 전투 이후 유예 시간이 지나면 감소한다. 시간과 증가량은 데이터다.
- Dash 보너스는 Dash 종료 후 짧은 데이터 시간 창 안에서 실제 Hit가 난 경우 1회 지급한다. 빈 방에서 Dash 반복으로 Momentum을 얻지 않는다.
- 높은 Momentum은 공격 속도와 Room Gold 보너스에 영향을 준다. 각 상한을 데이터로 둔다. UI/일시정지에서는 전투 시간도 멈추고 새 런에서 0으로 초기화한다.
- Combo는 키 입력 자체가 아닌 성립한 AttackStarted/DashStarted 행동 이벤트를 받는다. 쿨다운에 거절된 입력은 이벤트가 아니다. 공격 성공 보상과 Combo 행동 인식은 구분한다.
- 정의 예: Attack → Attack → Dash → Attack, Dash → Attack → Attack. 단계 간 제한 시간 초과 시 초기화, 틀린 행동은 새 시퀀스 첫 행동이 될 수 있게 접두사를 재검사한다.
- 같은 행동에서 여러 Combo가 끝나면 긴 정의, 명시적 priority, 안정된 ID 순으로 하나만 발동한다. 완료 시 소비하고 재귀적으로 Combo를 생성하지 않는다. 무기 교체/사망/런 종료 시 진행을 초기화한다.

### 데이터와 의존 방향

- ScriptableObject는 무기/적/아이템/Combo/진화/Rebirth/Threat의 불변 설정이다. 런의 가변 상태를 에셋에 저장하지 않는다.
- 새 데이터 항목은 기존 효과 종류를 조합해 추가한다. 완전히 새로운 행동의 구현 코드는 필요할 수 있으며 모든 행동을 노코드화하는 에디터는 만들지 않는다.
- 핵심 규칙은 일반 C# 클래스와 명시적 입력값으로 계산한다. Unity 컴포넌트가 시간·입력·충돌을 전달한다.
- Bootstrap은 Profile/Save와 씬 이동만 연결한다. Scene 단위 composition root가 플레이어/Room/UI를 연결한다. 전역 이벤트 버스와 거대한 Singleton을 기본 구조로 두지 않는다.
- 전투의 Hit/Kill → Momentum, 행동 이벤트 → Combo, Combo 완료 → 효과/Momentum, Room Clear → Reward, Run 종료 → 정산, Rebirth → Profile/Threat로 전달한다. 구독 해제는 수명 주기에 맞춘다.

### 제안 폴더 구조

다음 경로는 아직 생성하지 않은 계획 경로다. 아래 Milestone의 파일 경로는 이 기준을 따른다.

```text
Assets/_Game/
  Scenes/                    MainMenu, Hub, Dungeon, PlayerTest
  Scripts/
    Core/                    순수 계산과 상태 모델
    Input/ Player/ Combat/ Enemy/ Dungeon/ Reward/
    Momentum/ Combo/ Weapon/ Rebirth/ WorldThreat/ Save/ UI/
  Data/                      기능별 ScriptableObject 에셋
  Prefabs/                   Player, Enemies, Rooms, UI
  Art/                       직접 만든 Placeholder
  Tests/EditMode/             계산·직렬화 테스트
  Tests/PlayMode/             입력·물리·씬·루프 테스트
docs/qa/                     Milestone별 재현 절차와 결과
```

Core/Runtime/EditMode/PlayMode asmdef를 최소 분리하고 기능마다 어셈블리를 과도하게 만들지 않는다. 기존 SampleScene과 템플릿 자산은 새 기준 씬 검증 전 삭제하지 않는다.

## 5. Milestone 공통 진행 규칙

각 Milestone은 아래 Goal, Implementation Tasks, Dependencies, Important Files, Testing Method, Definition of Done을 모두 충족해야 한다. 다음 단계는 현재 단계의 테스트와 수동 검증 증빙이 있을 때만 시작한다. 구현 시 순수 계산은 실패하는 경계 사례 테스트 → 최소 구현 → 통과 순서로 진행한다.

- EditMode와 해당 PlayMode 테스트를 실행하고 테스트 개수/실패/실행 버전/로그 경로를 `docs/qa/mXX.md`에 기록한다.
- 게임 변경 후 기존 테스트 전체가 깨지지 않았는지 확인한다. 실행하지 못한 검증은 미검증으로 기록하고 완료를 주장하지 않는다.
- Console Error, Missing Script, 참조 누락, Broken Prefab은 원인을 해결한다.
- 검증된 실행 가능한 단위로 독립 commit을 남긴다. 실패 상태를 완료 commit으로 묶지 않는다.
- 후속 시스템이 없는 단계에서는 현재 단계의 테스트 씬과 명시적 상태 재설정으로 검증한다. M1은 입력 비활성화/물리, M2는 기본 공격과 행동/피해 이벤트, M4는 RunState 생성·초기화와 경로 노드, M5는 주입 가능한 전투 시간의 정지/재개를 제공한다. 완성 메뉴·Hub는 M9, 일시정지 UI 통합은 M12에서 연결한다. 앞 단계는 이 UI들의 구현을 완료 조건으로 요구하지 않는다.
- 체감 관련 DoD는 사용한 데이터/Seed/무기/반복 횟수와 관찰 결과를 기록한다. '재미있음'만으로 통과시키지 않고 각 Testing Method의 동작 결과와 회피·반격·이동 반응을 함께 확인한다.

## Milestone 0 — Project Foundation

**Goal:** 재현 가능한 2D 프로젝트와 버전 관리 기반을 준비한다.

**Implementation Tasks**
- [x] AGENTS.md를 프로젝트 지침의 단일 원본으로 사용한다(파일명 정정 완료). 동일 내용의 AGENT.md가 다시 발견되면 AGENTS.md로 통합하고 제거하거나 참조 안내만 남긴다. 이 파일명·통합 방침은 다시 질문하지 않는다. 서로 충돌하는 실제 지침이 발견된 경우에만 사용자에게 질문한다.
- [x] Git을 초기화하고 Library/Temp/Obj/Logs/UserSettings/Builds를 제외한다. Assets의 .meta, ProjectSettings, manifest/packages-lock과 지침은 추적한다.
- [x] Unity 버전, VS Code 편집 연동, 패키지 로드와 필수 2D 기반을 확인한다. Orthographic Camera/XY Plane/Rigidbody2D/Collider2D/SpriteRenderer/Gravity 0을 기준 씬에서 검증한다. 기존 URP를 유지할 수 있고 2D Renderer 전환은 완료 조건이 아니다. 불필요 패키지는 참조 확인 후에만 정리한다.
- [x] 폴더/asmdef/PlayerTest 씬과 테스트 실행 기반을 구성한다.

**Dependencies:** 없음. 로컬 Editor 설치와 실행 가능 여부는 이 단계에서 확인한다.

**Important Files:** `.gitignore`, `AGENTS.md`, `Packages/manifest.json`, `Packages/packages-lock.json`, `ProjectSettings/EditorSettings.asset`, `ProjectSettings/EditorBuildSettings.asset`, `Assets/_Game/Scenes/PlayerTest.unity`, `Assets/_Game/Scripts/Core/Game.Core.asmdef`, `Assets/_Game/Scripts/Game.Runtime.asmdef`, `Assets/_Game/Tests/EditMode/Game.EditModeTests.asmdef`, `Assets/_Game/Tests/PlayMode/Game.PlayModeTests.asmdef`.

**Testing Method:** 프로젝트 재열기, 기준 씬에서 직교 투영·XY 스프라이트 표시·Rigidbody2D/Collider2D 충돌·중력 0 확인, Test Runner 어셈블리 검색, 실제 테스트 1개 실행, git check-ignore로 생성 폴더 제외와 .meta 추적 확인. 게임 Controller가 없는 간단한 검증 오브젝트로 수행한다.

**Definition of Done:** 필수 2D 기반 6개가 기준 씬에서 확인되고 패키지/컴파일 오류가 없다. 테스트가 실제 검색·실행되고 Git 변경에 생성 캐시가 없다. URP 렌더러 종류와 무관하게 이 증빙 확보 후 M1 시작.

## Milestone 1 — Player Controller

**Goal:** 반응성이 좋은 이동/Sprint/Dash/조준을 독립 검증한다.

**Implementation Tasks**
- [x] 기존 Input Actions를 Move/Aim/Attack/Dash/Sprint 계약으로 수정하고 UI 맵을 유지한다.
- [x] InputReader, Movement, Sprint, Dash, Motor2D를 분리하고 속도 데이터를 연결한다.
- [x] 벽 충돌, 무입력 Dash, Hold 해제, 포커스 상실, 카메라 추적과 조준 표시를 구현한다.

**Dependencies:** M0.

**Important Files:** `Assets/InputSystem_Actions.inputactions`, `Assets/_Game/Scripts/Input/PlayerInputReader.cs`, `Assets/_Game/Scripts/Player/PlayerMovement.cs`, `PlayerSprint.cs`, `PlayerDash.cs`, `PlayerMotor2D.cs`(동일 Player 폴더), `Assets/_Game/Data/Player/PlayerStats.asset`, `Assets/_Game/Prefabs/Player.prefab`, `Assets/_Game/Tests/PlayMode/PlayerControllerTests.cs`.

**Testing Method:** 축/대각선 같은 시간 이동 거리 비교, Shift 누름/해제 시 1.5/1.0 배율, Sprint 중 Dash 거리 동일성, 쿨다운 재입력 거절, 벽 관통 없음, 정지 Dash 방향, UI/포커스 전환 잔류 입력 확인. 거리 오차 허용치를 물리 timestep 기준으로 기록한다.

**Definition of Done:** 고정 입력 전부 동작하고 기본 이동이 Dash 연타로 대체되지 않으며 데이터 수정이 반영된다. 직접 조작 점검 후 M2 시작.

## Milestone 2 — Combat Foundation

**Goal:** 공격·피해·넉백·사망의 공통 계약을 만든다.

**Implementation Tasks**
- [x] 마우스 방향 기본 공격, Hitbox/Hurtbox, attackId 중복 방지, 팀 구분을 구현한다. 준비/회복/이동 배율은 임시 기본 공격 설정으로 제공하고 M6에서 무기 데이터로 확장한다. 유효 AttackStarted/DashStarted 및 Hit/Kill 이벤트를 연결한다.
- [x] 순수 Damage 계산, PlayerHealth, 넉백 우선권과 사망 1회 처리를 구현한다.
- [x] 공격 예고/유효 범위와 피격 색상/효과를 Placeholder로 보여준다.

**Dependencies:** M1.

**Important Files:** `Assets/_Game/Scripts/Combat/DamageCalculator.cs`, `Hitbox.cs`, `Hurtbox.cs`(동일 Combat 폴더), `Assets/_Game/Scripts/Player/PlayerCombat.cs`, `PlayerHealth.cs`, `Assets/_Game/Tests/EditMode/DamageTests.cs`, `Assets/_Game/Tests/PlayMode/HitboxTests.cs`.

**Testing Method:** 0 피해/보정/HP 하한, 다중 콜라이더 동일 대상 1회 타격, 자해 방지, 사망 이벤트 1회, 공격 recovery 입력 거절, 넉백 벽 충돌 확인.

**Definition of Done:** 테스트 대상에 조준 공격·피격·넉백·사망이 일관되게 동작하고 M1 회귀가 없다. 이후 M3 시작.

## Milestone 3 — Enemy Combat

**Goal:** 행동이 다른 일반 적 3종과 싸울 수 있다.

**Implementation Tasks**
- [x] Chaser는 접근 근접 공격, Ranged는 거리 유지와 예고 투사체, Charger는 예고 후 직선 돌진/회복을 구현한다.
- [x] EnemyData, AI/Attack/Health를 분리하고 Elite modifier 적용 지점을 둔다.
- [x] 벽/플레이어/적 충돌 레이어와 원거리 투사체 수명을 설정한다.

**Dependencies:** M2.

**Important Files:** `Assets/_Game/Scripts/Enemy/EnemyAI.cs`, `EnemyAttack.cs`, `EnemyHealth.cs`, `EnemyData.cs`, `ChaserAI.cs`, `RangedAI.cs`, `ChargerAI.cs`(동일 Enemy 폴더), `Assets/_Game/Prefabs/Enemies/Chaser.prefab`, `Ranged.prefab`, `Charger.prefab`, `Assets/_Game/Tests/PlayMode/EnemyCombatTests.cs`.

**Testing Method:** 세 적의 추적/거리 유지/돌진 차이, 예고 회피 가능성, 장애물 및 타깃 사망, 투사체 정리, 전투 중 이동/Sprint/Dash 체감 점검.

**Definition of Done:** 단순 스탯 변형이 아닌 세 행동을 식별하고 모두 처치 가능하다. 조작·전투 감각 문제를 해결한 뒤 M4 시작.

## Milestone 4 — Room / Dungeon Loop

**Goal:** 여러 방과 기본 보상을 진행할 수 있다.

**Implementation Tasks**
- [x] Combat/Reward/Elite/Shortcut/Boss 타입과 작은 Room graph/Seed 선택을 구현한다.
- [x] 입장/문 잠금/모든 적 처치/출구 해제/다음 방을 구현한다. 빈 방과 잘못된 spawn 데이터도 검사한다.
- [x] RunState 생성/초기화와 최소 RewardService를 도입하고 Room Gold 1회 지급을 연결한다. 테스트 씬에서 새 RunState를 시작할 수 있게 한다. 실제 보상 선택 확장은 M6에서 한다.

**Dependencies:** M3. Boss 전투와 Shortcut 위험은 각각 M8/M7에서 완성한다.

**Important Files:** `Assets/_Game/Scripts/Dungeon/RunState.cs`, `DungeonManager.cs`, `RoomController.cs`, `RoomGenerator.cs`, `RoomDefinition.cs`, `Assets/_Game/Scripts/Reward/RewardService.cs`, `Assets/_Game/Prefabs/Rooms/CombatRoom.prefab`, `Assets/_Game/Tests/EditMode/RoomGraphTests.cs`, `Assets/_Game/Tests/PlayMode/RoomTransitionTests.cs`.

**Testing Method:** 같은 Seed의 경로 동일성, 도달 가능한 Boss 노드, 출구 잠금, 마지막 적 사망 시 1회 클리어/보상, 방 전환 후 적/투사체/구독 정리.

**Definition of Done:** 실제 일반 전투방 최소 3개를 진행하고 보상을 받는다. 임시 Boss 노드는 전체 루프 완성으로 세지 않는다. 이후 M5 시작.

## Milestone 5 — Momentum + Combo

**Goal:** 공격적 행동과 전투 행동 조합에 보상을 준다.

**Implementation Tasks**
- [x] Momentum 증가/유예/감소/상한과 공격 속도·Gold modifier를 구현한다. 전투 시간은 주입 가능하게 하여 UI 없이 정지/재개를 검증한다.
- [x] 데이터 기반 Combo 2개, 시간 창, 불일치 접두사 재시작과 중복 발동 규칙을 구현한다.
- [x] HUD에 Momentum, Combo 피드백을 표시하고 무효 Dash/공격 스팸을 차단한다.

**Dependencies:** M4의 전투 이벤트와 RewardService.

**Important Files:** `Assets/_Game/Scripts/Momentum/MomentumSystem.cs`, `Assets/_Game/Scripts/Combo/ComboRecognizer.cs`, `ComboDefinition.cs`, `Assets/_Game/Data/Momentum/MomentumConfig.asset`, `Assets/_Game/Tests/EditMode/MomentumTests.cs`, `ComboTests.cs`.

**Testing Method:** 증감 경계/시간 분할 독립성/일시정지, 유효·오순서·시간 초과·겹친 Combo, 거절된 입력 미등록, 보너스 상한, 빈 방 Dash로 증가하지 않음.

**Definition of Done:** 두 Combo를 실제 전투에서 발동하고 Momentum이 피해 외 요소에도 영향을 준다. 새 런 초기화를 확인한 뒤 M6 시작.

## Milestone 6 — Weapons + Evolution

**Goal:** 4개 무기 스타일(사용자 추가 요청 Bow 포함), 런 빌드 변화와 1개 진화를 체험한다.

**Implementation Tasks**
- [x] Dagger의 빠른 짧은 연계, Sword의 중간 범위 부채꼴, Greatsword의 긴 준비/넓은 타격/큰 넉백을 데이터와 공격 구현으로 구분한다.
- [x] 공격 중 이동 배율과 무기별 Combo 정의를 적용한다. 현재 테스트 씬에 임시 무기 선택 UI를 제공하고 실제 Hub 연결은 M9에서 수행한다.
- [x] 아이템 3개와 Seed 기반 보상 선택, 중복 처리 규칙을 구현한다.
- [x] 사용자 추가 요청인 Bow를 네 번째 무기로 제공한다. 기존 마우스 조준/왼쪽 클릭 계약을 유지하고 단일 화살의 명중, 벽 차단, 수명, 방 전환 정리와 Momentum 연계를 검증한다.
- [x] Dagger Combo 3회 성공을 초기 진화 조건으로 두고 진화 후 Combo 마무리에 관통 파동을 추가한다. 조건은 데이터화하며 해당 무기를 선택하면 한 런 안에 달성 가능하게 한다.

**Dependencies:** M5.

**Important Files:** `Assets/_Game/Scripts/Weapon/WeaponController.cs`, `WeaponData.cs`, `WeaponEvolution.cs`, `Assets/_Game/Scripts/Reward/ItemData.cs`, `Assets/_Game/Data/Weapons/Dagger.asset`, `Sword.asset`, `Greatsword.asset`, `Assets/_Game/Data/Weapons/DaggerEvolution.asset`, `Assets/_Game/Tests/EditMode/WeaponStatsTests.cs`, `EvolutionTests.cs`.

**Testing Method:** 세 무기 공격 간격/범위/이동/Combo 차이, 효과 중첩 계산, 에셋 런 상태 오염 없음, 진화 조건 직전/직후와 중복 적용, 일반 플레이에서 진화 달성.

**Definition of Done:** 수치만 다른 무기가 아니며 최소 1개 진화가 플레이 스타일을 바꾼다. Seed/선택에 따른 빌드 차이를 확인한 뒤 M7 시작.

## Milestone 7 — Dangerous Shortcut

**Goal:** 실제 위험과 보상이 있는 경로 선택을 제공한다.

**Implementation Tasks**
- [x] Safe 2방과 Elite Shortcut 1방 분기를 연결하고 선택 전에 위험/보상을 표시한다.
- [x] Elite에 예고형 추가 공격 modifier를 주고 위험 방 Gold 보너스와 희귀 보상 확률을 데이터화한다.
- [x] 출구의 색·형태·텍스트를 구분한다. 선택 확정 후 반대 경로 보상 중복 획득을 차단한다.

**Dependencies:** M6.

**Important Files:** `Assets/_Game/Scripts/Dungeon/ShortcutController.cs`, `Assets/_Game/Scripts/Enemy/EliteModifier.cs`, `Assets/_Game/Data/Dungeon/ShortcutRules.asset`, `Assets/_Game/Prefabs/Rooms/ShortcutRoom.prefab`, `EliteRoom.prefab`, `Assets/_Game/Tests/PlayMode/ShortcutTests.cs`.

**Testing Method:** 양 경로의 Boss 입구 노드 도달(전투는 M8), 방 수 차이, 보상 1회, 경로 복귀 악용 차단. 같은 무기·아이템 구성으로 양 경로 각각 3회 이상 Boss 입구까지 시간/남은 HP/사망/보상을 기록한다. 실행 파일 빌드는 요구하지 않는다.

**Definition of Done:** Shortcut의 시간·보상 이득에 실제 전투 위험이 따르고 테스트에서 일방적 우월성이 보이면 값을 조정한다. 이후 M8 시작.

## Milestone 8 — Boss

**Goal:** 일반 적과 다른 최종 전투를 제공한다.

**Implementation Tasks**
- [x] 패턴 1은 방향 예고 후 돌진, 패턴 2는 바닥 위험 구역 예고 후 순차 폭발로 구성한다.
- [x] 패턴 선택/준비/공격/회복 상태와 안전 구간을 구현한다.
- [x] Boss 입장 잠금, 체력 UI, 사망 시 승리 신호와 잔여 위험 제거를 연결한다.

**Dependencies:** M7.

**Important Files:** `Assets/_Game/Scripts/Enemy/BossController.cs`, `BossPatternData.cs`, `Assets/_Game/Prefabs/Enemies/Boss.prefab`, `Assets/_Game/Prefabs/Rooms/BossRoom.prefab`, `Assets/_Game/Tests/PlayMode/BossTests.cs`.

**Testing Method:** 두 패턴 모두 반복 관찰, 예고-피해 시간 일치, 회피·반격 기회, Boss 사망 후 공격 없음, 동시 사망 우선순위 검증.

**Definition of Done:** 세 무기로 처치 가능하고 체력만 큰 일반 적이 아니다. 승리/사망 신호 확인 후 M9 시작.

## Milestone 9 — Run Result + Hub

**Goal:** Main Menu부터 Hub 복귀와 재화 확인까지 연결한다.

**Implementation Tasks**
- [x] Main Menu/Hub/Dungeon 씬과 RunFlow 상태 전환, 최소 GameBootstrap 연결을 구현한다. 앞 단계 테스트 씬의 구성을 실제 흐름에 연결한다.
- [x] Victory/Death 결과, 획득액/보존율/Bank Gold를 표시하고 Run ID로 정산을 한 번만 적용한다.
- [x] Hub 무기 선택, 다음 런 시작과 모든 런 전용 상태 초기화를 구현한다. Profile은 이 단계에서 메모리에 유지한다.

**Dependencies:** M8.

**Important Files:** `Assets/_Game/Scenes/MainMenu.unity`, `Hub.unity`, `Dungeon.unity`, `Assets/_Game/Scripts/Core/ProfileData.cs`, `Assets/_Game/Scripts/Dungeon/RunFlow.cs`, `Assets/_Game/Scripts/UI/RunResultView.cs`, `HubView.cs`, `Assets/_Game/Tests/PlayMode/RunLoopTests.cs`.

**Testing Method:** 승리/사망 각각 전체 루프, 결과 버튼 중복 클릭, 정산 중복 이벤트, 두 번째 런 HP/아이템/진화/Momentum 초기화와 Bank Gold 유지.

**Definition of Done:** Main Menu에서 시작해 두 결과 모두 Hub로 돌아오고 재화가 정확하다. 영속 저장 미완성임을 구분한 뒤 M10 시작.

## Milestone 10 — Rebirth + World Threat

**Goal:** Rebirth 전후의 영구 성장과 세계 변화를 체험한다.

**Implementation Tasks**
- [x] 조건/비용 확인, Count/Point/데이터 기반 Gold 배율, Upgrade 2개와 구매 로직을 구현한다. 비용 100과 기존 10% 공식은 후보로만 취급하며 첫 Rebirth 도달 시간과 이후 성장 체감에 맞게 조정한다.
- [x] Hub에 유지/소비 항목과 Threat 증가를 표시한다. Run 중에는 Rebirth를 허용하지 않는다.
- [x] Threat 1의 Ranged 3방향 패턴과 보상 변화를 다음 런에 적용한다.

**Dependencies:** M9.

**Important Files:** `Assets/_Game/Scripts/Rebirth/RebirthSystem.cs`, `RebirthUpgradeData.cs`, `Assets/_Game/Scripts/WorldThreat/WorldThreatSystem.cs`, `WorldThreatModifier.cs`, `Assets/_Game/Tests/EditMode/RebirthTests.cs`, `WorldThreatTests.cs`.

**Testing Method:** 비용 미만/정확히 충족, 조건 실패 시 변경 없음, 중복 구매/포인트 부족, 배율 데이터와 정산 계산, 동일 Seed·무기 조건의 Threat 0/1 공격 패턴 비교, 일반 Run Reset의 메타 보존. 새 Profile에서 보상 치트 없이 일반 경로를 포함한 플레이 세션 3개를 기록하고 첫 Rebirth까지 성공 런 수를 센다. 동일 기본 보상의 Rebirth 전후 정산액, 동일 메타 목표 비용에 도달하는 예상 성공 런 수, 다음 런에서의 실제 획득액과 성장 체감 관찰을 함께 기록한다.

**Definition of Done:** 일반적인 플레이 기준으로 첫 Rebirth를 약 1~2회의 성공적인 Dungeon Run 안에서 경험하도록 밸런싱하며, 위 3개 세션의 성공 런 수가 이 범위인지 확인한다. 첫 Rebirth 이후 배율 적용 정산액 증가와 동일 메타 목표까지 필요한 예상 성공 런 수 감소가 기록되고, 실제 다음 런에서 더 빠른 성장 체감을 확인한다. 배율 숫자만 표시하는 것으로 통과시키지 않는다. Upgrade 적용과 Threat 1의 Ranged 3방향 공격을 확인한 뒤 M11 시작.

## Milestone 11 — Save / Load

**Goal:** 메타 성장과 설정이 재실행 후 유지된다.

**Implementation Tasks**
- [x] schemaVersion과 안정된 데이터 ID를 가진 저장 DTO를 정의한다. Count/Gold 배율/Point/Upgrade/Unlock/Settings/Bank Gold/정산 ID를 포함한다.
- [x] persistentDataPath에 단일 Profile 파일을 저장한다. 가능하면 임시 파일 작성 후 교체하여 기존 정상 파일 손상을 줄인다. Unity 에셋 참조는 직렬화하지 않는다. 별도 원자적 트랜잭션/복구 프레임워크는 요구하지 않는다.
- [x] 파일이 없으면 기본 Profile을 생성한다. 손상된 파일은 보존하고 유효한 백업이 있으면 사용하며, 없으면 복구 안내 후 기본 Profile로 시작한다. 지원하지 않는 schemaVersion은 원본을 보존하고 안내한다. schemaVersion별 로더 확장 지점만 유지하며 가상의 v1→v2 migration이나 migration 테스트 framework는 필수가 아니다.
- [x] Settings의 기본값과 볼륨 등 첫 설정 변경 경로를 최소 UI로 제공한다. M12는 이 경로와 일시정지 메뉴를 통합한다.
- [x] 결과 정산/Rebirth/구매/설정 변경 시 저장한다. 저장 실패 시 원래 Profile을 유지하고 재시도를 표시한다.

**Dependencies:** M10. 스키마 경계는 M9 Profile 도입 때부터 지킨다.

**Important Files:** `Assets/_Game/Scripts/Save/SaveSystem.cs`, `SaveData.cs`, `ProfileFileStore.cs`, `Assets/_Game/Scripts/UI/SettingsView.cs`, `Assets/_Game/Tests/EditMode/SaveSerializationTests.cs`, `SaveFallbackTests.cs`.

**Testing Method:** 필수 필드 전체 왕복, 파일 없음의 기본값, 잘못된 ID/손상/지원하지 않는 버전의 안전한 처리, 쓰기 실패 시 기존 파일 보존, 중복 정산 ID, 실제 종료 후 재실행 확인. 테스트 전용 임시 디렉터리로 파일 오류를 검증하고 실제 게임은 persistentDataPath를 사용하는지 확인한다. 임시 파일 교체를 적용한 경우 교체 실패도 검사한다. 가상의 버전 migration 테스트는 요구하지 않는다.

**Definition of Done:** schemaVersion, Rebirth Count, Permanent Gold Multiplier, Rebirth Point/Upgrades, Unlock IDs, Settings, Bank Gold가 persistentDataPath 저장 및 재실행 후 보존된다. 파일 없음은 기본 Profile, 손상 파일은 안전한 fallback으로 오류 반복 없이 시작하며 기존 원본을 보존한다. 임시 파일 교체 적용 여부와 쓰기 실패 검증을 기록한다. 런 중간 이어하기 미제공을 UI로 전달한 뒤 M12 시작.

## Milestone 12 — Vertical Slice Integration

**Goal:** 모든 요구 시스템이 하나의 정상 플레이에서 연결된다.

**Implementation Tasks**
- [x] M9의 GameBootstrap과 앞 단계 UI를 연결하여 전체 흐름 UI, HP/Momentum/Dash cooldown, 조작 안내, 보상·진화·Threat 피드백을 통합한다.
- [x] 일시정지/설정/메뉴 이동 시 입력과 시간이 일관되게 정지·복구되도록 한다.
- [x] 데이터 참조 검증과 필요한 경우에만 측정에 근거한 최적화를 수행한다.

**Dependencies:** M11 및 M0~10의 검증 증빙.

**Important Files:** `Assets/_Game/Scripts/UI/GameHUD.cs`, `PauseMenu.cs`, `Assets/_Game/Scripts/Core/GameBootstrap.cs`, `Assets/_Game/Tests/PlayMode/VerticalSliceTests.cs`, `docs/qa/m12.md`.

**Testing Method:** Safe 승리, Shortcut 승리, Death 복귀를 각각 실행한다. 자연 플레이 진화 1회, Rebirth 후 다음 런, 저장 후 재실행, 세 무기/세 적/Boss 두 패턴을 체크리스트로 대조한다.

**Definition of Done:** 요구 루프에 임시 종료 노드나 테스트 전용 우회가 없고 위 시나리오가 오류 없이 완료된다. 이후 M13 시작.

## Milestone 13 — QA / Build

**Goal:** Editor 밖에서 실행 가능한 검증된 빌드를 만든다.

**Implementation Tasks**
- [x] EditMode/PlayMode 전체를 실행하고 Windows Development Build를 만든다.
- [x] Build 실행 Smoke Test로 Main Menu → Hub → Dungeon → Boss → Hub → Rebirth → 다음 Run을 확인한다. 재실행 후 Save를 확인하고 Editor/Player 로그를 점검한다.
- [x] 검증한 Windows 환경과 알려진 제한을 기록한다. 정확한 최소사양 확정, 정식 배포용 Build 최적화와 본격적인 CPU/GPU profiling은 후속 Polish Goal로 둔다.
- [x] README에 실행/조작/빌드/테스트/저장 위치/알려진 제한을 기록하고 완료 commit을 남긴다.

**Dependencies:** M12. 필요한 Windows 빌드 모듈 설치 여부를 확인한다.

**Important Files:** `Assets/_Game/Editor/BuildScript.cs`, `ProjectSettings/EditorBuildSettings.asset`, `README.md`, `docs/qa/m13.md`, `docs/qa/vertical-slice-checklist.md`.

**Testing Method:** 전체 EditMode/PlayMode 결과 XML과 Editor/Player 로그 보관. Windows Development Build를 실행하여 Main Menu → Hub → Dungeon → Boss → Hub → Rebirth → 다음 Run 흐름을 새 Profile로 검증한다(필요하면 첫 Rebirth까지 성공 런 2회). 종료·재실행 후 저장된 성장/재화/설정을 대조한다. Missing Script/Prefab 참조, 반복 Console Error와 치명적 Exception을 점검한다.

**Definition of Done:** 전체 EditMode/PlayMode 테스트 실패 0, Windows Development Build 생성 및 실행 Smoke Test 통과, 위 전체 흐름과 재실행 Save 확인, 반복 Console Error 및 치명적 Exception 없음. 공통 참조/Prefab 오류 기준도 충족한다. 최소사양 확정·정식 배포용 빌드·본격 profiling은 완료 조건이 아니다.

## 6. 설계 모순·범위 검토와 구조 수정 제안

| 발견한 문제 또는 모호함 | 결정 또는 제안 |
|---|---|
| AGENTS.md 파일명 | 이름 변경 완료. 단일 원본 사용과 중복 통합은 재질문하지 않으며 실제 지침 충돌만 질문한다. |
| 이전 오케스트레이터 연결 파일 부재 | 존재하지 않는 규칙을 추정하거나 복원 framework를 필수로 만들지 않는다. 실제 지침 충돌 발견 시에만 확인한다. |
| 2D 목표와 현재 perspective/3D 템플릿 | M0에서 필수 2D 기반 6개 검증. 기존 URP 유지 가능하며 2D Renderer 전환은 선택 사항. |
| Space Jump와 고정 Dash 충돌 | M1에서 Jump 바인딩을 Dash로 교체. Mouse delta도 Mouse position으로 수정. |
| 여러 이동 컴포넌트의 Rigidbody 경쟁 | Motor2D 하나만 최종 속도 적용. Sprint와 Dash는 정책/상태를 제공. |
| Combo가 입력 QTE로 변질될 위험 | 유효한 전투 행동 이벤트만 인식. |
| Dash 반복 Momentum 악용 | 실제 적 Hit와 연결된 시간 창 보너스만 인정. |
| M4 Reward가 M9 결과보다 먼저 필요 | M4에 Run Gold/Reward 최소 계약, M9에 메타 정산을 배치. |
| M6 진화가 M7 Elite에 의존할 위험 | 첫 진화는 Combo 횟수로 달성하여 역방향 의존성을 없앤다. |
| Rebirth 조건/재화 소실 규칙 미지정 | 본문의 비용·유지·배율을 조정 가능한 초안으로 명시. 핵심 방향 변경 시 사용자 결정. |
| Save가 후반에 있어 구조 재작성 위험 | M9부터 Profile/Run 분리와 ID 기반 데이터를 사용. 파일 I/O는 M11. |
| 이전 단계에서 미래 UI/Boss/Bootstrap을 요구할 위험 | M1 입력 비활성화, M4 RunState 초기화, M5 시간 주입, M6 테스트 씬 무기 선택, M7 Boss 입구 노드로 검증. M9 최소 Bootstrap, M11 설정 변경 경로를 제공하고 M12에서 통합. |
| Save와 QA의 과도한 기반 작업 | schemaVersion 확장 지점과 단일 파일 저장/fallback으로 제한. 가상 migration, 정식 배포 최적화와 본격 profiling은 첫 Slice의 필수가 아니다. |
| 작은 Slice 대비 시스템 수가 많음 | 1테마/4무기(사용자 추가 Bow 포함)/3적/1Boss/1진화/1분기/Threat 2단계로 제한. 기능 삭제 대신 콘텐츠 상한 적용. |

이 단계에서 확정된 요구 사이의 해결 불가능한 충돌은 발견하지 않았다. 재화 수치, Rebirth 조건, 보상 보존율, 무기 공격 세부 방식은 사용자 요구 자체가 아닌 본 계획의 초기 설계값이며 플레이테스트로 검증한다.

## 7. 장기 아이디어와 제외 범위

Slice 통과 후에만 무기별 Combo/진화 분기, Sprint와 Momentum 상호작용, Dash i-frame/Dash Attack, Curse/HP 희생형 Shortcut, 새 Room 테마, Seed 생성 확장, Rebirth 선택 트리, Threat별 적 패턴/던전 변형, Controller 지원을 검토한다.

**후속 Polish Goal:** 정확한 최소사양 확정, 정식 배포용 Build 최적화, CPU/GPU profiling과 성능 목표 수립. 실제 저장 schema 변경이 생길 때 필요한 이전 버전 migration을 추가하며 가상 버전을 미리 구현하지 않는다.

멀티플레이, 온라인 서버/계정, Steam, 클라우드 저장, 복잡한 업적/Crafting, 수십 개 무기·Boss, 최종 그래픽은 첫 목표에서 제외한다. 추가 패키지/전용 에디터/대형 DI 프레임워크도 실제 필요가 입증되기 전에는 도입하지 않는다.

## 8. 이번 단계의 산출물과 검증 한계

- 프로젝트 파일/패키지/씬/입력/지침/Git 상태를 읽고 분석했다.
- PLAN.md에 M0~13의 Goal, Tasks, Dependencies, Files, Testing, DoD와 진행 게이트를 작성했다.
- 게임 코드, 씬, 패키지, 입력 설정은 변경하지 않았다. Git 초기화도 수행하지 않았다.
- Unity Play Mode, 자동 테스트, 빌드는 아직 실행하지 않았다. 이 계획은 실행 결과가 아니다.

### 수정 후 자체 검토

- M0~13 순서와 6개 필수 항목을 유지했다. 완료 의존성은 앞 단계로만 향한다. 후속 단계 번호는 향후 통합 범위를 설명하며 현재 통과 조건이 아니다.
- M6 Hub, M7 실제 Boss, M5 완성 일시정지 UI, M9 이후에만 존재하던 Bootstrap 같은 잠재적 역방향 요구를 테스트 씬/노드/시간 주입 및 필요한 단계의 최소 구현으로 정리했다.
- 모든 DoD를 해당 Testing Method의 결과와 연결했다. Rebirth는 일반 플레이 세션 3개에서 1~2회 성공 런 도달, 전후 정산액·목표 도달 런 수와 실제 성장 체감 기록으로 검증한다.
- 저장은 필수 Profile 필드/파일 없음/fallback/가능한 임시 교체로 축소했고 production-level migration 기반 작업을 제거했다. M13은 Development Build와 전체 흐름 Smoke Test 중심이다.
- Input Contract, PlayerInputReader, PlayerMotor2D, Momentum 악용 방지, 행동 Combo, Dagger Combo 진화, 양 경로, 적 3종, Boss 2패턴, Threat 행동 변화, RunState/ProfileData 분리를 유지했다.
- 이 자체 검토는 문서의 의존성·요구사항 검토이며 게임 실행 검증은 아니다. 이번 수정 대상은 PLAN.md뿐이다.
