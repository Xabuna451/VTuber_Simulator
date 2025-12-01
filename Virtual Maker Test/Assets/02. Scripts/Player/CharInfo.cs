using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CharInfo : MonoBehaviour
{
    bool isInfoVisible = false;
    public Text textName;
    public Text textGender;
    public Text textAge;

    public void OnClickInfo()
    {
        Debug.Log("캐릭터 정보 클릭됨");
        SetCharInfo();
        isInfoVisible = !isInfoVisible;
        gameObject.SetActive(isInfoVisible);
    }
    public void SetCharInfo()
    {
        textName.text = "이름: " + VTuber.Instance.state.profile.vtuberName;
        textGender.text = "성별: " + GenderCheck();
        textAge.text = "나이: " + VTuber.Instance.state.profile.age.ToString();
    }

    string GenderCheck()
    {
        if (VTuber.Instance.state.profile.gender == Gender.Female ? true : false)
        {
            return "여자";
        }
        else
        {
            return "남자";
        }
    }
}
