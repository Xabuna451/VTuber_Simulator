using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    [SerializeField] public Button button;
    [SerializeField] public Text text;

    [Header("인스펙터에서 씬 이름 지정")]
    [SerializeField] public string scene;

    void Awake()
    {
        button = GetComponentInChildren<Button>();
        text = GetComponentInChildren<Text>();
        button.onClick.AddListener(OnButtonClick);
        text.text = "게임 시작";
    }

    void OnButtonClick()
    {
        SceneManager.LoadScene(scene);
    }
}
