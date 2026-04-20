using UnityEngine;

[CreateAssetMenu(fileName = "JudgeDialogue", menuName = "JudgementGame/JudgeDialogueData")]
public class JudgeDialogueData : ScriptableObject
{
    [Header("도입부 - 게임 시작 시점 대사")]
    [TextArea(1, 3)]
    public string[] introLines = new[]
    {
        "신입, 견습 판관이 된 걸 축하해.",
        "사후세계 판관은 일반적인 판사와는 다르지.",
        "법이 아니라 자네만의 잣대로 판결을 내리면 돼.",
        "이제, 그 판결을 시뮬레이션으로 연습해보자고.",
        "자네 첫 사건이네.",
        "사건명: 녹음기.",
        "판결: 미정.",
        "자, 앞의 녹음기를 틀어볼까?"
    };

    [Header("각 차수 판단 시점 대사")]
    [TextArea(1, 3)] public string judgmentLine1 = "누가 더, 잘못한 사람 같나?";
    [TextArea(1, 3)] public string judgmentLine2 = "누가 더 잘못했나? 판단해라.";
    [TextArea(1, 3)] public string judgmentLine3 = "잘못한 사람을 선택해라.";
    [TextArea(1, 3)] public string judgmentLine4 = "마지막 판단이네. 어떤 사람을 처벌해야 하나?";

    [Header("엔딩 - 패턴 평가 이전 대사")]
    [TextArea(1, 3)]
    public string[] endingOpening = new[]
    {
        "그래, 신입."
    };

    [Header("엔딩 - 판단 패턴별 한 줄 평가")]
    [TextArea(1, 3)] public string endingConsistent    = "일관된 판단을 했군.";      // AAAA / BBBB
    [TextArea(1, 3)] public string endingLastSwitch    = "마지막에 판단이 흔들렸군."; // AAAB / BBBA
    [TextArea(1, 3)] public string endingMiddleSwitch  = "중간에 판단을 바꿨군.";     // AABB / BBAA
    [TextArea(1, 3)] public string endingFirstSwitch   = "처음에는 흔들렸군.";        // ABBB / BAAA
    [TextArea(1, 3)] public string endingManyChanges   = "판단에 변화가 많았군.";    // 2회 이상 변화

    [Header("엔딩 - 패턴 평가 이후 대사")]
    [TextArea(1, 3)]
    public string[] endingClosing = new[]
    {
        "이번 훈련, 수고했어.",
        "판단이란 건, 언제나 부족한 정보 속에서 내리는 거야.",
        "다음에는 더 나은 판결을 내리길.",
        "아, 그래서 진짜 판결은 어떻게 나왔냐고?",
        "미정, 이라네."
    };

    public string GetJudgmentLine(int stageNumber)
    {
        switch (stageNumber)
        {
            case 1: return judgmentLine1;
            case 2: return judgmentLine2;
            case 3: return judgmentLine3;
            case 4: return judgmentLine4;
        }
        return "";
    }
}