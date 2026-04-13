using UnityEngine;

public class RecorderUI : MonoBehaviour
{
    public GameObject recorderPanel;
    public AudioSource recorderAudio;
    public GameObject crosshair;

    public void Open()
    {
        recorderPanel.SetActive(true);
        crosshair.SetActive(false);  // 점 숨기기

        // 아직 재생 중이 아니면 재생 시작
        if (!recorderAudio.isPlaying)
        {
            recorderAudio.Play();
        }
    }

    public void Close()
    {
        recorderPanel.SetActive(false);
        crosshair.SetActive(true);  // 점 다시 보이기
    }

    void Update()
    {
        if (recorderPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }
}
