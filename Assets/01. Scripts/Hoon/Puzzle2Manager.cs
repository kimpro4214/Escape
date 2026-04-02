using TMPro;
using UnityEngine;

public class Puzzle2Manager : MonoBehaviour
{
    [Header("활성화 시킬 퍼즐 순서대로 할당")]
    public GameObject[] puzzles;

    [Header("현재 클릭 횟수 TMP 할당")]
    public TextMeshPro maxClickText;
    public TextMeshPro curClickText;

    public static Puzzle2Manager instance;

    private int curPuzzleIndex;

    private void Awake()
    {
        if (instance == null) instance = this;
        curPuzzleIndex = 0;
    }

    private void Start()
    {
        // 모든 퍼즐 오브젝트 일단 비활성화
        foreach (GameObject p in puzzles)
        {
            p.SetActive(false);
        }
        // 첫 번째 레벨 퍼즐 오브젝트만 활성화 후 텍스트 업데이트.
        puzzles[curPuzzleIndex].SetActive(true);
        Puzzle2 curPuzzle = puzzles[curPuzzleIndex].GetComponent<Puzzle2>();
        UpdateClickText(curPuzzle);
    }

    // 다음 레벨의 퍼즐 활성화 시도. 만약 최대 레벨을 넘으면 클리어 처리.
    public void ActivateNextPuzzle2()
    {
        // 현재 레벨 비활성화.
        puzzles[curPuzzleIndex].SetActive(false);

        // 모든 레벨 다 클리어 했을 때
        if (++curPuzzleIndex >= puzzles.Length)
        {
            AllPuzzleClear();
            return;
        }

        // 다음 레벨 활성화 시키고 클릭 Text 업데이트.
        Puzzle2 curPuzzle = puzzles[curPuzzleIndex].GetComponent<Puzzle2>();
        puzzles[curPuzzleIndex].SetActive(true);
        UpdateClickText(curPuzzle);
    }

    // 모든 할당된 레벨 클리어 시 호출
    public void AllPuzzleClear()
    {
        Debug.Log("퍼즐 2 모든 레벨 클리어");
    }

    // 리셋 오브젝트 클릭 시 모든 노드 꺼지는 함수 호출. (ResetNodes.cs에서만 호출.)
    public void ResetAllNodes()
    {
        puzzles[curPuzzleIndex].GetComponent<Puzzle2>().Reset();
    }

    // 현재 퍼즐을 참조하여 클릭 횟수 텍스트 업데이트.
    public void UpdateClickText(Puzzle2 curPuzzle)
    {
        maxClickText.text = "Max Clicks: " + curPuzzle.maxClickTimes;
        curClickText.text = "Cur Clicks: " + curPuzzle.curClickTimes;
    }
}
