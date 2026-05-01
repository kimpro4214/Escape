using UnityEngine;

public class StartDraw : MonoBehaviour, IInteractable
{
    [Header("카메라 설정")]
    [Tooltip("플레이어가 평소에 쓰는 메인 카메라 (Player 안의 카메라 등)")]
    public Camera playerCamera;

    [Tooltip("그림판을 비추는 드로잉 전용 카메라")]
    public Camera puzzleCamera;

    [Header("UI 설정")]
    [Tooltip("그림 그리기 시스템을 담당하는 캔버스나 오브젝트 (옵션)")]

    [Header("플레이어 오브젝트")]
    public GameObject playerOjbect;

    public GameObject drawingUI;

    // 현재 그리기 모드인지 상태 저장
    private bool isDrawingMode = false;

    private void Start()
    {
        // 시작할 때 드로잉 카메라와 UI는 꺼둡니다.
        if (puzzleCamera != null) puzzleCamera.gameObject.SetActive(false);
        if (drawingUI != null) drawingUI.SetActive(false);
    }

    public void OnInteract()
    {
        if (isDrawingMode) return; // 이미 그리기 모드면 무시

        EnterDrawingMode();
    }

    private void EnterDrawingMode()
    {
        isDrawingMode = true;
        playerOjbect.SetActive(false);

        // 카메라 스위칭
        if (playerCamera != null) playerCamera.gameObject.SetActive(false);
        if (puzzleCamera != null) puzzleCamera.gameObject.SetActive(true);

        // 그리기 UI 켜기
        if (drawingUI != null) drawingUI.SetActive(true);

        // 3. 커서 상태 변경 (FPS 뷰라면 필수!)
        // 화면 가운데 고정된 커서를 풀어서 그림을 그릴 수 있게 해줍니다.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 플레이어 움직임 멈추기

        Debug.Log("그림 그리기 모드 진입!");
    }

    // 나중에 그리기 모드에서 나갈 때 부를 함수 (예: '나가기' 버튼 클릭)
    public void ExitDrawingMode()
    {
        isDrawingMode = false;
        playerOjbect.SetActive(true);

        // 1. 카메라 스위칭 복구
        if (puzzleCamera != null) puzzleCamera.gameObject.SetActive(false);
        if (playerCamera != null) playerCamera.gameObject.SetActive(true);

        // 2. 그리기 UI 끄기
        if (drawingUI != null) drawingUI.SetActive(false);

        // 3. 커서 상태 복구 (원래 상태로)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 4. (선택) 플레이어 움직임 다시 켜기
        // 예: FindObjectOfType<PlayerMovement>().enabled = true;

        Debug.Log("그림 그리기 모드 종료!");
    }
}