using System.Collections.Generic;
using UnityEngine;
using TMPro;



public class SubtitleManager : MonoBehaviour
{
    public AudioSource recorderAudio;
    public TextMeshProUGUI subtitleText;
    public SubtitleData subtitleData;

    public GameObject recorderPanel;

    private int currentIndex = -1;

    void Update()
    {
        // 녹음기 UI가 열려있으면 자막 숨기기
        if (recorderPanel.activeSelf)
        {
            subtitleText.text = "";
            currentIndex = -1; // 리셋
            return;
        }
        
        if (!recorderAudio.isPlaying)
        {
            subtitleText.text = "";
            return;
        }

        float currentTime = recorderAudio.time;

        // 지금 시간에 해당하는 자막 찾기
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

        // 자막이 바뀌었을 때만 업데이트
        if (newIndex != currentIndex)
        {
            currentIndex = newIndex;
            if (newIndex >= 0)
            {
                SubtitleLine line = subtitleData.lines[newIndex];
                subtitleText.text = line.speaker + ": " + line.text;
            }
            else
            {
                subtitleText.text = "";
            }
        }
    }
}