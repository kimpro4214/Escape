using UnityEngine;

public class Puzzle2Manager : MonoBehaviour
{
    [Header("활성화 시킬 퍼즐 순서대로 할당")]
    public GameObject[] puzzles;

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
        // 첫 번째 레벨 퍼즐 오브젝트만 활성화
        puzzles[curPuzzleIndex].SetActive(true);
    }

    // 다음 레벨의 퍼즐 활성화 시도. 만약 최대 레벨을 넘으면 AllPuzzleClear() 호출하여 클리어 처리.
    public void ActivateNextPuzzle2()
    {
        puzzles[curPuzzleIndex].SetActive(false);

        if (++curPuzzleIndex >= puzzles.Length)
        {
            AllPuzzleClear();
            return;
        }
        puzzles[curPuzzleIndex].SetActive(true);
    }

    public void AllPuzzleClear()
    {
        Debug.Log("퍼즐 2 모든 레벨 클리어");
    }
}
