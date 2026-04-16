using UnityEngine;

public class Clue : MonoBehaviour, IInteractable
{
    public int sectionIdToUnlock;  // 이 단서로 해금할 구간 ID
    public string clueName;         // 디버그용 이름

    public void Interact()
    {
        GameManager.Instance.UnlockSection(sectionIdToUnlock);
        Debug.Log(clueName + " 발견! 구간 " + sectionIdToUnlock + " 해금");
    }
}