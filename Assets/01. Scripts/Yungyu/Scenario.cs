using UnityEngine;

[System.Serializable]
public class Scenario
{
    public string title;
    [TextArea(3, 5)]
    public string openingText;
    public string correctAnswer;
    public string secretTruth;
    [TextArea(5, 10)]
    public string gptInstruction;
    public string[] hints;
}