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

    // 현재 그리기 모드인지 상태 저장
    private bool isDrawingMode = false;


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


        // 커서 상태 변경
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("그림 그리기 모드 진입");
    }

    // 나중에 그리기 모드에서 나갈 때 부를 함수
    public void ExitDrawingMode()
    {
        isDrawingMode = false;
        playerOjbect.SetActive(true);

        // 카메라 스위칭 복구
        if (puzzleCamera != null) puzzleCamera.gameObject.SetActive(false);
        if (playerCamera != null) playerCamera.gameObject.SetActive(true);

        // 커서 상태 복구 (원래 상태로)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("그림 그리기 모드 종료");
    }
}