using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleSceneManager : MonoBehaviour
{
    public Button startButton;
    public Button quitButton;
    public string mainSceneName = "MainScene";
    public AudioSource bgmAudio;

    void Awake()
    {
        if (startButton) startButton.onClick.AddListener(OnStartClicked);
        if (quitButton) quitButton.onClick.AddListener(OnQuitClicked);
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if (!Cursor.visible || Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void OnStartClicked()
    {
        if (bgmAudio != null && bgmAudio.isPlaying)
            bgmAudio.Stop();
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
