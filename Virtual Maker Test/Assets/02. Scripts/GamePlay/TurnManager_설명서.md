# TurnManager 턴 관리 시스템 설명서

이 문서는 `TurnManager` 스크립트의 구조, 역할, 메서드, 사용법을 자세히 설명합니다. 게임 전체의 턴 진행(Schedule → DayCycle → Schedule 반복) 및 이벤트 시스템을 이해하는 데 필수 문서입니다.

---

## 1. 개요

### 목적

VTuber 시뮬레이션 게임에서 한 주(7일) 단위의 턴을 관리하는 싱글톤 매니저입니다.

### 게임 흐름도

```
[Schedule 단계]
    ↓
플레이어가 스케줄 설정 및 Confirm 버튼 클릭
    ↓
[DayCycle 단계]
    ↓
1일차 → 2일차 → ... → 7일차 (순차 처리)
각 일차 중간에 이벤트 발생 가능 (자동/선택형)
    ↓
[7일 완료]
    ↓
[Schedule 단계] (다음 주)
```

### 핵심 특징

- **상태 기반 턴 관리:** 게임이 항상 `Schedule` 또는 `DayCycle` 중 하나의 단계에만 존재.
- **Coroutine 기반 순차 처리:** DayCycle은 코루틴으로 구현되어 1~7일을 순서대로 처리하고 진행 상황을 시각적으로 피드백 가능.
- **이벤트 중단점:** 각 일차 중간에 이벤트가 발생하면 자동 처리 또는 플레이어 선택 대기.
- **느슨한 결합:** `ScheduleUI`, 이벤트 시스템, VTuberState 등이 `TurnManager`의 공개 인터페이스(`OnScheduleConfirmed`, `TriggerEventAtCurrentDay`)를 통해 호출.

---

## 2. 열거형(Enum)

### TurnPhase

게임이 취할 수 있는 턴 단계(상태).

```csharp
public enum TurnPhase
{
    None,          // 정의되지 않은 상태 (일반적으로 사용 안 함)
    Schedule,      // 스케줄 설정 단계 (플레이어 입력 대기)
    DayCycle,      // 1~7일차 순차 처리 단계 (이벤트 포함)
}
```

- **Schedule:** 플레이어가 1주일 스케줄을 설정하고 `Confirm`을 누를 때까지 이 상태 유지.
- **DayCycle:** `Confirm` 클릭 후 자동으로 1~7일을 처리. 이벤트가 발생할 수 있으며, 7일 완료 후 다시 `Schedule`로.

### EventType

이벤트의 처리 방식을 구분.

```csharp
public enum EventType
{
    None,          // 이벤트 없음
    Auto,          // 자동 처리 이벤트 (UI 선택 불필요, 로그/상태만 변경 후 자동 진행)
    Choice,        // 선택형 이벤트 (플레이어 선택 필요, 선택창 표시 후 대기)
}
```

- **Auto:** 예) 논란 발생 → 민심 자동 감소 → 진행 계속
- **Choice:** 예) 협찬 제의 → 플레이어가 수락/거절 선택 → 선택에 따라 보상/패널티 적용

---

## 3. 주요 필드

### 공개 필드 (Public)

```csharp
public static TurnManager Instance { get; private set; }
```

- 싱글톤 인스턴스. 어디서든 `TurnManager.Instance`로 접근 가능.

```csharp
public TurnPhase currentPhase = TurnPhase.Schedule;
```

- 현재 게임이 처한 턴 단계. 외부에서 읽기 전용으로 사용(상태 확인용).

### 비공개 필드 (Private)

```csharp
private int currentDay = 0;
```

- 현재 처리 중인 일자 (0-based). 0 = 1일차, 6 = 7일차.
- DayCycle 진행 중에만 유의미한 값.

---

## 4. 싱글톤 및 초기화

### Awake()

```csharp
void Awake()
{
    if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    else
    {
        Destroy(gameObject);
        return;
    }
}
```

- 표준 싱글톤 패턴.
- 게임 시작 시 첫 `TurnManager` 찾은 후, 이후 추가로 생성되는 `TurnManager`는 파괴.
- `DontDestroyOnLoad` 설정으로 씬 전환 후에도 유지.

### Start()

```csharp
void Start()
{
    EnterSchedulePhase();
}
```

- 게임 시작 시 `Schedule` 단계로 진입.

---

## 5. 단계별 메서드 상세 설명

### 5.1 Schedule 단계

#### EnterSchedulePhase() [Private]

```csharp
private void EnterSchedulePhase()
{
    currentPhase = TurnPhase.Schedule;
    Debug.Log("=== Schedule 단계 시작 ===");
    Debug.Log("플레이어가 스케줄을 설정할 때까지 대기 중...");
}
```

- **역할:** Schedule 단계로 진입하고 상태 초기화.
- **호출 시점:**
  1. 게임 시작 시 (`Start()`에서)
  2. DayCycle 완료 후 (다음 주 스케줄 설정 대기)
- **동작:**
  - `currentPhase`를 `TurnPhase.Schedule`로 설정.
  - 디버그 로그 출력.
  - UI에서 스케줄 버튼이 활성화됨 (ScheduleUI가 이 상태를 감지해 UI 표시).
  - 플레이어가 노드를 1~7개 클릭하고 `Confirm` 버튼을 누를 때까지 대기.

#### OnScheduleConfirmed() [Public]

```csharp
public void OnScheduleConfirmed()
{
    if (currentPhase == TurnPhase.Schedule)
    {
        Debug.Log("스케줄 확정됨 → DayCycle 단계로 진입");
        EnterDayCyclePhase();
    }
}
```

- **역할:** Schedule 단계 완료 신호. 외부에서 호출하는 진입점.
- **호출 위치:** `ScheduleUI.ConfirmSchedule()` 마지막 부분.
- **호출 흐름:**
  1. 플레이어가 Confirm 버튼 클릭.
  2. `ScheduleUI.ConfirmSchedule()` 실행.
  3. `TurnManager.Instance.OnScheduleConfirmed()` 호출.
  4. `EnterDayCyclePhase()` 자동 실행.
- **조건 확인:** 현재 `Schedule` 단계인지 먼저 확인 (안전 체크).

---

### 5.2 DayCycle 단계

#### EnterDayCyclePhase() [Public]

```csharp
public void EnterDayCyclePhase()
{
    currentPhase = TurnPhase.DayCycle;
    currentDay = 0;
    Debug.Log("=== DayCycle 단계 시작 ===");
    StartCoroutine(ProcessDayCycle());
}
```

- **역할:** DayCycle 단계 진입 및 코루틴 시작.
- **호출 시점:** `OnScheduleConfirmed()`에서만 호출됨.
- **동작:**
  1. `currentPhase`를 `TurnPhase.DayCycle`로 변경.
  2. `currentDay`를 0으로 초기화 (1일차부터 시작).
  3. 메인 루프 코루틴 `ProcessDayCycle()` 시작.

#### ProcessDayCycle() [Private Coroutine]

```csharp
private IEnumerator ProcessDayCycle()
{
    for (currentDay = 0; currentDay < 7; currentDay++)
    {
        Debug.Log($"\n--- {currentDay + 1}일차 처리 중 ---");

        // 1. 해당 요일 활동 처리
        yield return StartCoroutine(ProcessDayActivity(currentDay));

        // 2. 이벤트 발생 확률 체크 및 처리
        if (ShouldEventOccur(currentDay))
        {
            yield return StartCoroutine(HandleEvent(currentDay));
        }

        // 3. 시각적 피드백용 지연
        yield return new WaitForSeconds(1f);
    }

    // 7일 모두 처리 완료
    Debug.Log("\n=== DayCycle 완료 ===");

    // 다음 주 스케줄 단계로
    EnterSchedulePhase();
}
```

- **역할:** 1~7일을 순차적으로 처리하는 메인 루프.
- **흐름:**
  1. **루프:** 각 일차마다 다음 3단계 실행.
     - 요일별 활동 처리 (`ProcessDayActivity`)
     - 이벤트 발생 체크 및 처리 (`ShouldEventOccur`, `HandleEvent`)
     - UI 업데이트 시간 확보용 1초 지연
  2. **완료:** 7일 모두 처리 후 `EnterSchedulePhase()`로 돌아감 (다음 주 준비).
- **중요:** 이 코루틴은 동기적으로 작동하므로, 각 이벤트나 활동의 `yield return`으로 인해 자동으로 다음 단계를 기다림.

#### ProcessDayActivity(int dayIndex) [Private Coroutine]

```csharp
private IEnumerator ProcessDayActivity(int dayIndex)
{
    // 실제 프로젝트에서: Schedule의 weeklySchedule[dayIndex]에 따라 활동 실행
    // 예: Streaming이면 수익 계산, Training이면 스탯 증가 등
    Debug.Log($"  요일 활동 처리: {dayIndex + 1}일차");
    yield return new WaitForSeconds(0.5f);
}
```

- **역할:** 해당 일차의 스케줄에 따른 활동 실행.
- **매개변수:**
  - `dayIndex` (int): 0-based 일차 인덱스.
- **실제 구현 방향:**

  ```csharp
  // 예제 (실제 프로젝트에서는 VTuberState 업데이트)
  Schedule schedule = /* 참조 획득 */;
  ScheduleNode activity = schedule.weeklySchedule[dayIndex];

  switch (activity)
  {
      case ScheduleNode.Streaming:
          // 수익 계산, 피로도 증가, 행복도 변화 등
          break;
      case ScheduleNode.Training:
          // 스탯 증가, 피로도 증가, 스트레스 감소 등
          break;
      case ScheduleNode.Rest:
          // 피로도 감소, 건강도 증가 등
          break;
      case ScheduleNode.Social:
          // 팬 충성도 증가, 스트레스 감소 등
          break;
  }
  ```

- **현재 상태:** 예제만 구현되어 있음. 실제 로직은 별도 시스템(VTuberController, StateManager 등)에서 구현 필요.

---

### 5.3 이벤트 시스템

#### ShouldEventOccur(int dayIndex) [Private]

```csharp
private bool ShouldEventOccur(int dayIndex)
{
    // 예: 30% 확률로 이벤트 발생
    return Random.value < 0.3f;
}
```

- **역할:** 해당 일차에 이벤트가 발생할지 판정.
- **반환값:** `true`면 이벤트 발생, `false`면 이벤트 스킵.
- **현재 구현:** 30% 고정 확률.
- **확장 가능한 조건:**
  ```csharp
  private bool ShouldEventOccur(int dayIndex)
  {
      // 민심이 높을수록 더 자주 긍정 이벤트 발생
      float mentalWeight = GetVTuberMental(); // 예: 0~1 범위

      // 특정 요일에만 이벤트 (예: 주말)
      if (dayIndex == 5 || dayIndex == 6) return Random.value < 0.5f;

      // 기본: 민심에 따라 변동
      return Random.value < (0.2f + mentalWeight * 0.3f);
  }
  ```

#### HandleEvent(int dayIndex) [Private Coroutine]

```csharp
private IEnumerator HandleEvent(int dayIndex)
{
    Debug.Log($"\n  >> {dayIndex + 1}일차에 이벤트 발생!");

    // 이벤트 타입 결정 (실제: 데이터 드리븐)
    EventType eventType = Random.value < 0.5f ? EventType.Auto : EventType.Choice;

    if (eventType == EventType.Auto)
    {
        yield return StartCoroutine(HandleAutoEvent(dayIndex));
    }
    else
    {
        yield return StartCoroutine(HandleChoiceEvent(dayIndex));
    }
}
```

- **역할:** 발생한 이벤트의 타입을 결정하고 처리 분기.
- **로직:**
  1. 이벤트 발생 로그 출력.
  2. 이벤트 타입 결정 (현재: 50% Auto, 50% Choice).
  3. 타입에 따라 처리 코루틴 호출.
- **확장 가능:**
  ```csharp
  // 민심에 따라 이벤트 타입 결정
  float mentalWeight = GetVTuberMental();
  EventType eventType;
  if (mentalWeight > 0.7f)
      eventType = Random.value < 0.7f ? EventType.Choice : EventType.Auto;
  else
      eventType = Random.value < 0.5f ? EventType.Auto : EventType.Choice;
  ```

#### HandleAutoEvent(int dayIndex) [Private Coroutine]

```csharp
private IEnumerator HandleAutoEvent(int dayIndex)
{
    Debug.Log($"  [자동 이벤트] 논란 발생: 민심이 1단계 떨어짐");
    // 실제 로직: VTuberState 컨디션 변경 등
    yield return new WaitForSeconds(2f);
    Debug.Log($"  [자동 이벤트] 처리 완료, 계속 진행");
}
```

- **역할:** 자동으로 처리되는 이벤트 실행.
- **특징:**
  - 플레이어 선택/입력 없음.
  - 상태 변경만 수행 (예: 민심 감소, 스트레스 증가).
  - 2초 대기 후 다음 단계로 진행.
- **구현 예제:**
  ```csharp
  private IEnumerator HandleAutoEvent(int dayIndex)
  {
      // 1. 이벤트 내용 표시 (UI 또는 로그)
      string eventText = "논란 발생: 부적절한 발언으로 팬들의 신뢰 하락";
      Debug.Log($"  [자동 이벤트] {eventText}");

      // 2. VTuberState 업데이트
      VTuberState state = GetCurrentVTuberState();
      state.민심 = Math.Max(state.민심 - 1, 0);
      state.condition.happiness -= 10f;

      // 3. UI 표시 (선택사항)
      // UIManager.ShowEventNotification(eventText, 2f);

      yield return new WaitForSeconds(2f);
      Debug.Log($"  [자동 이벤트] 처리 완료, 계속 진행");
  }
  ```

#### HandleChoiceEvent(int dayIndex) [Private Coroutine]

```csharp
private IEnumerator HandleChoiceEvent(int dayIndex)
{
    Debug.Log($"  [선택 이벤트] 협찬 제의가 들어왔습니다!");
    Debug.Log($"  선택 대기 중... (실제 구현시 UI 선택창 표시)");

    // 실제 구현: UI 버튼 클릭 대기 또는 타임아웃
    yield return new WaitForSeconds(3f);

    Debug.Log($"  [선택 이벤트] 플레이어 선택: 수락 (예제)");
    // 실제 로직: 선택에 따른 상태 변경
}
```

- **역할:** 플레이어 선택을 요구하는 이벤트 실행.
- **특징:**
  - UI 선택 창 표시 필요.
  - 플레이어의 선택(수락/거절 등)을 기다림.
  - 선택에 따라 다른 상태 변경.
- **개선된 구현 (콜백 방식):**
  ```csharp
  private IEnumerator HandleChoiceEvent(int dayIndex)
  {
      string eventDescription = "협찬 제의: 유명 브랜드에서 협찬을 제안했습니다.";
      Debug.Log($"  [선택 이벤트] {eventDescription}");

      // 1. UI 선택 창 표시
      EventChoiceUI choiceUI = Instantiate(choiceUIPrefab);
      choiceUI.Show(
          eventDescription,
          new[] { "수락 (보상 +100)", "거절 (보상 없음)" }
      );

      // 2. 플레이어 선택 대기
      int playerChoice = -1;
      choiceUI.OnChoiceSelected += (choice) => playerChoice = choice;

      while (playerChoice == -1)
      {
          yield return new WaitForSeconds(0.1f);
      }

      // 3. 선택에 따른 처리
      VTuberState state = GetCurrentVTuberState();
      if (playerChoice == 0)
      {
          state.followers.youtubeSubscribers += 100;
          Debug.Log($"  [선택 이벤트] 협찬 수락 → 구독자 +100");
      }
      else
      {
          Debug.Log($"  [선택 이벤트] 협찬 거절");
      }

      choiceUI.Hide();
      yield return new WaitForSeconds(1f);
  }
  ```

---

## 6. 외부 호출 인터페이스

### OnScheduleConfirmed() [Public]

```csharp
public void OnScheduleConfirmed()
{
    if (currentPhase == TurnPhase.Schedule)
    {
        Debug.Log("스케줄 확정됨 → DayCycle 단계로 진입");
        EnterDayCyclePhase();
    }
}
```

- **호출 위치:** `ScheduleUI.ConfirmSchedule()`
- **용도:** Schedule 단계 완료 신호.
- **안전 조건:** `currentPhase == TurnPhase.Schedule` 체크.

### TriggerEventAtCurrentDay(string eventDescription) [Public]

```csharp
public void TriggerEventAtCurrentDay(string eventDescription)
{
    Debug.Log($"외부 이벤트 발생 요청: {eventDescription} (at {currentDay + 1}일차)");
    // 필요시 현재 DayCycle 코루틴에 영향 주기 (고급 구현)
}
```

- **역할:** 외부 시스템에서 중간에 이벤트를 강제 발생시키는 진입점.
- **호출 위치:** 스토리 진행, 특정 조건 감지 등에서.
- **예제:**
  ```csharp
  // 예: 팬 댓글이 너무 많으면 즉시 논란 발생
  if (commentCount > 1000)
  {
      TurnManager.Instance.TriggerEventAtCurrentDay("과도한 팬 댓글로 인한 커뮤니티 혼란");
  }
  ```
- **현재 상태:** 스텁(stub) 형태. 실제 구현 시 고급 코루틴 제어 필요.

---

## 7. 턴 흐름 다이어그램 (상세)

```
┌─────────────────────────────────────────────────────────────────┐
│ 게임 시작 (Awake → Start)                                        │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
              ┌─────────────────────────┐
              │ EnterSchedulePhase()    │
              │ currentPhase.Schedule   │
              └────────────┬────────────┘
                           │
        ┌──────────────────┴──────────────────┐
        │ (플레이어 스케줄 설정 UI 표시)      │
        │ (1~7개 노드 클릭 후 Confirm 대기)   │
        │                                     │
        │ ScheduleUI.ConfirmSchedule()        │
        │         ↓                           │
        │ TurnManager.OnScheduleConfirmed()   │
        └──────────────────┬──────────────────┘
                           │
                           ▼
              ┌──────────────────────────────┐
              │ EnterDayCyclePhase()         │
              │ StartCoroutine(ProcessD...) │
              │ currentPhase = DayCycle      │
              └───────────────┬──────────────┘
                              │
                              ▼
              ┌────────────────────────────┐
              │ ProcessDayCycle() Loop     │
              └───────────┬────────────────┘
                          │
                          ▼
              ┌─────────────────────────────────────┐
              │ for (currentDay = 0; day < 7; day++) │
              └────────────┬────────────────────────┘
                           │
         ┌─────────────────┴─────────────────┐
         │                                   │
         ▼                                   ▼
    ┌──────────────────┐         ┌───────────────────┐
    │ProcessDayActivity│         │ShouldEventOccur() │
    │(활동 처리)       │         │(이벤트 확률 체크) │
    └────────┬─────────┘         └────────┬──────────┘
             │                            │
             │                    ┌───────▼──────────┐
             │                    │ true? 이벤트 발생 │
             │                    └────────┬─────────┘
             │                             │
             │                    ┌────────▼────────────┐
             │                    │ HandleEvent()      │
             │                    └────────┬────────────┘
             │                             │
             │                ┌────────────┴────────────┐
             │                │                         │
             │                ▼                         ▼
             │          ┌──────────────┐        ┌──────────────────┐
             │          │ HandleAuto.. │        │ HandleChoice...  │
             │          │(자동 처리)    │        │ (선택형 처리)     │
             │          │2초 대기      │         │ 플레이어 선택 대기│
             │          └──────┬───────┘        └────────┬─────────┘
             │                 │                        │
             └─────────────────┼────────────────────────┘
                               │
                         ┌─────▼────────┐
                         │1초 지연       │
                         │yield return   │
                         └─────┬────────┘
                               │
                    ┌──────────▼──────────┐
                    │ 다음 일차로 loop    │
                    │ (또는 loop 종료)    │
                    └──────────┬──────────┘
                               │
                    (7일 모두 처리 완료)
                               │
                               ▼
                    ┌─────────────────────────┐
                    │ EnterSchedulePhase()    │
                    │ (다음 주 스케줄로)     │
                    └─────────────────────────┘
                               │
                               ▼ (반복)
```

---

## 8. 사용 예시 및 시나리오

### 시나리오 1: 기본 턴 진행

```
게임 시작
  ↓
Schedule 단계: 플레이어가 월~일 스케줄 설정 (7개 노드)
  ↓
Confirm 클릭 → OnScheduleConfirmed() 호출
  ↓
DayCycle 시작
  1일차: Streaming (수익 +100)
  2일차: Training (매력 +5)
  3일차: Rest (피로 -20) → 이벤트 발생! 자동 논란 발생 (민심 -1)
  4일차: Social (스트레스 -10)
  ...
  7일차: Streaming (수익 +100) → 이벤트 발생! 협찬 제의 (플레이어 선택)
         플레이어 선택: 수락 (구독자 +50)
  ↓
DayCycle 종료
  ↓
Schedule 단계 (다음 주)
```

### 시나리오 2: 이벤트 연쇄 처리

```
3일차 처리 중...
  ↓
ShouldEventOccur(2) = true → HandleEvent(2)
  ↓
EventType 결정: Auto
  ↓
HandleAutoEvent(): "악성 댓글 증가로 민심 1단계 감소"
  ↓
2초 대기 후 계속 진행
  ↓
4일차 처리...
```

### 시나리오 3: 외부 시스템 개입

```
DayCycle 진행 중 (5일차)
  ↓
외부 시스템 감지: "유명 유튜버가 언급했습니다!"
  ↓
TriggerEventAtCurrentDay("유명 유튜버 언급") 호출
  ↓
현재 실행 중인 코루틴에 이벤트 중단점 삽입
  ↓
특수 이벤트 처리 (보상 큼)
  ↓
계속 진행
```

---

## 9. 통합 및 확장

### 다른 시스템과의 연동

#### ScheduleUI ↔ TurnManager

```csharp
// ScheduleUI.cs의 ConfirmSchedule()
public void ConfirmSchedule()
{
    // ... 스케줄 확정 로직 ...
    scheduleManager.SetWeeklySchedule(tempSchedule);
    Debug.Log("임시 스케줄을 Schedule에 확정했습니다.");

    // TurnManager에 신호 전달
    TurnManager.Instance.OnScheduleConfirmed();
}
```

#### VTuberState ↔ TurnManager

```csharp
// ProcessDayActivity()에서 호출 (확장 구현)
private IEnumerator ProcessDayActivity(int dayIndex)
{
    Schedule schedule = GetSchedule();
    ScheduleNode activity = schedule.weeklySchedule[dayIndex];
    VTuberState state = GetVTuberState();

    // 활동에 따른 상태 변경
    switch (activity)
    {
        case ScheduleNode.Streaming:
            state.followers.youtubeSubscribers += 50;
            state.condition.fatigue += 15f;
            break;
        // ...
    }

    yield return new WaitForSeconds(0.5f);
}
```

#### 이벤트 시스템 ↔ TurnManager

```csharp
// 외부 이벤트 매니저에서
public void OnSpecialConditionMet()
{
    TurnManager.Instance.TriggerEventAtCurrentDay("특수 이벤트 발생!");
}
```

### 확장 가능한 개선사항

1. **이벤트 데이터 드리븐화:**

   - ScriptableObject로 이벤트 데이터 정의 (이름, 설명, 타입, 영향도)
   - 로드된 데이터로 처리

2. **UI 연결:**

   - 현재 진행 상황(날짜, 활동) 실시간 표시
   - 이벤트 팝업 시스템 통합

3. **선택형 이벤트 고도화:**

   - 여러 선택지 지원 (2개 이상)
   - 선택에 따른 복합 효과

4. **타임라인 저장:**
   - 각 턴의 이벤트/선택사항 기록
   - 리플레이 또는 회고 시스템

---

## 10. 문제 해결 체크리스트

| 증상                            | 원인                                          | 해결                                             |
| ------------------------------- | --------------------------------------------- | ------------------------------------------------ |
| Confirm 누른 후 아무것도 안 됨  | `OnScheduleConfirmed()` 호출 안 됨            | ScheduleUI.ConfirmSchedule()에 호출 코드 추가    |
| 이벤트가 빠르게 지나감          | yield 시간이 너무 짧음                        | `WaitForSeconds` 값 조정 (예: 3f → 5f)           |
| 7일을 다 처리하지 않음          | loop 조건 오류 또는 조기 탈출                 | `for (currentDay = 0; currentDay < 7; ...)` 확인 |
| 선택형 이벤트 선택 대기가 안 됨 | 콜백/플래그 메커니즘 미구현                   | HandleChoiceEvent() 선택 대기 로직 구현 필요     |
| 다음 주로 넘어가지 않음         | DayCycle 후 `EnterSchedulePhase()` 호출 안 됨 | ProcessDayCycle() 마지막 부분 확인               |

---

## 11. 빠른 참조 (Quick Reference)

### 상태 확인

```csharp
if (TurnManager.Instance.currentPhase == TurnPhase.Schedule)
{
    // Schedule 단계 중
}
else if (TurnManager.Instance.currentPhase == TurnPhase.DayCycle)
{
    // DayCycle 단계 중
}
```

### 외부에서 이벤트 발생

```csharp
TurnManager.Instance.TriggerEventAtCurrentDay("카메라 고장!");
```

### 일차 확인 (DayCycle 진행 중)

```csharp
// TurnManager.currentDay는 private이므로 필요시 public property 추가
// public int GetCurrentDay() => currentDay;
```

---

## 12. 작성 정보

- **작성 일자:** 2025-11-25
- **작성자:** 김준희
- **최종 수정:** 2025-11-25
- **관련 파일:**
  - `Assets/02. Scripts/GamePlay/TurnManager.cs` (본체)
  - `Assets/02. Scripts/GamePlay/Schedule/ScheduleUI.cs` (호출 위치)
  - `Assets/02. Scripts/Player/States.cs` (상태 반영)
  - `Assets/02. Scripts/GamePlay/Schedule/Schedule.cs` (스케줄 데이터)

---

이 설명서가 도움이 되길 바랍니다. 추가 질문이나 확장 기능 구현이 필요하면 언제든 문의하세요!
