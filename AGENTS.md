# Project Development Rules

이 프로젝트는 Unity 6.3 LTS와 C#으로 개발하는 싱글플레이 2D 탑다운 액션 로그라이트 게임이다.

Codex는 모든 작업에서 이 문서의 규칙을 최우선 프로젝트 규칙으로 따른다.

## 1. Core Game Identity

게임의 핵심은 다음과 같다.

- 빠른 2D 탑다운 액션 전투
- Room 기반 던전 탐험
- 로그라이트 방식의 반복 플레이
- 런마다 변화하는 무기 및 빌드
- Momentum 시스템
- Key Combo 시스템
- Dangerous Shortcut
- Risk / Reward 선택
- Weapon Evolution
- Rebirth
- World Threat

Sephiria와 Dungreed의 장르적 특징에서 영감을 받을 수 있으나 코드, 그래픽, 캐릭터, 맵, 아이템, 명칭, 사운드, UI 등을 직접 복제하지 않는다.

독립적인 게임으로 개발한다.

## 2. Input Contract

다음 조작 방식은 프로젝트의 고정된 Input Contract다. 임의로 변경하지 않는다.

### Movement

- W = 위로 이동
- A = 왼쪽 이동
- S = 아래로 이동
- D = 오른쪽 이동

WASD 입력은 서로 조합될 수 있어야 하며 대각선 이동 시 이동속도가 비정상적으로 증가하지 않도록 입력 벡터를 정규화한다.

### Dash

Space = Dash

Space를 누르면 현재 이동 방향으로 Dash한다.

이동 입력이 없는 상태에서 Dash하면 마지막으로 바라본 방향 또는 조준 방향 중 게임 설계에 적합한 일관된 방향을 사용한다.

Dash에는 Cooldown을 둔다.

Dash의 거리, 속도, Cooldown은 코드에 직접 하드코딩하지 않고 데이터 또는 설정값으로 관리한다.

Dash가 지나치게 연속 사용되어 기본 이동을 대체하지 않도록 한다.

### Sprint / Run

Shift를 누르고 있는 동안 달린다.

Shift = Sprint / Run

Shift는 Toggle 방식이 아니라 Hold 방식으로 구현한다.

초기 Vertical Slice에서는 Stamina를 요구하지 않는다.

기본 달리기 배율은 약 1.5x를 시작점으로 사용하되 데이터 값으로 관리하여 플레이테스트 후 수정할 수 있게 한다.

Dash와 Sprint는 서로 다른 기능이다.

Sprint 중에도 Dash를 사용할 수 있어야 한다.

### Combat

- Mouse Position = Aim
- Left Mouse Button = Primary Attack

공격 방향은 기본적으로 마우스 위치를 향한다.

추가 능력 입력은 향후 확장할 수 있지만 다음 키는 현재 핵심 입력으로 예약되어 있으므로 다른 기능에 사용하지 않는다.

- W / A / S / D
- Space
- Shift

## 3. Input System

Unity의 새로운 Input System을 사용한다.

Player 입력을 코드 여러 곳에서 직접 읽지 않는다.

입력 처리를 한 곳에서 관리하고 게임 로직과 분리한다.

예:

```text
PlayerInputReader
→ PlayerMovement
→ PlayerDash
→ PlayerCombat
```

게임 시스템이 `Keyboard.current`를 여기저기 직접 참조하는 구조를 피한다.

Input Action은 향후 Controller 지원이 가능하도록 설계하되 첫 번째 버전에서는 Keyboard + Mouse를 우선한다.

## 4. Architecture

하나의 거대한 MonoBehaviour에 플레이어 기능 전체를 구현하지 않는다.

각 시스템은 가능한 한 한 가지 책임만 가진다.

예:

```text
PlayerMovement
PlayerDash
PlayerCombat
PlayerHealth
PlayerStats
MomentumSystem
ComboSystem

EnemyAI
EnemyAttack
EnemyHealth

DungeonManager
RoomController
RoomGenerator

WeaponController
WeaponData
WeaponEvolution

RebirthSystem
WorldThreatSystem
SaveSystem
```

시스템 간 통신은 직접적인 강결합을 최소화한다.

## 5. Data Driven Design

다음 데이터는 가능한 한 ScriptableObject 또는 이에 준하는 데이터 구조를 사용한다.

- Weapons
- Enemies
- Player Stats
- Items
- Weapon Evolutions
- Rebirth Upgrades
- World Threat Modifiers
- Combo Definitions

데이터 하나를 추가하기 위해 핵심 코드를 수정해야 하는 구조를 피한다.

## 6. Player Feel First

기능의 양보다 플레이 감각을 우선한다.

특히 다음 요소를 우선적으로 검증한다.

- Movement
- Sprint
- Dash
- Attack
- Hit feedback
- Enemy interaction
- Momentum

화려한 콘텐츠를 추가하기 전에 기본 이동과 전투가 재미있는지 확인한다.

## 7. Placeholder Policy

초기 개발에는 Placeholder Art를 사용할 수 있다.

단 다음 조건을 만족해야 한다.

- Player와 Enemy가 쉽게 구분될 것.
- 공격 범위가 알아보기 쉬울 것.
- 피격 여부를 명확하게 확인할 수 있을 것.
- Room 경계를 구분할 수 있을 것.
- Dangerous Shortcut과 일반 경로가 시각적으로 구분될 것.

다른 게임에서 가져온 저작물을 Placeholder로 사용하지 않는다.

## 8. Testing

테스트 가능한 핵심 로직은 MonoBehaviour에서 분리한다.

가능한 경우 Unity Test Framework를 사용한다.

특히 다음 시스템은 자동 테스트 가능하도록 설계한다.

- Damage calculation
- Momentum calculation
- Combo recognition
- Weapon stats
- Rebirth multiplier
- World Threat calculation
- Save data serialization

새 기능을 구현한 뒤 기존 테스트가 깨지지 않았는지 확인한다.

## 9. Error Policy

다음 상태를 정상 완료로 간주하지 않는다.

- Compile Error
- NullReferenceException
- MissingReferenceException
- Missing Script
- 반복적으로 발생하는 Console Error
- Scene Reference 오류
- Broken Prefab

원인을 조사하고 해결한다.

오류를 단순히 숨기거나 예외를 무시하여 통과시키지 않는다.

## 10. Git Rules

각 Milestone은 가능한 한 독립적인 Git commit으로 남긴다.

Commit은 해당 단계가 테스트되고 실행 가능한 상태에서 만든다.

대규모 기능을 하나의 불명확한 commit으로 합치지 않는다.

Unity에서 생성되는 불필요한 파일을 Git에 포함하지 않는다.

Unity 프로젝트에 적합한 `.gitignore`를 사용한다.

Assets와 ProjectSettings 및 필요한 Packages 설정은 버전 관리한다.

## 11. Scope Control

요구사항에 없는 대규모 시스템을 임의로 추가하지 않는다.

특히 초기 Vertical Slice에서는 다음 기능을 우선하지 않는다.

- 멀티플레이
- 온라인 서버
- 계정 시스템
- Steam 연동
- 클라우드 저장
- 복잡한 업적 시스템
- 복잡한 Crafting
- 수십 개의 무기
- 수십 개의 Boss
- 정교한 최종 그래픽

먼저 작은 범위의 완전한 게임 플레이 루프를 완성한다.

## 12. Autonomous Decisions

사소한 구현 방식은 프로젝트 구조와 게임 개발 관점에서 합리적으로 판단하여 진행한다.

다음 상황에서만 사용자에게 중요한 결정을 요청한다.

- 게임의 핵심 방향을 변경해야 하는 경우
- Input Contract를 변경해야 하는 경우
- 기존 요구사항끼리 충돌하는 경우
- 필수 외부 프로그램 또는 권한이 없어 진행이 불가능한 경우

그 외의 일반적인 프로그래밍, 파일 구성, 클래스 구성, 테스트 방법 등의 결정은 스스로 판단하여 진행한다.
