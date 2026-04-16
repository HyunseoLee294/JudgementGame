using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;  // 다른 스크립트에서 쉽게 접근

    public SubtitleData subtitleData;
    private HashSet<int> unlockedSections = new HashSet<int>();

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
        if (!unlockedSections.Contains(sectionId))
        {
            unlockedSections.Add(sectionId);
            Debug.Log("구간 해금: " + sectionId);
        }
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