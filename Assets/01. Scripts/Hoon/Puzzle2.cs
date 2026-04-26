using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Puzzle2 : MonoBehaviour
{
    public static Action<int> OnPuzzleCleared; // 해당 레벨 퍼즐 클리어 시 방송

    [Header("색상 설정")]
    public Color onColor = Color.green;
    public Color offColor = new Color(0.5f, 0.5f, 0.5f);

    [Header("해당 레벨 최소 클릭 횟수 설정")]
    public int curClickTimes;

    [Header("퍼즐 번호 설정")]
    public int curPuzzleNum;

    [Header("현재 클릭 횟수 TMP")]
    public TextMeshPro curClicks;

    private Puzzle2Node[] allNodes; // 클리어 체크용으로만 씀

    private bool isCleared = false;
    private void Awake()
    {
        // 1. 자식 오브젝트들 중에서 Puzzle2Node를 가진 애들을 싹 긁어옵니다.
        allNodes = GetComponentsInChildren<Puzzle2Node>(true);
        Debug.Log($"총 {allNodes.Length}개의 노드를 찾았습니다.");

        UpdateClickText();
    }

    // 노드가 클릭되었을 때 호출되는 함수
    public void HandleNodeClick(Puzzle2Node clickedNode)
    {
        // 클릭된 노드 자신을 토글
        clickedNode.Toggle();

        // 해당 노드에 연결된 이웃노드 모두 토글
        foreach (Puzzle2Node neighbor in clickedNode.neighbors)
        {
            if (neighbor != null)
            {
                neighbor.Toggle();
            }
        }

        // 클릭 횟수 추가 및 텍스트 업데이트.
        curClickTimes++;
        UpdateClickText();

        // 모든 노드 활성화 체크 후 클리어 처리.
        if (CheckClear()) Clear();
        
    }

    protected bool CheckClear()
    {
        bool isClear = true;
        foreach (var node in allNodes)
        {
            if (!node.isOn)
                isClear = false;
        }

        if (isClear)
        {
            return true;
        }
        
        return false;
    }

    // 해당 레벨 클리어 시 (모든 노드가 IsOn상태가 될 때) 호출.
    protected void Clear()
    {
        if (isCleared) return;

        isCleared = true;
        Debug.Log("해당 레벨 클리어.");
        OnPuzzleCleared?.Invoke(curPuzzleNum);
    }

    // 해당 레벨의 노드 전부 꺼지는 리셋.
    public void Reset()
    {
        foreach (Puzzle2Node node in allNodes)
        {
            node.isOn = false;
            node.UpdateVisual();
        }
        curClickTimes = 0;
        UpdateClickText();
        Debug.Log("퍼즐 리셋!");
    }

    // 현재 노드 정보를 기반으로 솔버용 데이터를 반환.
    public (Puzzle2Node[] nodes, int[,] A, int[] b) GetSolverData()
    {
        int n = allNodes.Length;

        var nodeIndex = new System.Collections.Generic.Dictionary<Puzzle2Node, int>(n);
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

        return (allNodes, A, b);
    }

    // ── 솔버 ────────────────────────────────────────────────────────────────
    // 버튼에서 호출: 현재 퍼즐의 최소 클릭 해법을 Debug.Log로 출력
    public void Solve()
    {
        List<int> bestSol = GetBestSolution();
        if (bestSol == null) return;

        int minClicks = bestSol.Count;

        string msg = minClicks == 0
            ? "이미 완성 상태입니다!"
            : $"최소 {minClicks}번 클릭: " + string.Join(" -> ", bestSol);

        Debug.Log($"[Solver] {msg}");
    }

    public List<int> GetBestSolution()
    {
        Puzzle2 curPuzzle = this;
        var (allNodes, A, b) = curPuzzle.GetSolverData();
        int n = allNodes.Length;
        if (n == 0) { Debug.LogWarning("[Solver] 로드된 노드가 없습니다."); return null; }

        int[] bestSolution = SolveGF2(A, b, n);

        if (bestSolution == null)
        {
            Debug.Log("[Solver] 현재 상태에서 해가 존재하지 않습니다.");
            return null;
        }

        var answer = new List<int>();
        for (int i = 0; i < n; i++)
        {
            if (bestSolution[i] != 1) continue;
            answer.Add(i + 1);
        }
        return answer;
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

    public void UpdateClickText()
    {
        if (curClicks != null)
            curClicks.text = "Cur Clicks: " + curClickTimes;
    }
}
