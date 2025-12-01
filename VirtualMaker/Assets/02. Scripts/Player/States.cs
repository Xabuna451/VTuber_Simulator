using UnityEngine;

/// <summary>
/// 2025-11-24 작성
/// VTuber의 상태를 나타내는 클래스, 구조체들을 정의함.
/// 우선 VTuberState에서 VTuber의 프로필, 외부 지표(구독자 수 등), 내부 지표(스트레스, 피로도 등), 평판 시스템을 포함함.
/// 각 지표들은 별도의 클래스로 분리하여 관리함.
/// 
/// VTuber 한 명의 전체 상태를 표현하는 루트 데이터 모델.
/// 프로필, 외부 지표(구독자/팔로워/시청자), 내부 컨디션, 능력치, 방송 민심을 포함한다.
/// 게임 진행 중 Save/Load의 기본 단위가 된다.
/// 
/// 자세한건 States_설명서.md 문서 참고.
/// </summary>
[System.Serializable]
public class VTuberState
{
    [Header("[프로필] 이름, 성별, 나이")]
    public VTuberProfile profile;
    [Header("[외부 지표] 구독자 수, 팔로워 수, 평균 시청자 수")]
    public VTuberFollowers followers;
    [Header("[내부 컨디션] 스트레스, 피로도, 행복도, 건강도")]
    public VTuberCondition condition;
    [Header("[능력치] 도덕성, 매력, 지능")]
    public VTuberStats stats;
    [Header("[관리자 상태] 게임 머니, 명성, 평판, 소속사 영향력")]
    public ManagerState managerState;
    [Header("[방송 민심] 시청자들의 VTuber에 대한 호감도")]
    public 방송민심 민심;

    public VTuberState()
    {
        InitIfNeeded();
    }

    public void InitIfNeeded()
    {
        if (profile == null) profile = new VTuberProfile();
        if (followers == null) followers = new VTuberFollowers();
        if (condition == null) condition = new VTuberCondition();
        if (stats == null) stats = new VTuberStats();
    }
}

[System.Serializable]
public class VTuberProfile
{
    public string vtuberName;
    public Gender gender;
    public int age;
}

[System.Serializable]
public class VTuberCondition
{
    public float stress;     // 스트레스 0~100
    public float fatigue;    // 피로도 0~100
    public float happiness;  // 행복도 0~100
    public float health;     // 건강도 0~100
    public float mood;       // 방송 분위기 0~100 (높을수록 긍정적)
}

[System.Serializable]
public class VTuberFollowers
{
    public int youtubeSubscribers;   // 유튜브 구독자 수
    public int instagramFollowers;   // 인스타 팔로워 수
    public int channelSubscribers;   // 생방송 플랫폼 구독자 수
    public int averageViewers;       // 평균 동시 시청자 수
}

[System.Serializable]
public class VTuberStats
{
    public int morality;     // 도덕성 0~999
    public int charm;        // 매력 0~999
    public int intelligence; // 지능 0~999

    // TODO: 필요하면 singing, talkSkill, gameSense 같은 실전 능력 추가
}

[System.Serializable]
public class ManagerState
{
    public int money;           // 게임 머니. 장비 구매/훈련/이벤트 비용에 사용    
    public int fame;            // 업계에서의 명성. 광고/콜라보/스폰서 발생에 영향을 줌
    public int reputation;      // 업계 평판. NPC 관계, 회사 이벤트, 협력 노선에 영향
    public int agencyPower;     // 소속사/팀의 영향력. 확장된 경제 시스템에서 사용
}

public enum Gender
{
    None = 0,
    Male = 1,
    Female = 2,
    Other = 3
}

public enum 방송민심
{
    나락 = 0,
    비호감 = 1,
    중립 = 2,
    호감 = 3,
    찬양 = 4,
}
