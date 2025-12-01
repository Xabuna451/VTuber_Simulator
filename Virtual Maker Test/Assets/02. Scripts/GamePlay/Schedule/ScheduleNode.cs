using UnityEngine;

/// <summary>
/// 2025-11-25 작성
/// 스케줄 노드 열거형.
/// Schedule 턴일 때 일주일에 어떤 활동을 할지 선택하는 데 사용.
/// 작성자: 김준희
/// </summary>
public enum ScheduleNode
{
    None,
    Streaming,
    Training,
    Rest,
    Social,
}

public class ScheduleNodeButton : MonoBehaviour
{
    public ScheduleNode nodeType;
    [SerializeField] private ScheduleUI scheduleUI;

    // Unity UI Button의 OnClick에 안전하게 연결하려면 반환형을 void로 만듭니다.
    // (UnityEvent는 반환값을 무시하지만, 에디터에서 직관적으로 연결하기 위해 void를 권장)
    public void OnClick()
    {
        // 스케줄 UI에서 이 노드를 클릭했을 때 처리할 로직 작성
        //Debug.Log("스케줄 노드 클릭됨: " + nodeType);

        // ScheduleUI를 Inspector에서 할당하거나(권장), 자동으로 찾아서 사용합니다.
        if (scheduleUI == null)
        {
            // 최신 Unity API 권장 방식 사용
            scheduleUI = UnityEngine.Object.FindAnyObjectByType<ScheduleUI>();
        }

        if (scheduleUI != null)
        {
            int addedIndex = scheduleUI.AddTempNode(nodeType);
            if (addedIndex < 0)
            {
                Debug.LogWarning("임시 스케줄이 가득 찼습니다. 먼저 삭제하거나 확정하세요.");
            }
            else
            {
                //Debug.Log($"노드가 임시 슬롯 {addedIndex + 1}일차에 추가되었습니다.");
            }
        }
        else
        {
            Debug.LogWarning("ScheduleUI가 씬에 존재하지 않습니다. ScheduleUI 컴포넌트를 배치하거나 ScheduleNodeButton의 scheduleUI 필드에 할당하세요.");
        }
    }

    // 필요하면 코드에서 노드 타입을 얻어올 수 있도록 액세서 제공
    public ScheduleNode GetNodeType()
    {
        return nodeType;
    }
}
