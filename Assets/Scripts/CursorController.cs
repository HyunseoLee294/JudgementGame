using UnityEngine;

public static class CursorController
{
    public static void Lock()
    {
        // Judgement/Intro/Ending 페이즈에서는 커서를 절대 숨기지 않는다
        if (JudgeManager.Instance != null && JudgeManager.Instance.IsGameplayBlocked())
            return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public static void Unlock()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
