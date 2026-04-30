using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDrawer : MonoBehaviour
{
    [Header("설정")]
    public RectTransform drawArea;
    public GameObject linePrefab;
    public float minDistance = 0.1f;
    public float zDepth = 9f;

    private LineRenderer currentLine;
    private Camera mainCam;

    private Stack<GameObject> drawnLines = new Stack<GameObject>();

    void Awake()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        //마우스 클릭 시작
        if (Input.GetMouseButtonDown(0))
        {
            if (IsMouseInDrawArea())
            {
                StartDrawing();
            }
        }

        //드래그 중
        if (Input.GetMouseButton(0) && currentLine != null)
        {
            if (IsMouseInDrawArea())
            {
                UpdateDrawing();
            }
        }

        //마우스 뗌
        if (Input.GetMouseButtonUp(0))
        {
            currentLine = null;
        }
    }

    private bool IsMouseInDrawArea()
    {
        return RectTransformUtility.RectangleContainsScreenPoint(drawArea, Input.mousePosition, mainCam);
    }

    private void StartDrawing()
    {
        // 선 프리펩 생성
        GameObject lineGo = Instantiate(linePrefab);

        // 방금 만든 선을 기억 상자(Stack)에 맨 위로 밀어 넣음
        drawnLines.Push(lineGo);

        currentLine = lineGo.GetComponent<LineRenderer>();
        currentLine.positionCount = 0;
        UpdateDrawing();
    }

    private void UpdateDrawing()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = zDepth;
        Vector3 worldPos = mainCam.ScreenToWorldPoint(mousePos);

        if (currentLine.positionCount > 0)
        {
            Vector3 lastPos = currentLine.GetPosition(currentLine.positionCount - 1);
            if (Vector3.Distance(lastPos, worldPos) < minDistance) return;
        }

        currentLine.positionCount++;
        currentLine.SetPosition(currentLine.positionCount - 1, worldPos);
    }

    // 뒤로 가기(Undo) 실행 함수
    public void UndoLastLine()
    {
        // 상자 안에 기억해둔 선이 하나라도 있다면?
        if (drawnLines.Count > 0)
        {
            // 맨 위에 있는(가장 마지막에 그린) 선을 꺼냄
            GameObject lastDrawnLine = drawnLines.Pop();

            // 만약 지금 그리고 있던 선을 취소하는 거라면, 참조를 끊어줌 (버그 방지)
            if (currentLine != null && currentLine.gameObject == lastDrawnLine)
            {
                currentLine = null;
            }

            // 씬에서 해당 선 오브젝트를 아예 파괴해서 지움
            Destroy(lastDrawnLine);
        }
    }
}