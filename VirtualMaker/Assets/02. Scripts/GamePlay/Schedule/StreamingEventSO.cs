using UnityEngine;

public enum EventTriggerType
{
    Auto,       // 자동 이벤트
    Choice      // 선택형 이벤트
}

[CreateAssetMenu(fileName = "StreamingEvent", menuName = "Scriptable Objects/Streaming Event")]
public class StreamingEvent : ScriptableObject
{
    [Header("기본 정보")]
    public string eventName;
    [TextArea] public string description;
    public EventTriggerType triggerType;

    [Header("발생 조건")]
    public bool onlyDuringStreaming;     // 스트리밍 상태일 때만 발생
    public int minDay = 0;               // 0 = 월요일, 6 = 일요일, -1 = 제한 없음
    public float probability = 1f;       // 0~1, 발생 확률 가중치

    [Header("상태 변화 (전부 변화량 Δ로 처리)")]
    public float deltaStress;
    public float deltaFatigue;
    public float deltaHappiness;
    public float deltaHealth;

    [Header("팔로워/시청자 변화량")]
    public int deltaYouTubeSubs;
    public int deltaInstagramFollowers;
    public int deltaChannelSubs;
    public int deltaAverageViewers;

    [Header("능력치 변화량")]
    public int deltaMorality;
    public int deltaCharm;
    public int deltaIntelligence;

    [Header("게임 스탯 변화량")]
    public int deltaMoney;
    public int deltaFame;
    public int deltaReputation;
    public int deltaAgencyPower;
}
