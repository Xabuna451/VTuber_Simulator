using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SaveLoadButton : MonoBehaviour
{
    [SerializeField] public Button button;
    [Header("인스펙터에서 씬 이름 지정")]
    [SerializeField] public string scene;
    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }

    void Start()
    {
    }

    void OnButtonClick()
    {
        SceneManager.LoadScene(scene);
    }
}
