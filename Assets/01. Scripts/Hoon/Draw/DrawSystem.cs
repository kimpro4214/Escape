using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawSystem : MonoBehaviour
{
    [Header("설정")]
    public RectTransform drawArea;
    public GameObject linePrefab;
    public float minDistance = 0.1f;
    public float zDepth = 9f;

    private LineRenderer currentLine;
    [Header("도화지 카메라")]
    public Camera drawCam;

    private Stack<GameObject> drawnLines = new Stack<GameObject>();
    private bool isActive = false;

    public void Activate()   { isActive = true; }
    public void Deactivate() { isActive = false; currentLine = null; }

    void Update()
    {
        if (!isActive) return;

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
        return RectTransformUtility.RectangleContainsScreenPoint(drawArea, Input.mousePosition, drawCam);
    }

    private void StartDrawing()
    {
        // 선 프리펩 생성
        GameObject lineGo = Instantiate(linePrefab);

        // 방금 만든 선을 Stack에 push
        drawnLines.Push(lineGo);

        currentLine = lineGo.GetComponent<LineRenderer>();
        currentLine.positionCount = 0;
        UpdateDrawing();
    }

    private void UpdateDrawing()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = zDepth;
        Vector3 worldPos = drawCam.ScreenToWorldPoint(mousePos);

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
        // 그린 적이 한 번이라도 있다면
        if (drawnLines.Count > 0)
        {
            // 가장 마지막에 그린 선을 꺼냄
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