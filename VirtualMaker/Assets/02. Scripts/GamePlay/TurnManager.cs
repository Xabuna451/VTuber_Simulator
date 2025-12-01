using UnityEngine;
using System.Collections;

/// <summary>
/// 2025-11-24 작성
/// 2025-11-25 수정
/// 
/// 턴 관리 매니저 싱글톤.
/// 게임 전반의 턴 진행을 관리.
/// 턴 진행은 Schedule → DayCycle → Schedule(다음 주) 순환.
/// 
/// Schedule: 플레이어가 스케줄을 설정하고 Confirm을 누를 때까지 대기.
///          (플레이어 입력 기다리는 상태 유지)
/// 
/// DayCycle: 1일차 ~ 7일차 순차 처리.
///          각 일차 사이 또는 특정 시점에 이벤트 발생 가능.
///          - 자동 처리 이벤트: 자동으로 처리 후 계속 진행
///          - 선택형 이벤트: 플레이어 선택(수락/거절 등) 대기 후 진행
///
/// 자세한건 TurnManager_설명서.md 문서 참고.
/// </summary>
/// 작성자: 김준희

public enum TurnPhase
{
    None,
    Schedule,      // 스케줄 설정 대기 중
    DayCycle,      // 1~7일차 순차 처리 (이벤트 포함)
}

// 이벤트 타입 구분
public enum EventType
{
    None,
    Auto,          // 자동 처리 이벤트 (로그만 출력 후 자동 진행)
    Choice,        // 선택 요구 이벤트 (플레이어가 선택할 때까지 대기)
}

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public TurnPhase currentPhase = TurnPhase.Schedule;

    // 현재 진행 중인 일자 (0 = 1일차, 6 = 7일차)
    [SerializeField] private int currentDay = 0;

    // 스케줄 진행 시간 / 기본 8시간
    // Tick 단위로 시간 관리. 예: 1 Tick = 1시간
    [SerializeField] private int currentTickTime = 0;

    // 현재 진행 중인 주차 (1 = 1주차, 2 = 2주차, ...)
    // 턴 마다 하나 씩 증가 
    [SerializeField] private int currentWeek = 1;
    [SerializeField] private int endWeek = 52; // 1년 게임 종료

    [Header("참조")]
    [SerializeField] private Schedule schedule;
    [SerializeField] private ActivityTickProcessor deltaStats;
    [SerializeField] private EventManager eventManager;

    // 싱글턴 패턴
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

    void Start()
    {
        // 시작시 Schedule 단계로 진입
        EnterSchedulePhase();
    }

    // ==================== Schedule 단계 ====================
    // Schedule 단계에 진입 (플레이어 입력 대기)
    private void EnterSchedulePhase()
    {
        currentPhase = TurnPhase.Schedule;
        Debug.Log("\n=== Schedule 단계 시작 ===");
        Debug.Log($"현재 {currentWeek}주차");
        Debug.Log("플레이어가 스케줄을 설정할 때까지 대기 중...");
        // UI에서 ScheduleUI.ConfirmSchedule()가 호출되면 EnterDayCyclePhase()가 자동으로 불려짐
    }

    // ==================== DayCycle 단계 ====================
    // DayCycle 단계 진입 (ScheduleUI.ConfirmSchedule()에서 호출)
    public void EnterDayCyclePhase()
    {
        currentPhase = TurnPhase.DayCycle;
        currentDay = 0;
        Debug.Log("=== DayCycle 단계 시작 ===");
        StartCoroutine(ProcessDayCycle());
    }

    private IEnumerator ProcessDayCycle()
    {
        // 7일을 순차적으로 처리
        for (currentDay = 0; currentDay < 7; currentDay++)
        {
            Debug.Log($"\n--- {currentDay + 1}일차 처리 중 ---");

            // 해당 요일 활동 처리 (Schedule에서 설정한 활동 실행)
            yield return StartCoroutine(ProcessDayActivity(currentDay));

            // 임의 시점에 이벤트 발생 가능 (예: 3일차에 갑자기 논란 발생)
            // 여기서 CheckForRandomEvent()를 호출하거나, 다른 시스템에서 TriggerEvent() 호출 가능
            if (ShouldEventOccur(currentDay))
            {
                yield return StartCoroutine(HandleEvent(currentDay));
            }

            yield return new WaitForSeconds(1f); // 시각적 피드백용
        }

        Debug.Log("\n=== DayCycle 완료 ===");

        // 턴이 끝 났으니 주차 증가
        currentWeek++;

        // 7일 처리 완료 후 다시 Schedule로 돌아감 (다음 주)
        EnterSchedulePhase();
    }

    private IEnumerator ProcessDayActivity(int dayIndex)
    {
        var state = VTuber.Instance.state;
        var cond = state.condition;
        var today = schedule.weeklySchedule[dayIndex];

        switch (today)
        {
            case ScheduleNode.Streaming:
                while (currentTickTime < 8)
                {
                    currentTickTime++;
                    float mood = Random.Range(0.6f, 0.9f); // 임시
                    deltaStats.ApplyStreamingTick(cond, mood);
                    yield return new WaitForSeconds(0.5f);
                }
                currentTickTime = 0;
                yield return new WaitForSeconds(0.5f);
                break;

            case ScheduleNode.Rest:
                while (currentTickTime < 8)
                {
                    currentTickTime++;
                    deltaStats.ApplyRestTick(cond);
                    yield return new WaitForSeconds(0.5f);
                }
                currentTickTime = 0;
                yield return new WaitForSeconds(0.5f);
                break;

                // Training / Social 은 나중에
        }

        yield return null;
    }


    // 해당 일차에 이벤트가 발생할지 판정 (확률 또는 사전 설정된 일정)
    private bool ShouldEventOccur(int dayIndex)
    {
        // 예: 30% 확률로 이벤트 발생
        // 실제 프로젝트에서는 더 복잡한 로직(민심, 스탯, 특정 조건 등) 사용
        return Random.value < 0.3f;
    }

    // 이벤트 처리 (자동/선택형 분기)
    private IEnumerator HandleEvent(int dayIndex)
    {
        if (eventManager == null)
        {
            Debug.LogWarning("[TurnManager] EventManager가 할당되어 있지 않습니다.");
            yield break;
        }

        Debug.Log($"\n  >> {currentWeek}주차 {dayIndex + 1}일차: 이벤트 체크");

        // 1) 오늘 사용할 이벤트 하나 고르기
        StreamingEvent ev = eventManager.PickRandomEvent(dayIndex);

        if (ev == null)
        {
            Debug.Log("  >> 조건에 맞는 이벤트 없음 (스킵)");
            yield break;
        }

        // 2) 이벤트 정보 로그
        Debug.Log($"  >> [이벤트 발생] {ev.eventName}");
        Debug.Log(ev.description);

        // 3) 이벤트 타입에 따라 처리
        if (ev.triggerType == EventTriggerType.Auto)
        {
            // 자동 이벤트: 바로 적용하고 잠깐 연출 딜레이
            eventManager.ApplyEvent(ev);
            yield return new WaitForSeconds(2f);
        }
        else
        {
            // 선택형 이벤트: 선택 UI 코루틴으로 넘기기
            // 일단은 TODO로 두고 나중에 구현해도 됨
            yield return StartCoroutine(HandleChoiceEvent(ev));
        }
    }

    IEnumerator HandleChoiceEvent(StreamingEvent ev)
    {
        // TODO: 선택형 이벤트 UI 띄우고 플레이어 선택 대기
        Debug.Log("  >> [선택형 이벤트] 플레이어 선택 대기 중... (TODO 구현 필요)");
        return null;
    }

    IEnumerator HandleDeltaStats()
    {
        while (currentTickTime < 8)
        {
            currentTickTime++;

            deltaStats.ApplyStreamingTick(VTuber.Instance.state.condition, VTuber.Instance.state.condition.mood);
            yield return new WaitForSeconds(0.5f); // 시각적 피드백용
        }
        currentTickTime = 0;
        yield return new WaitForSeconds(0.5f);
    }


    // ==================== 외부 호출 인터페이스 ====================
    // ScheduleUI.ConfirmSchedule()에서 호출 (기존과 유사하지만 메서드명 변경)
    public void OnScheduleConfirmed()
    {
        if (currentPhase == TurnPhase.Schedule)
        {
            Debug.Log("스케줄 확정됨 → DayCycle 단계로 진입");
            EnterDayCyclePhase();
        }
    }

}
