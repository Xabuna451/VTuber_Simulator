using UnityEngine;

public class VTuber : MonoBehaviour
{
    public static VTuber Instance { get; private set; }
    public VTuberState state = new VTuberState();


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);   // 게임 전역 유지 원하면
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        state.InitIfNeeded();   // 초기화 함수
    }

    void Start()
    {
        //charInfo.GetComponent<CharInfo>().SetCharInfo(state.profile.vtuberName, state.profile.gender, state.profile.age);
    }

}
