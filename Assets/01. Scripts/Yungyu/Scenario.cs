using UnityEngine;

[System.Serializable]
public class Scenario
{
    public string title;
    [TextArea(3, 5)]
    public string openingText;      // <--- 여기를 openingText로 수정!
    public string correctAnswer;
    public string secretTruth;
    [TextArea(5, 10)]
    public string gptInstruction;
    public string[] hints;
}