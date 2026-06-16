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

    [Header("새 구간 재생 중 안내 텍스트 (녹음기 UI가 열려있으면 숨김)")]
    public TextMeshProUGUI playingNewSectionText;
    public string playingNewSectionMessage = "새로운 섹션 재생 중";
    private HashSet<int> unlockedSections = new HashSet<int>();
    public DialogueDisplay dialogueDisplay;
    public PlaybackBarDisplay playbackBarDisplay;

    private int lastReportedSection = -1;
    private Coroutine notificationRoutine;
    private Coroutine unlockRoutineHandle;

    public bool isPlayingUnlockSection { get; private set; } = false;

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
        UpdatePlayingNewSectionText();

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

    void UpdatePlayingNewSectionText()
    {
        if (playingNewSectionText == null) return;

        bool recorderOpen = recorder != null
            && recorder.recorderUI != null
            && recorder.recorderUI.recorderPanel != null
            && recorder.recorderUI.recorderPanel.activeSelf;

        bool shouldShow = isPlayingUnlockSection && !recorderOpen;

        if (shouldShow)
        {
            if (!playingNewSectionText.gameObject.activeSelf)
            {
                playingNewSectionText.gameObject.SetActive(true);
            }
            playingNewSectionText.text = playingNewSectionMessage;
        }
        else if (playingNewSectionText.gameObject.activeSelf)
        {
            playingNewSectionText.gameObject.SetActive(false);
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
            // 이전 단서의 미리듣기가 아직 끝나지 않았다면 정리하고 새로 시작
            CancelUnlockRoutine();
            unlockRoutineHandle = StartCoroutine(UnlockRoutine(unlockedSection));
        }
    }

    // 단서 미리듣기 중인 UnlockRoutine을 강제로 중단한다.
    // (다른 단서를 곧바로 해금하거나, 엔딩으로 진입할 때 호출)
    public void CancelUnlockRoutine()
    {
        if (unlockRoutineHandle != null)
        {
            StopCoroutine(unlockRoutineHandle);
            unlockRoutineHandle = null;
        }
        isPlayingUnlockSection = false;
    }

    IEnumerator UnlockRoutine(DialogueSection section)
    {
        // 효과음 재생
        if (unlockSfx != null) unlockSfx.Play();
        if (dialogueDisplay != null) dialogueDisplay.RefreshDialogue();
        if (playbackBarDisplay != null) playbackBarDisplay.RefreshHatchMarks();

        // 알림 텍스트 표시
        ShowNotification(section.sectionName + " 발견");

        if (mainAudio == null) yield break;

        mainAudio.Stop();
        mainAudio.time = section.startTime;
        mainAudio.Play();

        isPlayingUnlockSection = true;

        float endTime = section.endTime < 0
            ? (mainAudio.clip != null ? mainAudio.clip.length : section.startTime)
            : section.endTime;

        // 새로 해금된 구간이 끝날 때까지 대기
        while (mainAudio.isPlaying && mainAudio.time < endTime)
            yield return null;

        isPlayingUnlockSection = false;

        // 판단 페이즈가 시작되는 경우에는 오디오를 그대로 둠
        // (TriggerJudgment가 0.3초 뒤 UI를 ForceOpen해서 흐름을 이어감)
        bool judgmentStarting = JudgeManager.Instance != null
            && (JudgeManager.Instance.Phase == GamePhase.Judgment1
             || JudgeManager.Instance.Phase == GamePhase.Judgment2
             || JudgeManager.Instance.Phase == GamePhase.Judgment3
             || JudgeManager.Instance.Phase == GamePhase.Judgment4);

        // 이 구간 뒤에 원래 스킵/되감기 사운드가 재생될 차례였다면, 멈추기 전에 그것까지 재생
        if (!judgmentStarting)
        {
            yield return PlayTrailingSkipOrRewindSfx(endTime);
        }

        bool uiOpen = recorder != null
            && recorder.recorderUI != null
            && recorder.recorderUI.recorderPanel != null
            && recorder.recorderUI.recorderPanel.activeSelf;

        if (!judgmentStarting && !uiOpen && mainAudio.isPlaying)
        {
            mainAudio.Pause();
        }
    }

    // 단서로 해금된 구간 재생이 끝난 시점 기준으로, 평소 녹음기 재생 중이었다면
    // 울렸을 스킵/되감기 사운드를 같은 순서로 재생한다.
    // 재생 위치는 항상 "해금된" 지점에서 멈춰야, 나중에 녹음기를 다시 열었을 때
    // 같은 스킵/되감기 사운드가 중복으로 재생되지 않는다.
    IEnumerator PlayTrailingSkipOrRewindSfx(float sectionEndTime)
    {
        if (recorder == null || mainAudio == null) yield break;

        // 사운드 재생 도중 오디오가 계속 흘러가며 잠긴 구간으로 들어가지 않도록 먼저 멈춤
        mainAudio.Pause();

        bool reachedClipEnd = mainAudio.clip == null || sectionEndTime >= mainAudio.clip.length - 0.05f;

        if (!reachedClipEnd)
        {
            // 뒤에 잠긴 구간이 이어진다 → 스킵 사운드
            if (recorder.skipSfx != null && recorder.skipSfx.clip != null)
            {
                recorder.skipSfx.Play();
                yield return new WaitForSeconds(recorder.skipSfx.clip.length);
            }

            float nextTime = GetNextUnlockedTime(sectionEndTime);
            if (nextTime >= 0)
            {
                // 이어서 들을 수 있는 해금된 구간이 있으면 그 시작점에 정지 (되감기 불필요)
                mainAudio.time = nextTime;
                yield break;
            }
        }

        // 더 들을 구간이 없음(오디오 끝 또는 스킵해도 갈 곳 없음) → 되감기 사운드, 처음으로 되돌림
        if (recorder.rewindSfx != null && recorder.rewindSfx.clip != null)
        {
            recorder.rewindSfx.Play();
            yield return new WaitForSeconds(recorder.rewindSfx.clip.length);
        }
        mainAudio.time = 0f;
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