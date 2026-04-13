using UnityEngine;

public class RecorderUI : MonoBehaviour
{
    public GameObject recorderPanel;

    public void Open()
    {
        recorderPanel.SetActive(true);
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
