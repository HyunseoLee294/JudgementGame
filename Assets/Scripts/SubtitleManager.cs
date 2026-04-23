using UnityEngine;
using TMPro;
using System.Collections;


public class SubtitleManager : MonoBehaviour
{
    public AudioSource recorderAudio;
    public TextMeshProUGUI subtitleText;
    public SubtitleData subtitleData;

    public GameObject recorderPanel;
    public float judgmentFeedbackDuration = 1.5f;


    private int currentIndex = -1;
    private Coroutine temporaryMessageRoutine;

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
        if (temporaryMessageRoutine != null) return;

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

    public IEnumerator ShowTemporaryMessage(string message)
    {
        if (subtitleText == null) yield break;

        if (temporaryMessageRoutine != null)
        {
            StopCoroutine(temporaryMessageRoutine);
        }

        temporaryMessageRoutine = StartCoroutine(ShowTemporaryMessageRoutine(message));
        yield return temporaryMessageRoutine;
    }

    IEnumerator ShowTemporaryMessageRoutine(string message)
    {
        subtitleText.gameObject.SetActive(true);
        subtitleText.text = message;
        currentIndex = -1;

        yield return new WaitForSeconds(judgmentFeedbackDuration);

        temporaryMessageRoutine = null;
        HideSubtitle();
    }
}