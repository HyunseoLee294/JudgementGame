using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSubtitleData", menuName = "Game/Subtitle Data")]
public class SubtitleData : ScriptableObject
{
    public List<SubtitleLine> lines;
    public List<DialogueSection> sections;
}