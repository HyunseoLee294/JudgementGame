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
    public JudgementUI judgementUI;
    public JudgeDialogueData dialogueData;

    [Header("단계별 요구 섹션")]
    public int[] stage1Sections = { 1 };
    public int[] stage2Sections = { 2, 3, 4 };
    public int[] stage3Sections = { 5, 6 };
    public int[] stage4Sections = { 7 };

    [Header("판단 전 지연 (초)")]
    public float delayBeforeJudgment = 0.3f;

    public GamePhase Phase { get; private set; } = GamePhase.Intro;
    public List<char> Judgments { get; private set; } = new List<char>();

    // 처음부터 끝까지 한 번 재생된 섹션들
    private readonly HashSet<int> sectionFirstPlayed = new HashSet<int>();

    // 해금은 되었지만 아직 "첫 완주"되지 않은 섹션들 (탐색 차단에 사용)
    private readonly HashSet<int> pendingUnheardSections = new HashSet<int>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 기본 해금된 섹션도 "들어야 할 섹션" 목록에 포함
        if (gameManager != null && gameManager.subtitleData != null)
        {
            foreach (var s in gameManager.subtitleData.sections)
            {
                if (s.unlockedByDefault)
                {
                    pendingUnheardSections.Add(s.sectionId);
                }
            }
        }

        StartCoroutine(IntroRoutine());
    }

    IEnumerator IntroRoutine()
    {
        Phase = GamePhase.Intro;
        if (judgementUI != null) judgementUI.SetBlackOverlay(true);

        if (judgementUI != null && dialogueData != null)
        {
            yield return judgementUI.ShowJudgeLines(dialogueData.introLines);
        }

        if (judgementUI != null) judgementUI.SetBlackOverlay(false);
        Phase = GamePhase.Stage1;
    }

    // GameManager → UnlockSection 호출 시점에 같이 호출됨
    public void RegisterUnlock(int sectionId)
    {
        if (sectionFirstPlayed.Contains(sectionId)) return; // 이미 완주한 섹션이면 무시
        pendingUnheardSections.Add(sectionId);
    }

    // 섹션이 처음부터 끝까지 재생된 시점에 GameManager가 호출
    public void NotifySectionFirstPlayed(int sectionId)
    {
        if (sectionFirstPlayed.Contains(sectionId)) return;
        sectionFirstPlayed.Add(sectionId);
        pendingUnheardSections.Remove(sectionId);

        int[] required = GetRequiredSectionsForCurrentStage();
        if (required == null) return;

        foreach (var sid in required)
        {
            if (!sectionFirstPlayed.Contains(sid)) return;
        }

        // 즉시 Phase를 Judgment로 전환 (0.3초 대기 중에도 상호작용 차단)
        Phase = NextJudgmentPhase(Phase);
        StartCoroutine(TriggerJudgment());
    }

    public bool HasUnheardUnlocks()
    {
        return pendingUnheardSections.Count > 0;
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
        // Phase는 이미 NotifySectionFirstPlayed에서 Judgment로 전환됨
        yield return new WaitForSeconds(delayBeforeJudgment);

        // 판단 UI 중에도 녹음기 UI/오디오는 계속 유지
        if (recorder != null) recorder.CancelCurrentRoutines();

        char choice = ' ';
        if (judgementUI != null && dialogueData != null)
        {
            string line = dialogueData.GetJudgmentLine(JudgmentStageNumber(Phase));
            yield return judgementUI.ShowJudgment(line, c => choice = c);
        }
        if (choice == ' ') choice = 'A'; // 방어: UI 미설정 시 기본값
        Judgments.Add(choice);

        Phase = NextStagePhase(Phase);

        if (Phase == GamePhase.Ending)
        {
            StartCoroutine(EndingRoutine());
        }
    }

    int JudgmentStageNumber(GamePhase p)
    {
        switch (p)
        {
            case GamePhase.Judgment1: return 1;
            case GamePhase.Judgment2: return 2;
            case GamePhase.Judgment3: return 3;
            case GamePhase.Judgment4: return 4;
        }
        return 0;
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

    IEnumerator EndingRoutine()
    {
        if (judgementUI == null || dialogueData == null) yield break;

        // 1) 검은 화면 켜기
        judgementUI.SetBlackOverlay(true);

        // 2) 오디오를 처음부터 끝까지 한 번 재생 (자막은 기존 SubtitleManager가 표시)
        if (recorder != null) recorder.CancelCurrentRoutines();
        if (mainAudio != null)
        {
            mainAudio.Stop();
            mainAudio.time = 0f;
            mainAudio.Play();

            while (mainAudio.isPlaying)
            {
                yield return null;
            }
        }

        // 3) 엔딩 대사: opening → 판단 시퀀스 → 패턴 한줄평 → closing
        var lines = new List<string>();
        if (dialogueData.endingOpening != null) lines.AddRange(dialogueData.endingOpening);

        string judgmentSummary = BuildJudgmentSummary();
        if (!string.IsNullOrEmpty(judgmentSummary)) lines.Add(judgmentSummary);

        string patternLine = AnalyzePattern();
        if (!string.IsNullOrEmpty(patternLine)) lines.Add(patternLine);

        if (dialogueData.endingClosing != null) lines.AddRange(dialogueData.endingClosing);

        yield return judgementUI.ShowJudgeLines(lines.ToArray());

        // 엔딩 대사 종료 후 검은 화면은 그대로 유지
    }

    string BuildJudgmentSummary()
    {
        if (dialogueData == null) return "";
        if (string.IsNullOrEmpty(dialogueData.endingJudgmentSummaryFormat)) return "";
        if (Judgments == null || Judgments.Count == 0) return "";

        string seq = new string(Judgments.ToArray());
        return string.Format(dialogueData.endingJudgmentSummaryFormat, seq);
    }

    string AnalyzePattern()
    {
        if (dialogueData == null) return "";

        string s = new string(Judgments.ToArray());
        int changes = 0;
        for (int i = 1; i < s.Length; i++)
        {
            if (s[i] != s[i - 1]) changes++;
        }

        if (changes >= 2) return dialogueData.endingManyChanges;
        if (s == "AAAA" || s == "BBBB") return dialogueData.endingConsistent;
        if (s == "AAAB" || s == "BBBA") return dialogueData.endingLastSwitch;
        if (s == "AABB" || s == "BBAA") return dialogueData.endingMiddleSwitch;
        if (s == "ABBB" || s == "BAAA") return dialogueData.endingFirstSwitch;
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
