using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeManager : MonoBehaviour
{
    public static JudgeManager Instance;

    [Header("참조")]
    public GameManager gameManager;
    public Recorder recorder;
    public AudioSource mainAudio;
    public JudgmentUI judgmentUI;

    [Header("단계별 요구 섹션")]
    public int[] stage1Sections = { 1 };
    public int[] stage2Sections = { 2, 3, 4 };
    public int[] stage3Sections = { 5, 6 };
    public int[] stage4Sections = { 7 };

    [Header("판단 전 지연 (초)")]
    public float delayBeforeJudgment = 0.3f;

    public GamePhase Phase { get; private set; } = GamePhase.Intro;
    public List<char> Judgments { get; private set; } = new List<char>();

    private readonly HashSet<int> sectionFirstPlayed = new HashSet<int>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(IntroRoutine());
    }

    IEnumerator IntroRoutine()
    {
        Phase = GamePhase.Intro;
        yield return judgmentUI.ShowJudgeLines(new[]
        {
            "신입, 견습 판관이 된 걸 축하해.",
            "사후세계 판관은 일반적인 판사와는 다르지.",
            "법이 아니라 자네만의 잣대로 판결을 내리면 돼.",
            "이제, 그 판결을 시뮬레이션으로 연습해보자고.",
            "자네 첫 사건이네.",
            "사건명: 녹음기.",
            "판결: 미정.",
            "자, 앞의 녹음기를 틀어볼까?"
        });
        Phase = GamePhase.Stage1;
    }

    public void NotifySectionFirstPlayed(int sectionId)
    {
        if (sectionFirstPlayed.Contains(sectionId)) return;
        sectionFirstPlayed.Add(sectionId);

        int[] required = GetRequiredSectionsForCurrentStage();
        if (required == null) return;

        foreach (var sid in required)
        {
            if (!sectionFirstPlayed.Contains(sid)) return;
        }

        StartCoroutine(TriggerJudgment());
    }

    int[] GetRequiredSectionsForCurrentStage()
    {
        switch (Phase)
        {
            case GamePhase.Stage1: return stage1Sections;
            case GamePhase.Stage2: return stage2Sections;
            case GamePhase.Stage3: return stage3Sections;
            case GamePhase.Stage4: return stage4Sections;
            default: return null;
        }
    }

    IEnumerator TriggerJudgment()
    {
        yield return new WaitForSeconds(delayBeforeJudgment);

        recorder.CancelCurrentRoutines();
        if (mainAudio.isPlaying) mainAudio.Pause();

        Phase = NextJudgmentPhase(Phase);

        char choice = ' ';
        yield return judgmentUI.ShowJudgment(GetJudgmentLine(Phase), c => choice = c);
        Judgments.Add(choice);

        Phase = NextStagePhase(Phase);

        if (Phase == GamePhase.Ending)
        {
            StartCoroutine(EndingRoutine());
        }
        else
        {
            mainAudio.Play();
        }
    }

    GamePhase NextJudgmentPhase(GamePhase s)
    {
        switch (s)
        {
            case GamePhase.Stage1: return GamePhase.Judgment1;
            case GamePhase.Stage2: return GamePhase.Judgment2;
            case GamePhase.Stage3: return GamePhase.Judgment3;
            case GamePhase.Stage4: return GamePhase.Judgment4;
        }
        return s;
    }

    GamePhase NextStagePhase(GamePhase j)
    {
        switch (j)
        {
            case GamePhase.Judgment1: return GamePhase.Stage2;
            case GamePhase.Judgment2: return GamePhase.Stage3;
            case GamePhase.Judgment3: return GamePhase.Stage4;
            case GamePhase.Judgment4: return GamePhase.Ending;
        }
        return j;
    }

    string GetJudgmentLine(GamePhase p)
    {
        switch (p)
        {
            case GamePhase.Judgment1: return "누가 더, 잘못한 사람 같나?";
            case GamePhase.Judgment2: return "누가 더 잘못했나? 판단해라.";
            case GamePhase.Judgment3: return "잘못한 사람을 선택해라.";
            case GamePhase.Judgment4: return "마지막 판단이네. 어떤 사람을 처벌해야 하나?";
        }
        return "";
    }

    IEnumerator EndingRoutine()
    {
        yield return judgmentUI.ShowJudgeLines(new[]
        {
            "그래, 신입.",
            AnalyzePattern(),
            "이번 훈련, 수고했어.",
            "판단이란 건, 언제나 부족한 정보 속에서 내리는 거야.",
            "다음에는 더 나은 판결을 내리길.",
            "아, 그래서 진짜 판결은 어떻게 나왔냐고?",
            "미정, 이라네."
        });
    }

    string AnalyzePattern()
    {
        string s = new string(Judgments.ToArray());
        int changes = 0;
        for (int i = 1; i < s.Length; i++)
        {
            if (s[i] != s[i - 1]) changes++;
        }

        if (changes >= 2) return "판단에 변화가 많았군.";
        if (s == "AAAA" || s == "BBBB") return "일관된 판단을 했군.";
        if (s == "AAAB" || s == "BBBA") return "마지막에 판단이 흔들렸군.";
        if (s == "AABB" || s == "BBAA") return "중간에 판단을 바꿨군.";
        if (s == "ABBB" || s == "BAAA") return "처음에는 흔들렸군.";
        return "";
    }

    public bool IsGameplayBlocked()
    {
        return Phase == GamePhase.Intro
            || Phase == GamePhase.Judgment1
            || Phase == GamePhase.Judgment2
            || Phase == GamePhase.Judgment3
            || Phase == GamePhase.Judgment4
            || Phase == GamePhase.Ending;
    }

    public bool IsIntro()
    {
        return Phase == GamePhase.Intro;
    }
}
