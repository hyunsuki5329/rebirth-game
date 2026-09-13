# 직접 플레이 테스트

Unity에서 `Roguelite > Play Vertical Slice`를 선택한다. MainMenu를 열고 Play Mode로 전환한다. 변경 중인 씬이 있으면 Unity의 저장 확인을 먼저 처리한다.

메뉴가 보이지 않으면 `Assets/_Game/Scenes/MainMenu.unity`를 열고 상단 ▶ Play를 누른다. `Enter Hub` → 무기 선택 → `Enter Dungeon`으로 시작한다. Game 뷰를 클릭한 뒤 조작하며 Play를 다시 누르면 종료한다.

| 조작 | 기능 |
|---|---|
| WASD | 이동 |
| Left Shift 누르고 있기 | 달리기 |
| Space | Dash |
| 마우스 위치 | 조준 |
| 마우스 왼쪽 클릭 | 공격 |

- Hub에서 Dagger / Sword / Greatsword / Bow 중 하나를 선택한다.
- Bow는 클릭당 화살 1발을 발사한다. 초기 피해 16, 발사 주기 0.5초, 화살 속도 14, 수명 0.6초다. 적 또는 벽에 닿으면 사라진다.
- 적을 모두 처치한 뒤 출구로 이동한다. 보상이 나오면 하나를 선택해야 다음 방으로 이동한다.
- 갈림길의 초록 사각형은 일반 전투 2방, 주황 마름모는 정예 전투 1방이다. 정예의 주황 바닥 표시를 피하면 추가 공격을 회피할 수 있다. Rare 보상은 2스택이며 상한 초과분은 Gold로 전환된다.
- Dagger Combo: 공격 → 공격 → Dash → 공격. 실제 명중을 동반하여 3회 완성하면 진화한다.
- 다른 Combo: Dash → 공격 → 공격. 단계 사이에는 1초 이내에 다음 행동을 한다. Cooldown에 거절된 공격 클릭은 Combo로 세지 않는다.
- 보스는 방향 예고 돌진과 바닥 순차 폭발을 번갈아 사용한다. 공격 후 Recovery가 반격 구간이다.
- 결과 화면은 획득 Gold와 보존율을 표시한다. Victory는 100%, Death는 50%를 Bank Gold에 한 번만 정산한다. `Return to Hub` 후 다시 시작하면 HP, 아이템, 진화, Momentum과 Run Gold는 초기화된다.
- Dungeon의 `Pause` 버튼으로 전투와 시간을 멈춘다. 정지 화면에서 볼륨을 조절하거나 `Resume`으로 이어갈 수 있다. Hub의 `Main Menu`는 시작 메뉴로 돌아간다. HUD에 Dash의 남은 Cooldown을 표시한다.

- 첫 승리 후 Bank Gold가 100 이상이면 Hub의 `Rebirth`를 사용할 수 있다. 비용을 소비하고 Point 1, 영구 Gold 배율 2배와 Threat 1을 얻는다. 기존 Unlock과 영구 Upgrade는 유지된다.
- Vitality는 Point 1로 다음 런 최대 HP를 20 늘린다. Focus는 Point 1로 Combo 단계 사이 허용 시간을 0.25초 늘린다. 각 Upgrade는 한 번만 구매한다.
- Threat 1부터 원거리 적이 3방향 화살을 발사한다. Threat 보상 배율 1.2배는 영구 Gold 배율과 별도로 적용된다.

메타 성장과 설정은 `Application.persistentDataPath/profile.json`에 저장한다. 기본 Windows 위치는 `%USERPROFILE%/AppData/LocalLow/DefaultCompany/My Game/profile.json`이다. 결과 정산, Rebirth, Upgrade 구매, 볼륨 변경 시 저장하며 런 중간 이어하기는 제공하지 않는다. MainMenu와 Hub의 `Vol -` / `Vol +`로 전체 볼륨을 변경한다. 저장 실패가 표시되면 현재 세션을 종료하기 전에 `Retry Save`를 사용한다. 손상/지원하지 않는 저장은 `.corrupt-*` 원본을 보존하고 정상 백업 또는 새 Profile로 복구한다.

개발용 `Assets/_Game/Scenes/DungeonArena.unity`에는 즉시 무기 교체와 `New Run` 버튼이 남아 있다. 실제 메뉴/Hub 흐름 검증은 MainMenu에서 시작한다.

MANUAL_PLAYTEST_REQUIRED — 이동·달리기·Dash 감각, 공격·예고 가독성, Combo 난도, 경로 선택 및 보스 난이도. 자동 검증을 인간 조작 테스트 완료로 기록하지 않는다.
