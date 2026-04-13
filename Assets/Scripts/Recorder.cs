using System.Collections;
using UnityEngine;

public class Recorder : MonoBehaviour
{
    public AudioSource mainAudio;
    public AudioSource rewindSfx;

    private bool isRewinding = false;
    private bool hasStarted = false;

    void Update()
    {
        if (isRewinding) return;

        // 한 번이라도 재생이 시작됐는데, 지금은 재생 중이 아니면 → 끝난 것
        if (hasStarted && !mainAudio.isPlaying)
        {
            StartCoroutine(RewindRoutine());
        }

        // 재생 시작 감지
        if (mainAudio.isPlaying && !hasStarted)
        {
            hasStarted = true;
        }
    }

    IEnumerator RewindRoutine()
    {
        isRewinding = true;

        mainAudio.time = 0f;

        // 되감기 효과음 재생
        rewindSfx.Play();

        // 효과음이 끝날 때까지 대기
        yield return new WaitForSeconds(rewindSfx.clip.length);

        // 메인 오디오 다시 재생
        mainAudio.Play();
        isRewinding = false;
    }
}