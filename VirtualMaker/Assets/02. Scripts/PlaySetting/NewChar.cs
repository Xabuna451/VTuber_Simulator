using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 캐릭터 생성 흐름 관리:
/// 이름 -> 성별 -> 나이 -> 확인 -> 완료(씬 이동)
/// </summary>
public enum CharSettingStep
{
    Name,
    Gender,
    Age,
    Confirm,
    Complete
}

public class NewCharController : MonoBehaviour
{
    [Header("패널들")]
    [SerializeField] private GameObject panelName;
    [SerializeField] private GameObject panelGender;
    [SerializeField] private GameObject panelAge;
    [SerializeField] private GameObject panelConfirm;

    [Header("이름 입력")]
    [SerializeField] private InputField inputName;

    [Header("나이 드롭다운")]
    [SerializeField] private Dropdown ageDropdown;

    [Header("확인 패널 UI")]
    [SerializeField] private Text txtSummary;

    [Header("게임 시작 씬 이름")]
    [SerializeField] private string gameSceneName = "GameScene";

    // 현재 단계
    private CharSettingStep currentStep = CharSettingStep.Name;
    // 캐릭터 생성 중 임시로 들고 있을 프로필 데이터
    private VTuberProfile profile = new VTuberProfile();

    void Start()
    {
        profile.gender = Gender.None;
        profile.age = 0;
        RefreshUI();
    }

    // ================== 단계 전환 공통 처리 ==================

    void SetStep(CharSettingStep step)
    {
        currentStep = step;
        RefreshUI();
    }

    void RefreshUI()
    {
        panelName.SetActive(currentStep == CharSettingStep.Name);
        panelGender.SetActive(currentStep == CharSettingStep.Gender);
        panelAge.SetActive(currentStep == CharSettingStep.Age);
        panelConfirm.SetActive(currentStep == CharSettingStep.Confirm);

        if (currentStep == CharSettingStep.Confirm)
        {
            txtSummary.text =
                $"이름: {profile.vtuberName}\n" +
                $"성별: {GetGenderKorean(profile.gender)}\n" +
                $"나이: {profile.age}\n\n" +
                "이 정보가 맞습니까?";
        }

        if (currentStep == CharSettingStep.Complete)
        {
            OnComplete();
        }
    }

    string GetGenderKorean(Gender g)
    {
        switch (g)
        {
            case Gender.Male: return "남자";
            case Gender.Female: return "여자";
            case Gender.Other: return "기타";
            default: return "미설정";
        }
    }

    // ================== 이름 단계 ==================

    // "이름 → 다음" 버튼에 연결
    public void OnClickNameNext()
    {
        string name = inputName.text.Trim();

        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("[NewChar] 이름이 비어있음");
            return;
        }

        profile.vtuberName = name;
        SetStep(CharSettingStep.Gender);
    }

    // ================== 성별 단계 ==================

    // "남자" 버튼에 연결
    public void OnClickSelectMale()
    {
        profile.gender = Gender.Male;
        SetStep(CharSettingStep.Age);
    }

    // "여자" 버튼에 연결
    public void OnClickSelectFemale()
    {
        profile.gender = Gender.Female;
        SetStep(CharSettingStep.Age);
    }

    // 필요하면 기타 버튼도 추가 가능
    public void OnClickSelectOther()
    {
        profile.gender = Gender.Other;
        SetStep(CharSettingStep.Age);
    }

    // ================== 나이 단계 (드롭다운) ==================

    // "나이" -> "다음" 버튼에 연결
    public void OnClickAgeNext()
    {
        if (!ageDropdown)
        {
            Debug.LogError("[NewChar] ageDropdown이 할당되지 않음");
            return;
        }

        var option = ageDropdown.options[ageDropdown.value];

        if (!int.TryParse(option.text, out int age))
        {
            Debug.LogWarning($"[NewChar] 나이 파싱 실패: {option.text}");
            return;
        }

        profile.age = age;
        SetStep(CharSettingStep.Confirm);
    }

    // ================== 확인 단계 ==================

    // "예" 버튼에 연결
    public void OnClickConfirmYes()
    {
        SetStep(CharSettingStep.Complete);
    }

    // "아니오" 버튼에 연결 (처음으로 되돌림)
    public void OnClickConfirmNo()
    {
        SetStep(CharSettingStep.Name);
    }

    // ================== 완료 / 게임 시작 ==================

    void OnComplete()
    {
        SaveToVTuberState();
        SceneManager.LoadScene(gameSceneName);
    }

    void SaveToVTuberState()
    {
        if (!VTuber.Instance)
        {
            Debug.LogError("[NewChar] VTuber.Instance 없음. VTuber 오브젝트가 씬에 있는지 확인.");
            return;
        }

        var s = VTuber.Instance.state;
        s.InitIfNeeded();

        s.profile.vtuberName = profile.vtuberName;
        s.profile.gender = profile.gender;
        s.profile.age = profile.age;

        // 여기서 시작 기본값 세팅 (원하면 수정)
        s.followers.youtubeSubscribers = 0;
        s.followers.instagramFollowers = 0;
        s.followers.channelSubscribers = 0;
        s.followers.averageViewers = 0;

        s.condition.stress = 0f;
        s.condition.fatigue = 0f;
        s.condition.happiness = 50f;
        s.condition.health = 100f;

        s.stats.morality = 0;
        s.stats.charm = 0;
        s.stats.intelligence = 0;

        s.민심 = 방송민심.중립;

        Debug.Log($"[NewChar] VTuberState 저장 완료: {s.profile.vtuberName}/{s.profile.gender}/{s.profile.age}");
    }
}
