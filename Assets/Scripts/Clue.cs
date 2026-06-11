using UnityEngine;

public class Clue : MonoBehaviour, IInteractable
{
    public int sectionIdToUnlock;  // 이 단서로 해금할 구간 ID
    public string clueName;         // 디버그용 이름

    // [변경] 기존 requiredSections(특정 섹션들이 "해금"돼야 함)를
    // unlockPhase(판단 차수가 이 단계 이상으로 진행돼야 함)로 교체.
    //
    // 이유: 새 규칙에서 단서가 열리는 시점은 "특정 섹션 해금"이 아니라
    // "직전 판단을 통과한 시점"이다. 각 차수 판단은 누적 완주 개수로
    // JudgeManager가 발동하므로, 단서 게이트는 그 결과(Phase)만 참조하면 된다.
    //
    //  - 2,3,4번 단서 → unlockPhase = Stage2 (1차 판단 통과 후)
    //  - 5,6번 단서  → unlockPhase = Stage3 (2차 판단 통과 후)
    //  - 7번 단서    → unlockPhase = Stage4 (3차 판단 통과 후)
    public GamePhase unlockPhase = GamePhase.Stage1;

    public int[] requiredSectionsPlayed; // 이 단서가 열리기 전 완주돼야 할 섹션들 (7번 단서엔 {1,2,3,4,5,6})

    public bool IsAvailable()
    {
        if (JudgeManager.Instance == null) return false;
        if (!JudgeManager.Instance.HasReachedPhase(unlockPhase)) return false;

        if (requiredSectionsPlayed != null && requiredSectionsPlayed.Length > 0)
        {
            if (!JudgeManager.Instance.AreSectionsFirstPlayed(requiredSectionsPlayed))
                return false;
        }
        return true;
    }


    public void Interact()
    {
        if (!IsAvailable()) return;

        if (GameManager.Instance.IsSectionUnlocked(sectionIdToUnlock))
        {
            GameManager.Instance.ShowNotification("이미 해금된 단서입니다.");
            return;
        }

        GameManager.Instance.UnlockSection(sectionIdToUnlock);
        Debug.Log(clueName + " 발견! 구간 " + sectionIdToUnlock + " 해금");
    }
}
