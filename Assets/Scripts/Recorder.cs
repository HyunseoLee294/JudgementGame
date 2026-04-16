using System.Collections;
using UnityEngine;

public class Recorder : MonoBehaviour
{
    public AudioSource mainAudio;
    public AudioSource rewindSfx;
    public AudioSource skipSfx;

    private bool isRewinding = false;
    private bool isSkipping = false;
    private bool hasStarted = false;

    void Update()
    {
        if (isRewinding || isSkipping) return;

        // 한 번이라도 재생이 시작됐는데, 지금은 재생 중이 아니면 → 끝난 것
        if (hasStarted && !mainAudio.isPlaying)
        {
            StartCoroutine(RewindRoutine());
        }

        // 재생 시작 감지
        if (mainAudio.isPlaying)
        {
            // 재생 시작 플래그 (한 번만 true로)
            if (!hasStarted)
            {
                hasStarted = true;
            }

            // 해금 안 된 구간 감지 → 스킵
            if (!GameManager.Instance.IsTimeUnlocked(mainAudio.time))
            {
                StartCoroutine(SkipRoutine());
            }
        }

    }
    IEnumerator SkipRoutine()
    {
        isSkipping = true;

        // 오디오 일시정지
        mainAudio.Pause();

        // 삐 소리 재생
        skipSfx.Play();
        yield return new WaitForSeconds(skipSfx.clip.length);

        // 다음 해금 구간 찾기
        float nextTime = GameManager.Instance.GetNextUnlockedTime(mainAudio.time);

        if (nextTime >= 0)
        {
            // 다음 해금 구간으로 점프 후 재생 재개
            mainAudio.time = nextTime;
            mainAudio.Play();
            isSkipping = false;
        }
        else
        {
            // 뒤에 해금 구간 없음 → 바로 되감기 루틴
            isSkipping = false;
            StartCoroutine(RewindRoutine());
        }
    }

    IEnumerator RewindRoutine()
    {
        isRewinding = true;
        mainAudio.Pause();
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