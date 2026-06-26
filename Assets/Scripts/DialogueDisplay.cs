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
    // 한 번이라도 재생된 줄의 인덱스를 영구 보관 — RefreshDialogue()에서 절대 초기화하지 않음
    private HashSet<int> revealedLines = new HashSet<int>();

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

            if (revealedLines.Contains(i))
            {
                // 이미 재생된 줄: 정상 표시
                tmp.text = line.speaker + "      " + line.text;
            }
            else
            {
                // 아직 재생 안 된 줄: 마커로 가림
                tmp.text = "???   <mark=#FFFFFFFF>" + line.text + "</mark>";
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

        // 재생 헤드가 새 줄에 처음 진입하는 순간, 해당 섹션이 해금된 경우에만 마커를 영구 해제
        if (currentIndex >= 0 && !revealedLines.Contains(currentIndex)
            && GameManager.Instance.IsTimeUnlocked(subtitleData.lines[currentIndex].startTime))
        {
            revealedLines.Add(currentIndex);
            if (currentIndex < lineTexts.Count)
            {
                SubtitleLine line = subtitleData.lines[currentIndex];
                lineTexts[currentIndex].text = line.speaker + "      " + line.text;
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
