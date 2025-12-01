using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 2025-11-25 작성
/// 스케줄 클래스
/// Schedule 턴일 때 플레이어가 일주일 스케줄을 설정하고 관리하는 역할.
/// 작성자: 김준희
/// </summary>

public class Schedule : MonoBehaviour
{
    // 일주일 스케줄을 저장하는 리스트 (7일치)
    public List<ScheduleNode> weeklySchedule = new List<ScheduleNode>();

    private const int DaysPerWeek = 7;

    private void Awake()
    {
        // 초기화: 기본값으로 None으로 채우기
        while (weeklySchedule.Count < DaysPerWeek)
        {
            weeklySchedule.Add(ScheduleNode.None);
        }
    }

    // 플레이어가 클릭을 해서 특정 요일의 스케줄을 설정하는 함수
    public void SetSchedule(int dayIndex, ScheduleNode activity)
    {
        if (dayIndex < 0 || dayIndex >= DaysPerWeek)
        {
            Debug.LogError("잘못된 날짜 인덱스입니다: " + dayIndex);
            return;
        }

        // 해당 요일의 스케줄을 설정
        if (weeklySchedule.Count > dayIndex)
        {
            weeklySchedule[dayIndex] = activity;
        }
        else
        {
            // 리스트가 아직 충분히 길지 않은 경우 확장
            while (weeklySchedule.Count <= dayIndex)
            {
                weeklySchedule.Add(ScheduleNode.None);
            }
            weeklySchedule[dayIndex] = activity;
        }
    }

    /// <summary>
    /// 한 번에 주간 스케줄 전체를 설정
    /// 리스트 길이는 최대 7까지만 적용됨
    /// 부족한 날은 None으로 채움
    /// </summary>
    public void SetWeeklySchedule(List<ScheduleNode> newSchedule)
    {
        if (newSchedule == null)
        {
            Debug.LogError("SetWeeklySchedule: newSchedule is null");
            return;
        }

        weeklySchedule.Clear();

        for (int i = 0; i < DaysPerWeek; i++)
        {
            if (i < newSchedule.Count)
            {
                weeklySchedule.Add(newSchedule[i]);
            }
            else
            {
                weeklySchedule.Add(ScheduleNode.None);
            }
        }
    }

    public List<ScheduleNode> GetWeeklySchedule()
    {
        // 반환시 원본을 건드리지 않도록 새 리스트로 복사해서 반환
        return new List<ScheduleNode>(weeklySchedule);
    }

    // 턴이 끝나면 초기화
    public void ClearWeeklySchedule()
    {
        weeklySchedule.Clear();
        while (weeklySchedule.Count < DaysPerWeek)
        {
            weeklySchedule.Add(ScheduleNode.None);
        }
    }
}
