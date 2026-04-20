using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JudgmentUI : MonoBehaviour
{
    [Header("판단 선택창 (A/B)")]
    public GameObject judgmentPanel;
    public Button buttonA;
    public Button buttonB;
    public Button confirmButton;
    public TextMeshProUGUI judgeLineText;
    public TextMeshProUGUI nameAText;
    public TextMeshProUGUI nameBText;

    [Header("사수 판관 대사 단독 표시 (도입부/엔딩)")]
    public GameObject judgeDialoguePanel;
    public TextMeshProUGUI judgeDialogueText;
    public float lineDuration = 3f;
    public KeyCode skipKey = KeyCode.Space;

    private char selected = ' ';

    void Awake()
    {
        if (judgmentPanel) judgmentPanel.SetActive(false);
        if (judgeDialoguePanel) judgeDialoguePanel.SetActive(false);

        if (buttonA) buttonA.onClick.AddListener(() => Select('A'));
        if (buttonB) buttonB.onClick.AddListener(() => Select('B'));
    }

    void Select(char c)
    {
        selected = c;
        if (nameAText) nameAText.fontStyle = (c == 'A') ? FontStyles.Bold : FontStyles.Normal;
        if (nameBText) nameBText.fontStyle = (c == 'B') ? FontStyles.Bold : FontStyles.Normal;
    }

    public IEnumerator ShowJudgeLines(string[] lines)
    {
        UnlockCursor();
        judgeDialoguePanel.SetActive(true);

        foreach (var line in lines)
        {
            judgeDialogueText.text = line;
            float t = 0f;
            while (t < lineDuration && !Input.GetKeyDown(skipKey))
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            // 프레임 하나 쉬어서 스페이스 중복 감지 방지
            yield return null;
        }

        judgeDialoguePanel.SetActive(false);
        LockCursor();
    }

    public IEnumerator ShowJudgment(string judgeLine, Action<char> onDecided)
    {
        UnlockCursor();
        judgmentPanel.SetActive(true);
        if (judgeLineText) judgeLineText.text = judgeLine;

        selected = ' ';
        if (nameAText) nameAText.fontStyle = FontStyles.Normal;
        if (nameBText) nameBText.fontStyle = FontStyles.Normal;
        if (confirmButton) confirmButton.interactable = false;

        bool confirmed = false;
        if (confirmButton)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() =>
            {
                if (selected != ' ') confirmed = true;
            });
        }

        while (!confirmed)
        {
            if (confirmButton) confirmButton.interactable = (selected != ' ');
            yield return null;
        }

        onDecided?.Invoke(selected);
        judgmentPanel.SetActive(false);
        LockCursor();
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
