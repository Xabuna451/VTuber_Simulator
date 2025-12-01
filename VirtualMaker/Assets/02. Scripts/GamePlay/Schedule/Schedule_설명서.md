# Schedule 시스템 설명서

이 문서는 `Schedule`, `ScheduleUI`, `ScheduleNode` 스크립트로 구성된 스케줄 시스템의 구조와 사용법을 정리합니다. 스타일은 `States_설명서.md`와 유사하게, 각 클래스의 역할·필드·메서드·사용 예시·씬 설정 방법을 포함합니다.

---

## 1. 개요
- 역할: 플레이어가 한 턴 동안 (예: 한 주 단위) 1일차 ~ 7일차의 활동을 선택하고 최종 확정하는 UI 및 매니저 로직을 제공한다.
- 흐름 요약:
  1. 플레이어가 스케줄 노드(Streaming/Training/Rest/Social 등)를 클릭하면 `ScheduleUI`의 임시 버퍼(`tempSchedule`)에 순서대로 추가된다 (1일차부터 차례대로).
  2. 플레이어는 필요시 `Undo`(최근 추가 제거) 또는 `Clear`(임시 초기화)를 사용할 수 있다.
  3. 플레이어가 `Confirm`을 누르면 `ScheduleUI`가 연결된 `Schedule` 인스턴스의 `SetWeeklySchedule`을 호출해 실제 주간 스케줄을 확정한다.
  4. 턴 종료 시 `Schedule.ClearWeeklySchedule()`를 호출해 초기화할 수 있다.

---

## 2. `Schedule` (파일: `Schedule.cs`)
- **역할:** 씬에 배치되는 `MonoBehaviour`로, 실제 주간 스케줄 데이터를 보관하고 관리한다. 전역 static이나 싱글턴이 아닌 명시적 인스턴스(Inspector에서 할당)로 사용하도록 설계되어 결합도를 낮춤.

- **주요 필드:**
  - `public List<ScheduleNode> weeklySchedule` : 7일치 스케줄을 보관하는 리스트(0: 1일차, 6: 7일차).
  - `private const int DaysPerWeek = 7` : 주 길이 상수.

- **주요 메서드:**
  - `void Awake()`
    - 인스펙터 값으로도 주간 스케줄을 세팅할 수 있지만, 빈 리스트일 경우 `None`으로 기본 채움 처리.
  - `void SetSchedule(int dayIndex, ScheduleNode activity)`
    - 특정 요일(0-based) 한 칸을 설정. 잘못된 인덱스는 로그 에러.
  - `void SetWeeklySchedule(List<ScheduleNode> newSchedule)`
    - 임시 버퍼(또는 외부 데이터)를 한 번에 적용. 부족한 날은 `None`으로 채움.
  - `List<ScheduleNode> GetWeeklySchedule()`
    - 현재 스케줄의 복사본을 반환(원본 보호).
  - `void ClearWeeklySchedule()`
    - 턴 초기화용. 리스트를 비우고 `None`으로 재채움.

- **사용처/주의사항:**
  - `Schedule`은 씬에 하나의 인스턴스를 두고 `ScheduleUI.scheduleManager`에 드래그하여 참조하게 하는 방식이 권장됩니다.
  - 게임 진행 중 스케줄이 확정되면 이 객체의 `weeklySchedule`가 서브시스템(이벤트, 수입 계산, 컨디션 영향 등)에 의해 참조됩니다.

---

## 3. `ScheduleUI` (파일: `ScheduleUI.cs`)
- **역할:** 플레이어가 스케줄을 편집하는 UI 관리용 컴포넌트. 임시 버퍼에 노드를 쌓고, UI 갱신, 확정 버튼 로직을 담당.

- **주요 필드:**
  - `private List<ScheduleNode> tempSchedule` : 사용자가 클릭한 순서대로 쌓이는 임시 버퍼.
  - `[SerializeField] private Schedule scheduleManager` : Inspector에서 `Schedule` 인스턴스를 할당. 미할당 시 `ConfirmSchedule()`에서 런타임으로 폴백 탐색.
  - `private const int MaxDays = 7` : 최대 허용 슬롯 수.

- **주요 프로퍼티/메서드:**
  - `IReadOnlyList<ScheduleNode> GetTempSchedule()` : 읽기 전용으로 임시 버퍼를 반환.
  - `int NextTempIndex` : 다음에 채워질 임시 슬롯의 0-based 인덱스를 반환. UI에서 "다음에 들어갈 요일"을 표시할 때 유용.
  - `int AddTempNode(ScheduleNode node)` : 임시 버퍼에 노드 추가. 성공하면 추가된 0-based 인덱스 반환, 실패(가득 참) 시 -1 반환.
  - `bool RemoveLastTemp()` : 최근 추가한 임시 노드 제거(Undo). 성공 여부 반환.
  - `void ClearTemp()` : 임시 버퍼 전체 초기화.
  - `void ConfirmSchedule()` : `scheduleManager.SetWeeklySchedule(tempSchedule)`를 호출해 확정 적용. `scheduleManager`가 Inspector에 할당되어 있지 않으면 `FindAnyObjectByType<Schedule>()`로 폴백 탐색 후 적용.
  - `void RefreshUI()` : 실제 프로젝트에서는 이곳에서 슬롯 텍스트/아이콘 변경, 버튼 활성화/비활성화 등을 처리. 현재는 디버그 로그로 상태를 출력.

- **사용 예시:**
  - UI에서 다음과 같이 연결합니다:
    - 각 스케줄 노드 버튼의 `OnClick()` → `ScheduleNodeButton.OnClick()` (해당 버튼이 자신을 참조하도록 설정)
    - Confirm 버튼의 `OnClick()` → `ScheduleUI.ConfirmSchedule()`
    - Undo 버튼의 `OnClick()` → `ScheduleUI.RemoveLastTemp()`
    - Clear 버튼의 `OnClick()` → `ScheduleUI.ClearTemp()`

- **디버그:** `AddTempNode`가 반환하는 인덱스로 UI 슬롯 번호(사용자에게 보여줄 1~7일차)를 표시할 수 있습니다.

---

## 4. `ScheduleNode` / `ScheduleNodeButton` (파일: `ScheduleNode.cs`)
- **역할:** 스케줄에서 선택 가능한 노드(열거형)와, UI 버튼이 클릭될 때 임시 버퍼에 추가하는 버튼 컴포넌트.

- **열거형:** `ScheduleNode { None, Streaming, Training, Rest, Social }`

- **ScheduleNodeButton 주요 필드/메서드:**
  - `public ScheduleNode nodeType` : 이 버튼이 나타내는 노드 타입(인스펙터에서 설정).
  - `[SerializeField] private ScheduleUI scheduleUI` : Inspector에서 `ScheduleUI`를 할당. 미할당 시 런타임에 `FindAnyObjectByType<ScheduleUI>()`로 자동 탐색(폴백).
  - `public void OnClick()` : Unity UI Button의 `OnClick()`에 연결할 수 있도록 `void` 시그니처로 제공. 내부에서 `scheduleUI.AddTempNode(nodeType)`를 호출하고, 추가된 인덱스 또는 실패 여부를 로깅.
  - `public ScheduleNode GetNodeType()` : 코드에서 타입 확인용 액세서.

- **연결 팁:**
  - Inspector로 `scheduleUI`를 할당하는 것이 가장 명시적이고 안정적입니다.
  - 자동 탐색 폴백은 개발 중 빠른 테스트용이며, 빌드/런타임에서는 명시적 할당을 권장합니다.

---

## 5. 씬 설정 & 사용 시나리오(단계별)
1. 빈 GameObject 생성 → 이름 `ScheduleManager` → `Schedule` 컴포넌트 추가.
2. 빈 GameObject 생성 → 이름 `ScheduleUI` → `ScheduleUI` 컴포넌트 추가.
3. `ScheduleUI.scheduleManager`에 `ScheduleManager`를 드래그하여 할당.
4. 각 활동(Streaming/Training 등)을 나타내는 UI 버튼에 `ScheduleNodeButton` 컴포넌트를 추가하고, `nodeType`을 설정.
5. 각 버튼의 UI `OnClick()` 이벤트에 해당 버튼의 `ScheduleNodeButton.OnClick()`을 연결(Inspector에서 드래그 후 함수 선택).
6. Confirm/Undo/Clear 버튼을 만들어 `ScheduleUI.ConfirmSchedule()`, `ScheduleUI.RemoveLastTemp()`, `ScheduleUI.ClearTemp()`에 연결.
7. Play 모드에서 클릭하면 임시 슬롯에 1일차→2일차 순으로 채워지고, Confirm 시 `ScheduleManager.weeklySchedule`에 적용됩니다.

---

## 6. 확장 및 권장 개선사항
- 슬롯별 직접 편집: 특정 요일을 클릭해 덮어쓰는 기능이 필요하면 `ScheduleUI.SetTempAt(int dayIndex, ScheduleNode node)` 메서드를 추가하세요.
- 이벤트 기반 통신: `ScheduleUI`에서 `public event Action<IReadOnlyList<ScheduleNode>> OnConfirm;`을 제공하면 다른 시스템(예: 이벤트 발생/로그/수익 계산)이 느슨하게 구독하여 확장성 향상.
- 시각적 슬롯 뷰: `RefreshUI()` 내부에서 `Text` 또는 `Image`를 업데이트하는 별도 `ScheduleSlot` 컴포넌트를 만들면 유연하게 표시 가능.
- Undo/확정 이력: 확정 후 롤백(확정 취소) 요구 시 `ConfirmSchedule()`에서 이전 스케줄을 백업하는 저장소를 추가.

---

## 7. 디버깅 체크리스트
- 버튼 클릭이 동작하지 않음: 버튼의 `OnClick()`에 `ScheduleNodeButton.OnClick()`이 연결되어 있는지 확인.
- `ScheduleUI` 미할당 경고: `ScheduleUI` 또는 `Schedule.scheduleManager`가 Inspector에 할당되어 있는지 확인.
- 임시 슬롯이 더 이상 추가되지 않음: `AddTempNode`는 최대 7개까지만 허용합니다. `RemoveLastTemp()` 또는 `ClearTemp()`를 호출해 비우세요.
- Confirm 후 스케줄이 반영되지 않음: `ScheduleUI.scheduleManager`가 올바른 `Schedule` 인스턴스를 참조하는지 확인.

---

## 8. 빠른 코드 예시
- 버튼에서 노드 추가 (이미 컴포넌트에 연결되어 있는 경우)
```csharp
// Unity Button OnClick -> ScheduleNodeButton.OnClick()
public void OnClick() {
    // 내부에서 scheduleUI.AddTempNode(nodeType)를 호출
}
```

- Confirm 연결 예시
```csharp
// Confirm 버튼 OnClick -> ScheduleUI.ConfirmSchedule()
scheduleUI.ConfirmSchedule();
```

- 특정 요일 직접 설정(예시 호출)
```csharp
// ScheduleManager는 씬에 있는 Schedule 컴포넌트 인스턴스
scheduleManager.SetSchedule(0, ScheduleNode.Streaming); // 1일차에 Streaming 설정
```

---

필요하시면 이 문서를 기반으로 다음을 추가로 만들어드리겠습니다:
- `RefreshUI()`를 실제 UI 텍스트/아이콘과 연결하는 `ScheduleSlot` 컴포넌트 및 예제 프리팹
- `SetTempAt(int dayIndex, ScheduleNode)` 같은 편집용 API와 관련 UI(슬롯 클릭으로 덮어쓰기)
- Confirm 시 이전 스케줄 백업/Undo 구현

원하시는 확장 기능을 알려주시면 바로 구현해 드리겠습니다.