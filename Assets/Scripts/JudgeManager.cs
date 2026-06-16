using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JudgeManager : MonoBehaviour
{
    public static JudgeManager Instance;

    [Header("참조")]
    public GameManager gameManager;
    public Recorder recorder;
    public AudioSource mainAudio;
    public JudgementUI judgementUI;
    public JudgeDialogueData dialogueData;

    [Header("엔딩 음악 (씬 전환 후에도 계속 재생됨)")]
    public AudioSource endingMusic;

    // [변경] 단계별 "고정 섹션 집합" 대신, 각 차수 판단이 발동하는
    // "누적 완주 섹션 개수" 임계값으로 트리거를 바꾼다.
    //  - 1차: 누적 1개 (섹션 1)
    //  - 2차: 누적 3개 (1 + 2,3,4 중 2개)
    //  - 3차: 누적 5개 (+ 남은 셋 중 2개)
    //  - 4차: 누적 7개 (전부)
    [Header("판단 트리거 (누적 완주 섹션 개수)")]
    public int[] judgmentThresholds = { 1, 3, 5, 7 };

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

        // [변경] "현재 단계의 요구 섹션이 전부 완주됐는가"가 아니라
        // "누적 완주 개수가 현재 차수의 임계값에 도달했는가"로 판단을 발동.
        int threshold = GetThresholdForCurrentStage();
        if (threshold < 0) return;               // 지금은 판단 대기 단계가 아님
        if (sectionFirstPlayed.Count < threshold) return;  // 아직 개수 부족

        // 즉시 Phase를 Judgment로 전환 (0.3초 대기 중에도 상호작용 차단)
        Phase = NextJudgmentPhase(Phase);
        StartCoroutine(TriggerJudgment());
    }

    public bool HasUnheardUnlocks()
    {
        return pendingUnheardSections.Count > 0;
    }

    // [변경] 고정 섹션 배열 대신, 현재 Stage 차수에 해당하는 누적 임계값을 반환.
    // 판단 대기 단계(Stage1~4)가 아니면 -1.
    int GetThresholdForCurrentStage()
    {
        int idx = StageIndex(Phase);   // Stage1→0, Stage2→1, Stage3→2, Stage4→3
        if (idx < 0) return -1;
        if (judgmentThresholds == null || idx >= judgmentThresholds.Length) return -1;
        return judgmentThresholds[idx];
    }

    // Stage 페이즈를 0-기반 차수 인덱스로 변환 (그 외 페이즈는 -1)
    int StageIndex(GamePhase p)
    {
        switch (p)
        {
            case GamePhase.Stage1: return 0;
            case GamePhase.Stage2: return 1;
            case GamePhase.Stage3: return 2;
            case GamePhase.Stage4: return 3;
        }
        return -1;
    }

    IEnumerator TriggerJudgment()
    {
        // Phase는 이미 NotifySectionFirstPlayed에서 Judgment로 전환됨
        yield return new WaitForSeconds(delayBeforeJudgment);

        // 판단 UI가 뜰 때 녹음기 UI가 닫혀 있었다면 강제로 열기
        // (오디오 재생/스킵/리와인드는 그대로 돌도록 CancelCurrentRoutines는 호출하지 않음)
        if (recorder != null && recorder.recorderUI != null)
        {
            recorder.recorderUI.ForceOpen();
        }

        char choice = ' ';
        if (judgementUI != null && dialogueData != null)
        {
            string line = dialogueData.GetJudgmentLine(JudgmentStageNumber(Phase));
            yield return judgementUI.ShowJudgment(line, c => choice = c);
        }
        if (recorder != null && recorder.recorderUI != null)
        {
            recorder.recorderUI.Close();
        }
        if (choice == ' ') choice = 'A'; // 방어: UI 미설정 시 기본값
        Judgments.Add(choice);

        if (Phase != GamePhase.Judgment4)
        {
            SubtitleManager subtitleManager = FindObjectOfType<SubtitleManager>();
            if (subtitleManager != null)
            {
                yield return subtitleManager.ShowTemporaryMessage(choice + ", 알겠네.");
            }
        }

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

        // [변경] 4차 판단 후 마지막 녹음 전체 재생 없이 바로 사수 판관 대사로 진행
        if (recorder != null) recorder.CancelCurrentRoutines();
        // 단서 미리듣기(UnlockRoutine)가 아직 끝나지 않았을 수 있으므로 먼저 중단
        // (안 그러면 엔딩 중에 엉뚱한 스킵/되감기 사운드가 울릴 수 있음)
        if (gameManager != null) gameManager.CancelUnlockRoutine();
        if (mainAudio != null) mainAudio.Stop();

        // 2) 엔딩 대사: opening → 판단 시퀀스 → 패턴 한줄평 → closing
        var lines = new List<string>();
        if (dialogueData.endingOpening != null) lines.AddRange(dialogueData.endingOpening);

        string judgmentSummary = BuildJudgmentSummary();
        if (!string.IsNullOrEmpty(judgmentSummary)) lines.Add(judgmentSummary);

        string patternLine = AnalyzePattern();
        if (!string.IsNullOrEmpty(patternLine)) lines.Add(patternLine);

        if (dialogueData.endingClosing != null) lines.AddRange(dialogueData.endingClosing);

        yield return judgementUI.ShowJudgeLines(lines.ToArray(), lineIndex =>
        {
            // 지정된 대사가 표시되는 순간 엔딩 음악 재생 시작
            if (endingMusic != null
                && !endingMusic.isPlaying
                && lines[lineIndex] == dialogueData.endingMusicTriggerLine)
            {
                endingMusic.Play();
            }
        });

        // 마지막 대사 사라진 뒤 2초 후 타이틀 씬으로 복귀
        yield return new WaitForSecondsRealtime(2f);

        // 씬 전환 후에도 음악이 끊기지 않도록 별도 루트 오브젝트로 분리 후 보존
        if (endingMusic != null && endingMusic.isPlaying)
        {
            endingMusic.transform.SetParent(null);
            DontDestroyOnLoad(endingMusic.gameObject);
        }

        SceneManager.LoadScene("TitleScene");
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

    public bool IsEnding()
    {
        return Phase == GamePhase.Ending;
    }

    // [추가] 단서 게이트용 헬퍼.
    // "이 Phase 단계 이상으로 진행됐는가?"를 GamePhase enum 선언 순서 기준으로 판정.
    // Clue가 unlockPhase 게이트를 검사할 때 사용한다.
    public bool HasReachedPhase(GamePhase target)
    {
        return (int)Phase >= (int)target;
    }

    // 주어진 섹션들이 모두 첫 완주됐는지
    public bool AreSectionsFirstPlayed(IEnumerable<int> sectionIds)
    {
        foreach (var id in sectionIds)
        {
            if (!sectionFirstPlayed.Contains(id)) return false;
        }
        return true;
    }
}
