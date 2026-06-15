using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleSceneManager : MonoBehaviour
{
    public Button startButton;
    public Button quitButton;
    public string mainSceneName = "MainScene";

    void Awake()
    {
        if (startButton) startButton.onClick.AddListener(OnStartClicked);
        if (quitButton) quitButton.onClick.AddListener(OnQuitClicked);
    }

    void OnStartClicked()
    {
        SceneManager.LoadScene(mainSceneName);
    }

    void OnQuitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
