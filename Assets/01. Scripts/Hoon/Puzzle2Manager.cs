using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Puzzle2Manager : MonoBehaviour
{
    [Header("활성화 시킬 퍼즐 오브젝트 할당")]
    public GameObject[] puzzles;

    [Header("남은 클릭 횟수 TMP 할당")]
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
        // 모든 퍼즐 오브젝트 초기 비활성화
        foreach (GameObject p in puzzles)
        {
            p.SetActive(false);
        }
        // 첫 번째 퍼즐 오브젝트를 활성화 후 텍스트 업데이트.
        puzzles[curPuzzleIndex].SetActive(true);
        Puzzle2 curPuzzle = puzzles[curPuzzleIndex].GetComponent<Puzzle2>();
        UpdateClickText(curPuzzle);
    }

    // 다음 퍼즐로 넘어 활성화 시도. 마지막 퍼즐도 클리어 시 처리.
    public void ActivateNextPuzzle2()
    {
        // 현재 퍼즐 비활성화.
        puzzles[curPuzzleIndex].SetActive(false);

        // 모든 퍼즐 다 클리어 시
        if (++curPuzzleIndex >= puzzles.Length)
        {
            AllPuzzleClear();
            return;
        }

        // 다음 퍼즐 활성화 시키고 클릭 Text 업데이트.
        Puzzle2 curPuzzle = puzzles[curPuzzleIndex].GetComponent<Puzzle2>();
        puzzles[curPuzzleIndex].SetActive(true);
        UpdateClickText(curPuzzle);
    }

    // 모든 할당된 퍼즐 클리어 시 호출
    public void AllPuzzleClear()
    {
        Debug.Log("퍼즐 2 모든 퍼즐 클리어");
    }

    // 리셋 버튼에서 눌릴 때 현재 퍼즐의 노드를 리셋하는 함수 호출. (ResetNodes.cs에서도 호출.)
    public void ResetAllNodes()
    {
        puzzles[curPuzzleIndex].GetComponent<Puzzle2>().Reset();
    }

    // 퍼즐 교체하거나 업데이트하여 클릭 횟수 텍스트 업데이트.
    public void UpdateClickText(Puzzle2 curPuzzle)
    {
        maxClickText.text = "Max Clicks: " + curPuzzle.maxClickTimes;
        curClickText.text = "Cur Clicks: " + curPuzzle.curClickTimes;
    }

    // ── 솔버 ────────────────────────────────────────────────────────────────

    // 버튼에서 호출: 현재 퍼즐의 최소 클릭 해법을 Debug.Log로 출력
    public void SolveMinimum()
    {
        Puzzle2Node[] allNodes = puzzles[curPuzzleIndex].GetComponentsInChildren<Puzzle2Node>(true);
        int n = allNodes.Length;
        if (n == 0) { Debug.LogWarning("[Solver] 로드된 노드가 없습니다."); return; }

        var nodeIndex = new Dictionary<Puzzle2Node, int>(n);
        for (int i = 0; i < n; i++)
            if (allNodes[i] != null) nodeIndex[allNodes[i]] = i;

        int[,] A = new int[n, n];
        for (int j = 0; j < n; j++)
        {
            if (allNodes[j] == null) continue;
            A[j, j] = 1;
            foreach (Puzzle2Node nb in allNodes[j].neighbors)
                if (nb != null && nodeIndex.TryGetValue(nb, out int idx))
                    A[idx, j] = 1;
        }

        int[] b = new int[n];
        for (int i = 0; i < n; i++)
            b[i] = (allNodes[i] != null && allNodes[i].isOn) ? 0 : 1;

        int[] bestSolution = SolveGF2(A, b, n);

        if (bestSolution == null)
        {
            Debug.Log("[Solver] 현재 상태에서 해가 존재하지 않습니다.");
            return;
        }

        int bestClicks = 0;
        var names = new List<string>();
        for (int i = 0; i < n; i++)
        {
            if (bestSolution[i] != 1) continue;
            bestClicks++;
            names.Add(allNodes[i].name);
        }

        string msg = bestClicks == 0
            ? "이미 완성 상태입니다!"
            : $"최소 {bestClicks}번 클릭: " + string.Join(" → ", names);

        Debug.Log($"[Solver] {msg}");
    }

    static int[] SolveGF2(int[,] A, int[] b, int n)
    {
        int[,] mat = new int[n, n + 1];
        for (int r = 0; r < n; r++)
        {
            for (int c = 0; c < n; c++) mat[r, c] = A[r, c];
            mat[r, n] = b[r];
        }

        int[] pivotOfRow = new int[n];
        for (int i = 0; i < n; i++) pivotOfRow[i] = -1;

        var pivotCols = new List<int>();
        var freeCols = new List<int>();
        int curRow = 0;

        for (int col = 0; col < n && curRow < n; col++)
        {
            int pivIdx = -1;
            for (int row = curRow; row < n; row++)
                if (mat[row, col] == 1) { pivIdx = row; break; }
            if (pivIdx < 0) { freeCols.Add(col); continue; }

            if (pivIdx != curRow)
                for (int c = 0; c <= n; c++)
                { int t = mat[curRow, c]; mat[curRow, c] = mat[pivIdx, c]; mat[pivIdx, c] = t; }

            for (int row = 0; row < n; row++)
                if (row != curRow && mat[row, col] == 1)
                    for (int c = 0; c <= n; c++)
                        mat[row, c] ^= mat[curRow, c];

            pivotOfRow[curRow] = col;
            pivotCols.Add(col);
            curRow++;
        }

        int rank = curRow;

        var pivSet = new HashSet<int>(pivotCols);
        for (int col = 0; col < n; col++)
            if (!pivSet.Contains(col) && !freeCols.Contains(col)) freeCols.Add(col);

        for (int row = rank; row < n; row++)
            if (mat[row, n] == 1) return null;

        int numFree = freeCols.Count;
        int bestCount = int.MaxValue;
        int bestMask = 0;

        for (int mask = 0; mask < (1 << numFree); mask++)
        {
            int count = 0;
            for (int k = 0; k < rank; k++)
            {
                int val = mat[k, n];
                for (int f = 0; f < numFree; f++)
                    val ^= mat[k, freeCols[f]] * ((mask >> f) & 1);
                count += val;
            }
            for (int f = 0; f < numFree; f++)
                count += (mask >> f) & 1;

            if (count < bestCount) { bestCount = count; bestMask = mask; }
        }

        int[] x = new int[n];
        for (int f = 0; f < numFree; f++)
            x[freeCols[f]] = (bestMask >> f) & 1;
        for (int k = 0; k < rank; k++)
        {
            int val = mat[k, n];
            for (int f = 0; f < numFree; f++)
                val ^= mat[k, freeCols[f]] * x[freeCols[f]];
            x[pivotCols[k]] = val;
        }

        return x;
    }
}
