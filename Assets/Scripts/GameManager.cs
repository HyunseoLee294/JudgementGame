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

    private int lastReportedSection = -1;
    private Coroutine notificationRoutine;

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

    void Update()
    {
        if (mainAudio == null || !mainAudio.isPlaying) return;

        float t = mainAudio.time;
        foreach (var section in subtitleData.sections)
        {
            bool inSection = t >= section.startTime &&
                             (section.endTime < 0 || t < section.endTime);
            if (!inSection) continue;
            if (!IsSectionUnlocked(section.sectionId)) return;

            float end = section.endTime < 0
                ? (mainAudio.clip != null ? mainAudio.clip.length : section.startTime)
                : section.endTime;

            if (t >= end - 0.05f && lastReportedSection != section.sectionId)
            {
                lastReportedSection = section.sectionId;
                if (JudgeManager.Instance != null)
                {
                    JudgeManager.Instance.NotifySectionFirstPlayed(section.sectionId);
                }
            }
            return;
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
            if (JudgeManager.Instance != null)
            {
                JudgeManager.Instance.RegisterUnlock(sectionId);
            }

            if (recorder != null) recorder.CancelCurrentRoutines();
            StartCoroutine(UnlockRoutine(unlockedSection));
        }
    }

    IEnumerator UnlockRoutine(DialogueSection section)
    {
        // 효과음 재생
        if (unlockSfx != null) unlockSfx.Play();
        if (dialogueDisplay != null) dialogueDisplay.RefreshDialogue();
        if (playbackBarDisplay != null) playbackBarDisplay.RefreshHatchMarks();

        // 알림 텍스트 표시
        ShowNotification(section.sectionName + " 발견");

        // 오디오 재생 위치 점프
        if (mainAudio != null)
        {
            mainAudio.Stop();
            mainAudio.time = section.startTime;
            mainAudio.Play();
        }
        yield break;
    }

    // 화면 상단(또는 설정된 위치)에 알림 문구를 notificationDuration초 동안 표시
    public void ShowNotification(string text)
    {
        if (unlockNotificationText == null) return;

        unlockNotificationText.text = text;
        unlockNotificationText.gameObject.SetActive(true);

        if (notificationRoutine != null) StopCoroutine(notificationRoutine);
        notificationRoutine = StartCoroutine(HideNotificationAfterDelay());
    }
    
    IEnumerator HideNotificationAfterDelay()
    {
        yield return new WaitForSeconds(notificationDuration);
        if (unlockNotificationText != null)
        {
            unlockNotificationText.gameObject.SetActive(false);
        }
        notificationRoutine = null;
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