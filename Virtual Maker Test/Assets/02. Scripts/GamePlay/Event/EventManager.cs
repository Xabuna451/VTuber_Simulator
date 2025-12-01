using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.VirtualTexturing;

public class EventManager : MonoBehaviour
{
    [Header("이벤트 데이터")]
    public EventDatabase database;

    [Header("상태 참조")]
    public Schedule schedule;


    void Awake()
    {
    }

    public StreamingEvent PickRandomEvent(int dayIndex)
    {
        if (database == null || database.events == null || database.events.Length == 0)
            return null;

        if (schedule == null)
            return null;

        // 오늘 요일의 활동 확인 (Streaming, Training, Rest, Social 등)
        ScheduleNode todayActivity = schedule.weeklySchedule[dayIndex];

        var candidates = new List<StreamingEvent>();

        foreach (var ev in database.events)
        {
            if (ev == null) continue;

            // "스트리밍 중에만 발생" 옵션이 켜져 있으면 필터
            if (ev.onlyDuringStreaming && todayActivity != ScheduleNode.Streaming)
                continue;

            // 확률 체크 (0~1)
            if (Random.value > ev.probability)
                continue;

            candidates.Add(ev);
        }

        if (candidates.Count == 0)
            return null;

        // 후보 중 하나 랜덤 선택
        int idx = Random.Range(0, candidates.Count);
        return candidates[idx];
    }


    public void ApplyEvent(StreamingEvent ev)
    {
        if (ev == null || VTuber.Instance.state == null) return;

        // 컨디션 변화
        VTuber.Instance.state.condition.stress += ev.deltaStress;
        VTuber.Instance.state.condition.fatigue += ev.deltaFatigue;
        VTuber.Instance.state.condition.happiness += ev.deltaHappiness;
        VTuber.Instance.state.condition.health += ev.deltaHealth;

        // 팔로워 변화
        VTuber.Instance.state.followers.youtubeSubscribers += ev.deltaYouTubeSubs;
        VTuber.Instance.state.followers.instagramFollowers += ev.deltaInstagramFollowers;
        VTuber.Instance.state.followers.channelSubscribers += ev.deltaChannelSubs;
        VTuber.Instance.state.followers.averageViewers += ev.deltaAverageViewers;

        // 능력치 변화
        VTuber.Instance.state.stats.morality += ev.deltaMorality;
        VTuber.Instance.state.stats.charm += ev.deltaCharm;
        VTuber.Instance.state.stats.intelligence += ev.deltaIntelligence;

        // 게임 스탯 변화 예 : 돈, 명성 등
        VTuber.Instance.state.managerState.money += ev.deltaMoney;
        VTuber.Instance.state.managerState.fame += ev.deltaFame;
        VTuber.Instance.state.managerState.reputation += ev.deltaReputation;
        VTuber.Instance.state.managerState.agencyPower += ev.deltaAgencyPower;

        // 변화된 값들 클램핑

        ClampCondition(VTuber.Instance.state.condition);
        ClampStats(VTuber.Instance.state.stats);

        Debug.Log($"[이벤트 적용] {ev.eventName}");

    }

    void ClampCondition(VTuberCondition cond)
    {
        cond.stress = Mathf.Clamp(cond.stress, 0f, 100f);
        cond.fatigue = Mathf.Clamp(cond.fatigue, 0f, 100f);
        cond.happiness = Mathf.Clamp(cond.happiness, 0f, 100f);
        cond.health = Mathf.Clamp(cond.health, 0f, 100f);
    }

    void ClampStats(VTuberStats stats)
    {
        stats.morality = Mathf.Clamp(stats.morality, 0, 999);
        stats.charm = Mathf.Clamp(stats.charm, 0, 999);
        stats.intelligence = Mathf.Clamp(stats.intelligence, 0, 999);
    }


}
