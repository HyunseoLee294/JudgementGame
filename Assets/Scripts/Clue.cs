using UnityEngine;

public class Clue : MonoBehaviour, IInteractable
{
    public int sectionIdToUnlock;  // 이 단서로 해금할 구간 ID
    public string clueName;         // 디버그용 이름
    public int[] requiredSections;  // 이 구간들이 전부 해금되어야 상호작용 가능

    public bool IsAvailable()
    {
        foreach (int id in requiredSections)
        {
            if (!GameManager.Instance.IsSectionUnlocked(id)) return false;
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
