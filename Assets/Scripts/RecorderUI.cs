using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecorderUI : MonoBehaviour
{
    public GameObject recorderPanel;
    public GameObject crosshair;
    public GameObject player;

    public AudioSource recorderAudio;
    public Slider playbackSlider;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI unlockCountText;
    private bool isDragging = false;
    public DialogueDisplay dialogueDisplay;
    public PlaybackBarDisplay playbackBarDisplay;


    public void Open()
    {
        if (JudgeManager.Instance != null && JudgeManager.Instance.IsGameplayBlocked()) return;

        if (recorderPanel) recorderPanel.SetActive(true);
        if (crosshair) crosshair.SetActive(false);
        CursorController.Unlock();
        SetPlayerControlEnabled(false);
        if (dialogueDisplay) dialogueDisplay.RefreshDialogue();
        if (playbackBarDisplay) playbackBarDisplay.RefreshHatchMarks();
    }

    public void Close()
    {
        if (recorderPanel) recorderPanel.SetActive(false);
        if (crosshair) crosshair.SetActive(true);
        CursorController.Lock();
        SetPlayerControlEnabled(true);
    }

    void SetPlayerControlEnabled(bool enabled)
    {
        if (!player) return;
        var fpc = player.GetComponent<StarterAssets.FirstPersonController>();
        if (fpc) fpc.enabled = enabled;
    }

    void Update()
    {
        if (!recorderPanel) return;

        if (recorderPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }

        if (!recorderPanel.activeSelf) return;

        UpdatePlaybackUI();
        UpdateUnlockCount();
    }

    void UpdatePlaybackUI()
    {
        if (!recorderAudio || recorderAudio.clip == null) return;

        float currentTime = recorderAudio.time;
        float totalTime = recorderAudio.clip.length;

        // 드래그 중이 아닐 때만 슬라이더 위치 갱신
        if (!isDragging && playbackSlider)
        {
            playbackSlider.value = currentTime / totalTime;
        }

        // 시간 텍스트 갱신
        if (timeText)
        {
            timeText.text = FormatTime(currentTime) + " / " + FormatTime(totalTime);
        }
    }

    void UpdateUnlockCount()
    {
        if (!unlockCountText) return;
        if (GameManager.Instance == null || GameManager.Instance.subtitleData == null) return;

        int unlocked = 0;
        int total = GameManager.Instance.subtitleData.sections.Count;

        foreach (var section in GameManager.Instance.subtitleData.sections)
        {
            if (GameManager.Instance.IsSectionUnlocked(section.sectionId))
            {
                unlocked++;
            }
        }

        unlockCountText.text = unlocked + "/" + total + " 구간 해금";
    }

    string FormatTime(float seconds)
    {
        int min = (int)(seconds / 60);
        int sec = (int)(seconds % 60);
        return min.ToString("00") + ":" + sec.ToString("00");
    }

    // 슬라이더 드래그 시작 (EventTrigger에서 호출)
    public void OnSliderDragStart()
    {
        isDragging = true;
    }

    // 슬라이더 드래그 끝 (EventTrigger에서 호출)
    public void OnSliderDragEnd()
    {
        isDragging = false;

        if (!recorderAudio || recorderAudio.clip == null) return;
        if (!playbackSlider) return;

        float targetTime = playbackSlider.value * recorderAudio.clip.length;

        // 해금된 구간인지 확인
        if (GameManager.Instance != null && GameManager.Instance.IsTimeUnlocked(targetTime))
        {
            recorderAudio.time = targetTime;
        }
        else
        {
            // 해금 안 된 구간이면 원래 위치로 되돌리기
            playbackSlider.value = recorderAudio.time / recorderAudio.clip.length;
        }
    }
}
