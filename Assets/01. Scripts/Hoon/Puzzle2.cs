using UnityEngine;

public class Puzzle2 : MonoBehaviour
{
    [Header("색상 설정")]
    public Color onColor = Color.green;
    public Color offColor = new Color(0.5f, 0.5f, 0.5f);

    private Puzzle2Node[] allNodes; // 클리어 체크용으로만 씀

    private bool isCleared = false;
    private void Awake()
    {
        // 1. 자식 오브젝트들 중에서 Puzzle2Node를 가진 애들을 싹 긁어옵니다.
        allNodes = GetComponentsInChildren<Puzzle2Node>(true);
        Debug.Log($"총 {allNodes.Length}개의 노드를 찾았습니다.");
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

        // 모든 노드가 on인지 확인 후 하나라도 off라면 return.
        foreach (var node in allNodes)
        {
            if (!node.isOn)
                return;
        }
        Clear();
    }

    // 해당 레벨 클리어 시 (모든 노드가 IsOn상태가 될 때) 호출.
    protected void Clear()
    {
        if (isCleared) return;
        isCleared = true;
        Debug.Log("퍼즐 클리어. 다음 레벨 호출 시도.");
        Puzzle2Manager.instance.ActivateNextPuzzle2();
    }

    // 해당 레벨의 노드 전부 꺼지는 리셋. Puzzle2Manager가 호출해줌.
    public void Reset()
    {
        foreach (Puzzle2Node node in allNodes)
        {
            node.isOn = false;
            node.UpdateVisual();
        }
    }
}
