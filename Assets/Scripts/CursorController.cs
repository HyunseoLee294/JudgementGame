using UnityEngine;

public class CursorController : MonoBehaviour
{
    public static CursorController Instance;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (JudgeManager.Instance != null && JudgeManager.Instance.IsGameplayBlocked())
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public static void Lock()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public static void Unlock()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
