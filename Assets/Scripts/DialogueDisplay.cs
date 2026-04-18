using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueDisplay : MonoBehaviour
{
    public Transform contentParent;    // Scroll View의 Content
    public GameObject dialogueLinePrefab;
    public SubtitleData subtitleData;
    public AudioSource recorderAudio;

    private List<TextMeshProUGUI> lineTexts = new List<TextMeshProUGUI>();
    private int lastHighlightedIndex = -1;

    public void RefreshDialogue()
    {
        // 기존 줄 전부 삭제
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        lineTexts.Clear();

        // 대사 줄 생성
        for (int i = 0; i < subtitleData.lines.Count; i++)
        {
            SubtitleLine line = subtitleData.lines[i];
            GameObject lineObj = Instantiate(dialogueLinePrefab, contentParent);
            TextMeshProUGUI tmp = lineObj.GetComponent<TextMeshProUGUI>();

            if (GameManager.Instance.IsTimeUnlocked(line.startTime))
            {
                // 해금된 대사: 정상 표시
                tmp.text = line.speaker + "      " + line.text;
            }
            else
            {
                // 해금 안 된 대사: ??? + 검정 네모
                tmp.text = "???   <mark=#000000>" + line.text + "</mark>";
            }

            lineTexts.Add(tmp);
        }
    }

    void Update()
    {
        if (!recorderAudio.isPlaying) return;
        if (lineTexts.Count == 0) return;

        // 현재 재생 중인 대사 하이라이트
        float currentTime = recorderAudio.time;
        int currentIndex = -1;

        for (int i = 0; i < subtitleData.lines.Count; i++)
        {
            if (currentTime >= subtitleData.lines[i].startTime)
            {
                currentIndex = i;
            }
            else
            {
                break;
            }
        }

        if (currentIndex != lastHighlightedIndex)
        {
            // 이전 하이라이트 해제
            if (lastHighlightedIndex >= 0 && lastHighlightedIndex < lineTexts.Count)
            {
                lineTexts[lastHighlightedIndex].color = Color.white;
            }

            // 새 하이라이트
            if (currentIndex >= 0 && currentIndex < lineTexts.Count)
            {
                lineTexts[currentIndex].color = Color.yellow;
            }

            lastHighlightedIndex = currentIndex;
        }
    }
}
