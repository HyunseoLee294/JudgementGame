[System.Serializable]
public class DialogueSection
{
    public string sectionName;  // 구간 이름 (예: "목걸이")
    public int sectionId;       // 구간 번호 (1, 2, 3...)
    public float startTime;     // 시작 시간 (초)
    public float endTime;       // 끝 시간 (초)
    public bool unlockedByDefault;  // 처음부터 해금된 구간인가?
}