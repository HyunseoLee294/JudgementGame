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


    public void Open()
    {
        recorderPanel.SetActive(true);
        crosshair.SetActive(false);  // 점 숨기기
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        player.GetComponent<StarterAssets.FirstPersonController>().enabled = false;
    }

    public void Close()
    {
        recorderPanel.SetActive(false);
        crosshair.SetActive(true);  // 점 다시 보이기
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        player.GetComponent<StarterAssets.FirstPersonController>().enabled = true;
    }

    void Update()
    {
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
        if (recorderAudio.clip == null) return;

        float currentTime = recorderAudio.time;
        float totalTime = recorderAudio.clip.length;

        // 드래그 중이 아닐 때만 슬라이더 위치 갱신
        if (!isDragging)
        {
            playbackSlider.value = currentTime / totalTime;
        }

        // 시간 텍스트 갱신
        timeText.text = FormatTime(currentTime) + " / " + FormatTime(totalTime);
    }

    void UpdateUnlockCount()
    {
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

        if (recorderAudio.clip == null) return;

        float targetTime = playbackSlider.value * recorderAudio.clip.length;

        // 해금된 구간인지 확인
        if (GameManager.Instance.IsTimeUnlocked(targetTime))
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
