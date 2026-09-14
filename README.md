# 2D Action Roguelite Vertical Slice

Unity 6.3 LTS (`6000.3.23f1`) / C# / Windows. 프로젝트 지침은 `AGENTS.md`, 구현 계획은 `PLAN.md`, 검증 현황은 `PROGRESS.md`를 따른다.

## 실행

- Windows: `Builds/Windows/ActionRoguelite.exe`를 실행한다. 배포하거나 이동할 때는 Data 폴더와 DLL을 포함한 `Windows` 폴더 전체를 함께 복사한다.
- 압축 배포본: `Builds/ActionRoguelite-Windows-Development.zip`을 모두 풀고 내부 `Windows/ActionRoguelite.exe`를 실행한다.
- Unity: `Roguelite > Play Vertical Slice` 또는 `Assets/_Game/Scenes/MainMenu.unity`를 열고 Play를 누른다.
- `Enter Hub` → 무기 선택 → `Enter Dungeon`. 상세 진행 방법은 [직접 플레이 안내](docs/PLAYTEST.md)를 참고한다.

| 입력 | 기능 |
|---|---|
| WASD | 이동, 대각선 속도 보정 |
| Left Shift Hold | 달리기 |
| Space | Dash |
| Mouse Position | 조준 |
| Left Mouse Button | 공격 |

Pause/Resume과 볼륨은 화면 버튼을 사용한다. Dagger, Sword, Greatsword, Bow를 제공하며 Safe Route와 Dangerous Shortcut이 동일 Boss로 연결된다. 승리/사망 정산 후 Hub에서 Rebirth와 영구 Upgrade를 구매한다. Threat 1에서는 원거리 적의 공격이 3방향으로 바뀐다.

## 빌드 및 테스트

Windows Build Support가 설치된 위 Unity 버전에서 `Roguelite > Build Windows Development`를 실행한다. 이 메뉴는 MainMenu/Hub/Dungeon 세 씬만 포함한다. Editor의 테스트 씬 목록을 그대로 배포하지 않는다.

```powershell
pwsh -File ./tools/RunUnity.ps1 -Mode EditMode -Tag local-edit
pwsh -File ./tools/RunUnity.ps1 -Mode PlayMode -Tag local-play
pwsh -File ./tools/RunUnity.ps1 -Mode Method -Tag local-build -Method Roguelite.Editor.BuildScript.BuildWindowsDevelopment
```

명령은 격리된 `.validation` 프로젝트에서 비동기로 Unity를 시작한다. `TestResults/<tag>.pid`, `.log`, 테스트 `.xml`로 종료와 결과를 확인한 뒤 다음 명령을 실행한다. CLI 빌드 출력은 `.validation/Builds/Windows`에 생성된다. `tools/RunUnity.ps1`의 Unity 설치 경로는 로컬 환경에 맞게 조정할 수 있다.

## 저장 및 제한

기본 저장 위치는 `%USERPROFILE%/AppData/LocalLow/DefaultCompany/My Game/profile.json`이다. Rebirth, Bank Gold, 영구 성장과 설정을 저장하며 런 중간 이어하기는 제공하지 않는다. 손상 파일은 보존하고 정상 백업 또는 기본 Profile로 복구한다. 저장 실패 시 화면의 `Retry Save`를 사용한다.

개발 빌드 자동 검증은 `--qa-profile <새로운이름> --smoke-run`, 재실행 대조는 같은 이름과 `--smoke-reload`로 명시적으로 켠다. QA 저장은 기본 저장 폴더의 `QA/<이름>`으로 분리된다. 일반 실행에는 자동 조작이 없다. `--smoke-hold`는 검증 성공 후 다음 런을 일시정지한 채 유지한다.

현재 그래픽은 도트 이미지와 회전/공격 포즈 중심이다. 인간 조작에 의한 감각·난이도·가독성 검증은 `MANUAL_PLAYTEST_REQUIRED`다. 최소사양 확정, 정식 배포 최적화와 본격적인 profiling은 후속 작업이다. 최신 검증 범위와 미완료 항목은 `PROGRESS.md` 및 `docs/qa`에 기록한다.
