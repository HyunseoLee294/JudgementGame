using System.Collections;
using UnityEngine;

public class FurnitureInteractable : MonoBehaviour, IInteractable
{
    public enum FurnitureType { Drawer, Cabinet }
    public enum UnlockCondition { Always, AfterSecondJudgment, WithPairedClue }

    [Header("종류")]
    public FurnitureType furnitureType = FurnitureType.Drawer;

    [Header("서랍 이동 (Drawer 전용)")]
    public Vector3 openDirection = Vector3.forward;  // 로컬 방향
    public float openDistance = 0.3f;

    [Header("문 회전 (Cabinet 전용)")]
    public float openAngle = 90f;
    public Vector3 rotationAxis = Vector3.up;

    [Header("공통")]
    public float animDuration = 0.4f;

    [Header("해금 조건")]
    public UnlockCondition unlockCondition = UnlockCondition.AfterSecondJudgment;
    public Clue pairedClue; // WithPairedClue일 때 7번 단서 Clue 오브젝트 연결

    [Header("내부 단서 (열릴 때만 활성화)")]
    public GameObject[] clueObjects;

    public bool IsOpen { get; private set; }
    private bool _animating;

    void Start()
    {
        SetCluesActive(false);
    }

    public bool IsAvailable()
    {
        if (JudgeManager.Instance == null) return false;

        return unlockCondition switch
        {
            UnlockCondition.Always => true,
            UnlockCondition.AfterSecondJudgment =>
                JudgeManager.Instance.HasReachedPhase(GamePhase.Stage3),
            UnlockCondition.WithPairedClue =>
                pairedClue != null && pairedClue.IsAvailable(),
            _ => false
        };
    }

    public void Interact()
    {
        if (_animating) return;
        StartCoroutine(ToggleCoroutine());
    }

    IEnumerator ToggleCoroutine()
    {
        _animating = true;
        bool opening = !IsOpen;

        Vector3 startPos = transform.localPosition;
        Quaternion startRot = transform.localRotation;
        Vector3 targetPos = startPos;
        Quaternion targetRot = startRot;

        if (furnitureType == FurnitureType.Drawer)
        {
            Vector3 delta = openDirection.normalized * openDistance;
            targetPos = opening ? startPos + delta : startPos - delta;
        }
        else // Cabinet
        {
            float angle = opening ? openAngle : -openAngle;
            targetRot = startRot * Quaternion.AngleAxis(angle, rotationAxis);
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / animDuration;
            float ease = Mathf.SmoothStep(0f, 1f, t);
            transform.localPosition = Vector3.Lerp(startPos, targetPos, ease);
            transform.localRotation = Quaternion.Slerp(startRot, targetRot, ease);
            yield return null;
        }

        transform.localPosition = targetPos;
        transform.localRotation = targetRot;
        IsOpen = opening;
        SetCluesActive(IsOpen);
        _animating = false;
    }

    void SetCluesActive(bool active)
    {
        if (clueObjects == null) return;
        foreach (var obj in clueObjects)
            if (obj != null) obj.SetActive(active);
    }
}