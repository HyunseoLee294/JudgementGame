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

    void Start()
    {
        HideSubtitle();
        if (subtitleText != null)
        {
            subtitleText.gameObject.SetActive(false);
        }
    }
    void Update()
    {
        // 하단 자막 기능을 사용하지 않으므로 항상 숨김 유지
        HideSubtitle();
    }

    void HideSubtitle()
    {
        if (subtitleText != null)
        {
            subtitleText.text = "";
        }

        currentIndex = -1;
    }
}