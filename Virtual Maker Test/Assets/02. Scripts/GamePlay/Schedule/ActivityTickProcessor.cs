using UnityEngine;

public class ActivityTickProcessor : MonoBehaviour
{
    [SerializeField] private Schedule schedule;
    ScheduleNode todayNode;

    [System.Obsolete]
    void Awake()
    {
        if (schedule == null) { schedule = FindObjectOfType<Schedule>().GetComponent<Schedule>(); }
    }

    // 스트리밍 활동 시 틱당(1시간) VTuber 상태 변화 적용
    public void ApplyStreamingTick(VTuberCondition c, float mood)
    {
        float s = c.stress / 100f;
        float f = c.fatigue / 100f;
        float h = c.happiness / 100f;
        float he = c.health / 100f;

        // 1) Stress
        float dStress = 4.0f
                      + 6.0f * f
                      - 5.0f * (h - 0.5f);

        if (c.stress > 70f)
            dStress += 2.0f;

        //    여기 추가: 기본값에 랜덤 배율 곱하기
        //    지금 상태(20,20,80) 기준으로 1~2 근처 나오도록 0.3~0.6 배
        float stressScale = Random.Range(0.3f, 0.6f);
        dStress *= stressScale;
        //    여기까지

        // 2) Fatigue
        float dFatigue = 5.0f
                       + 5.0f * s;

        // 3) Happiness
        float dHappy = 4.0f * (mood - 0.5f) * 2.0f;

        if (c.stress > 70f) dHappy -= 4.0f;
        else if (c.stress > 50f) dHappy -= 2.0f;

        // 4) Health
        float dHealth = -(2.0f * f + 1.5f * s);

        if (h > 0.6f)
        {
            float happyNorm = (h - 0.6f) / 0.4f;   // 0~1
            dHealth += 1.5f * happyNorm;
        }

        c.stress = Mathf.Clamp(c.stress + dStress, 0f, 100f);
        c.fatigue = Mathf.Clamp(c.fatigue + dFatigue, 0f, 100f);
        c.happiness = Mathf.Clamp(c.happiness + dHappy, 0f, 100f);
        c.health = Mathf.Clamp(c.health + dHealth, 0f, 100f);

        // 디버그 찍어서 밸런스 확인
        Debug.Log($"[StreamingTick] dS={dStress:F2}, dF={dFatigue:F2}, dHap={dHappy:F2}, dHe={dHealth:F2}  => S={c.stress:F1}, F={c.fatigue:F1}, H={c.happiness:F1}, He={c.health:F1}");
    }

    public void ApplyRestTick(VTuberCondition c)
    {
        float s = c.stress / 100f;  // 0~1
        float f = c.fatigue / 100f;
        float h = c.happiness / 100f;
        float he = c.health / 100f;

        // 1) Stress 감소
        // 피로/스트레스 높을수록 더 많이 깎임
        float dStress = -(3.0f + 4.0f * s + 2.0f * f);    // 대략 -3 ~ -9 근처
        float stressScale = Random.Range(0.7f, 1.2f);     // 약간 랜덤
        dStress *= stressScale;

        // 2) Fatigue 감소 (핵심 회복)
        float dFatigue = -(4.0f + 6.0f * f);              // 대략 -4 ~ -10
        float fatigueScale = Random.Range(0.8f, 1.3f);
        dFatigue *= fatigueScale;

        // 3) Happiness
        // 많이 지쳐있고 스트레스 높을수록 "휴식의 행복"이 커짐
        float tiredFactor = Mathf.Max(s, f);            // 둘 중 큰 쪽
        float baseHappyGain = 1.0f + 3.0f * tiredFactor;  // 1~4
        float dHappy = baseHappyGain;

        // 너무 건강/행복이 이미 높으면 효과 조금 줄이기 (심심한 휴식)
        if (he > 0.8f && h > 0.7f)
            dHappy *= 0.5f;

        float happyScale = Random.Range(0.8f, 1.2f);
        dHappy *= happyScale;

        // 4) Health
        // 피로/스트레스 높을수록 회복량↑, 행복도 좋으면 추가 버프
        float dHealth = 2.0f + 3.0f * f + 2.0f * s;       // 최소 +2, 최댓값 대략 +7
        if (h > 0.6f)
        {
            float happyNorm = (h - 0.6f) / 0.4f;          // 0~1
            dHealth += 1.5f * happyNorm;
        }
        float healthScale = Random.Range(0.8f, 1.2f);
        dHealth *= healthScale;

        // 실제 반영 + 클램프
        c.stress = Mathf.Clamp(c.stress + dStress, 0f, 100f);
        c.fatigue = Mathf.Clamp(c.fatigue + dFatigue, 0f, 100f);
        c.happiness = Mathf.Clamp(c.happiness + dHappy, 0f, 100f);
        c.health = Mathf.Clamp(c.health + dHealth, 0f, 100f);

        // 디버그 찍어서 밸런스 확인
        Debug.Log($"[RestTick] dS={dStress:F2}, dF={dFatigue:F2}, dHap={dHappy:F2}, dHe={dHealth:F2}  => S={c.stress:F1}, F={c.fatigue:F1}, H={c.happiness:F1}, He={c.health:F1}");
    }

}