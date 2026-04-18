using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;  // 다른 스크립트에서 쉽게 접근

    public SubtitleData subtitleData;
    public AudioSource mainAudio;
    public AudioSource unlockSfx;
    public TextMeshProUGUI unlockNotificationText;
    public float notificationDuration = 2f;
    public Recorder recorder;
    private HashSet<int> unlockedSections = new HashSet<int>();
    public DialogueDisplay dialogueDisplay;
    public PlaybackBarDisplay playbackBarDisplay;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 기본 해금 구간 등록
        foreach (var section in subtitleData.sections)
        {
            if (section.unlockedByDefault)
            {
                unlockedSections.Add(section.sectionId);
            }
        }
    }

    public void UnlockSection(int sectionId)
    {
        if (unlockedSections.Contains(sectionId)) return;

        unlockedSections.Add(sectionId);

        // 해금된 구간 찾기
        DialogueSection unlockedSection = null;
        foreach (var section in subtitleData.sections)
        {
            if (section.sectionId == sectionId)
            {
                unlockedSection = section;
                break;
            }
        }

        if (unlockedSection != null)
        {
            recorder.CancelCurrentRoutines();
            StartCoroutine(UnlockRoutine(unlockedSection));
        }
    }

    IEnumerator UnlockRoutine(DialogueSection section)
    {
        // 효과음 재생
        unlockSfx.Play();
        dialogueDisplay.RefreshDialogue();
        playbackBarDisplay.RefreshHatchMarks();

        // 알림 텍스트 표시
        unlockNotificationText.text = section.sectionName + " 발견";
        unlockNotificationText.gameObject.SetActive(true);

        // 오디오 재생 위치 점프
        mainAudio.Stop();
        mainAudio.time = section.startTime;
        mainAudio.Play();

        // 일정 시간 후 알림 숨기기
        yield return new WaitForSeconds(notificationDuration);
        unlockNotificationText.gameObject.SetActive(false);
    }

    public bool IsSectionUnlocked(int sectionId)
    {
        return unlockedSections.Contains(sectionId);
    }

    public bool IsTimeUnlocked(float time)
    {
        foreach (var section in subtitleData.sections)
        {
            bool afterStart = time >= section.startTime;
            bool beforeEnd = section.endTime < 0 || time < section.endTime;
            // endTime < 0 이면 오디오 끝까지

            if (afterStart && beforeEnd)
            {
                return IsSectionUnlocked(section.sectionId);
            }
        }
        return false;
    }

    // 주어진 시간 이후에 해금된 구간이 있으면 그 시작 시간 리턴
    // 없으면 -1 리턴
    public float GetNextUnlockedTime(float currentTime)
    {
        float nextTime = -1f;

        foreach (var section in subtitleData.sections)
        {
            if (!IsSectionUnlocked(section.sectionId)) continue;
            if (section.startTime <= currentTime) continue;

            // 가장 가까운 해금 구간 찾기
            if (nextTime < 0 || section.startTime < nextTime)
            {
                nextTime = section.startTime;
            }
        }

        return nextTime;
    }
}