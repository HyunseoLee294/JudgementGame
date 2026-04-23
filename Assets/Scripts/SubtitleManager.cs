using UnityEngine;
using TMPro;



public class SubtitleManager : MonoBehaviour
{
    public AudioSource recorderAudio;
    public TextMeshProUGUI subtitleText;
    public SubtitleData subtitleData;

    public GameObject recorderPanel;

    private int currentIndex = -1;

    void Start()
    {
        HideSubtitle();
    }
        
    void Update()
    {
        if (JudgeManager.Instance == null || !JudgeManager.Instance.IsEnding())
        {
            HideSubtitle();
            return;
        }

        if (recorderPanel != null && recorderPanel.activeSelf)
        {
            HideSubtitle();
            return;
        }

        if (recorderAudio == null || !recorderAudio.isPlaying)
        {
            HideSubtitle();
            return;
        }

        ShowEndingSubtitle();
    }

    void ShowEndingSubtitle()
    {
        if (subtitleText == null || subtitleData == null) return;

        if (!subtitleText.gameObject.activeSelf)
        {
            subtitleText.gameObject.SetActive(true);
        }

        float currentTime = recorderAudio.time;
        int newIndex = -1;

        for (int i = 0; i < subtitleData.lines.Count; i++)
        {
            if (currentTime >= subtitleData.lines[i].startTime)
            {
                newIndex = i;
            }
            else
            {
                break;
            }
        }

        if (newIndex != currentIndex)
        {
            currentIndex = newIndex;
            subtitleText.text = newIndex >= 0
                ? subtitleData.lines[newIndex].speaker + ": " + subtitleData.lines[newIndex].text
                : "";
        }
    }

    void HideSubtitle()
    {
        if (subtitleText != null)
        {
            subtitleText.text = "";
            if (subtitleText.gameObject.activeSelf)
            {
                subtitleText.gameObject.SetActive(false);
            }
        }

        currentIndex = -1;
    }
}