public enum GamePhase
{
    Intro,       // 도입부: 사수 판관 인트로 대사, 녹음기 재생 불가
    Stage1,      // 섹션 1만 들을 수 있음
    Judgment1,   // 1차 A/B 선택
    Stage2,      // 섹션 2,3,4 해금 가능
    Judgment2,
    Stage3,      // 섹션 5,6
    Judgment3,
    Stage4,      // 섹션 7
    Judgment4,
    Ending       // 엔딩 대사
}
