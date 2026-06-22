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
        // MainScene에서 DontDestroyOnLoad로 넘어온 엔딩 BGM 오브젝트를 정지 후 제거
        var ddolScene = gameObject.scene; // TitleScene
        foreach (var audio in FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
        {
            if (audio.gameObject.scene != ddolScene && audio.isPlaying)
            {
                audio.Stop();
                Destroy(audio.gameObject);
            }
        }
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
