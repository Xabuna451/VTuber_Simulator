using UnityEngine;
using System.Collections.Generic;

public class ScheduleUI : MonoBehaviour
{
    bool isInfoVisible = false;
    private const int MaxDays = 7;

    // 임시 저장 칸: 사용자가 UI에서 선택한 노드를 순서대로 보관
    // 사용자가 '확정'을 누르면 이 버퍼 내용이 실제 Schedule에 적용
    private List<ScheduleNode> tempSchedule = new List<ScheduleNode>();

    [Header("레퍼런스 (Inspector에서 할당)")]
    [SerializeField] private Schedule scheduleManager;


    public void OnClick()
    {
        isInfoVisible = !isInfoVisible;
        gameObject.SetActive(isInfoVisible);
    }
    // 읽기 전용으로 현재 임시 스케줄을 반환
    public IReadOnlyList<ScheduleNode> GetTempSchedule()
    {
        return tempSchedule.AsReadOnly();
    }

    // 다음에 추가될 임시 슬롯의 인덱스 (0-based). UI에서 "다음에 채워질 요일" 표시 시 사용하세요.
    public int NextTempIndex => tempSchedule.Count;

    // 임시 스케줄에 노드를 추가합니다. 최대 MaxDays 까지만 허용.
    // 반환값: 추가 성공 여부
    // 노드를 추가하고 추가된 인덱스를 반환합니다. 실패 시 -1을 반환합니다.
    public int AddTempNode(ScheduleNode node)
    {
        if (tempSchedule.Count >= MaxDays)
        {
            return -1;
        }

        tempSchedule.Add(node);
        int addedIndex = tempSchedule.Count - 1;
        Debug.Log($"임시 스케줄에 추가: {node} (index {addedIndex})");
        RefreshUI();
        return addedIndex;
    }

    // 마지막에 추가한 임시 노드를 제거합니다.
    public bool RemoveLastTemp()
    {
        if (tempSchedule.Count == 0) return false;
        var removed = tempSchedule[tempSchedule.Count - 1];
        tempSchedule.RemoveAt(tempSchedule.Count - 1);
        Debug.Log($"임시 스케줄에서 제거: {removed}");
        RefreshUI();
        return true;
    }

    // UI 버튼 등에서 호출할 수 있는 함수
    public void RemoveLast()
    {
        RemoveLastTemp();
    }

    public void ClearTemp()
    {
        tempSchedule.Clear();
        Debug.Log("임시 스케줄 초기화됨");
        RefreshUI();
    }

    // 임시 스케줄을 실제 Schedule에 확정 적용합니다. Schedule은 static API를 사용합니다.
    public void ConfirmSchedule()
    {
        if (scheduleManager == null)
        {
            // 자동 탐색은 fallback일 뿐, Inspector로 할당하는 것을 권장합니다.
            scheduleManager = FindAnyObjectByType<Schedule>();
            if (scheduleManager == null)
            {
                Debug.LogError("ConfirmSchedule 실패: Scene에 Schedule 매니저가 없습니다. Schedule 컴포넌트를 가진 GameObject를 배치하고 ScheduleUI에 할당하세요.");
                return;
            }
        }
        if (tempSchedule.Count + 1 <= MaxDays)
        {
            // 임시에서 7일을 다 안채우면 리턴
            Debug.LogWarning("임시 스케줄이 7일치로 완성되지 않았습니다. 7일치를 모두 채운 후 확정하세요.");
            return;
        }
        scheduleManager.SetWeeklySchedule(tempSchedule);
        Debug.Log("임시 스케줄을 Schedule에 확정했습니다.");

        // 스케줄 확정 → DayCycle 단계로 진입
        TurnManager.Instance.OnScheduleConfirmed();
    }

    // UI를 갱신하는 훅: 실제 프로젝트에서는 이곳에서 슬롯 표시, 버튼 색상 변경 등을 처리하세요.
    private void RefreshUI()
    {
        // TODO: UI 컴포넌트와 연동하여 시각적으로 표시
        //Debug.Log("임시 스케줄 갱신. 현재: " + (tempSchedule.Count == 0 ? "(비어있음)" : string.Join(", ", tempSchedule)));
    }
}
