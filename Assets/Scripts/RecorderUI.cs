using UnityEngine;

public class RecorderUI : MonoBehaviour
{
    public GameObject recorderPanel;
    public AudioSource recorderAudio;

    public void Open()
    {
        recorderPanel.SetActive(true);

        // 아직 재생 중이 아니면 재생 시작
        if (!recorderAudio.isPlaying)
        {
            recorderAudio.Play();
        }
    }

    public void Close()
    {
        recorderPanel.SetActive(false);
    }

    void Update()
    {
        if (recorderPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }
}
