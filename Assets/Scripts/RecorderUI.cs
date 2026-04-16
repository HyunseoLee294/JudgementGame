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
