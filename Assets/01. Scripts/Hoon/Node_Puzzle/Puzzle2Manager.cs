using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class Puzzle2Manager : MonoBehaviour
{
    [Header("모든 레벨의 Puzzle2 인스턴스")]
    public Puzzle2[] allPuzzles;

    [Header("현재 풀고 있는 Puzzle2 인스턴스")]
    public Puzzle2 curPuzzle;

    public static Puzzle2Manager instance;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    // 트리거 발동 시 curPuzzle 인스턴스가 바뀌는 함수.
    public void UpdateCurPuzzle(int targetLevel)
    {

    }
    
    public List<int> GetSolution()
    {
        if (curPuzzle != null)
            return curPuzzle.GetBestSolution();
        return null;
    }
}
