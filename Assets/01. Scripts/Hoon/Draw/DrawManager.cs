using UnityEngine;

public class DrawManager : MonoBehaviour, IInteractable
{
    [Header("카메라 설정")]
    [Tooltip("플레이어 씬에서 쓰는 일반 카메라 (Player 자식 카메라 것)")]
    public Camera playerCamera;

    [Tooltip("그림그리기 퍼즐에서 사용하는 카메라")]
    public Camera puzzleCamera;

    [Header("플레이어 오브젝트")]
    public GameObject playerOjbect;

    public UIDrawer uiDrawer;

    [Header("Interaction Control")]
    public bool canInteract = true;

    private bool isDrawingMode = false;

    public void OnInteract()
    {
        if (!canInteract || isDrawingMode) return;

        EnterDrawingMode();
    }

    private void EnterDrawingMode()
    {
        isDrawingMode = true;
        playerOjbect.SetActive(false);

        // 카메라 전환
        if (playerCamera != null) playerCamera.gameObject.SetActive(false);
        if (puzzleCamera != null) puzzleCamera.gameObject.SetActive(true);

        // 커서 보임 설정
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (uiDrawer != null) uiDrawer.Activate();

        Debug.Log("그림 그리기 모드 진입");
    }

    // 나중에 그림그리기 모드에서 나올 때 부르는 함수
    public void ExitDrawingMode()
    {
        isDrawingMode = false;
        playerOjbect.SetActive(true);

        // 카메라 전환 복구
        if (puzzleCamera != null) puzzleCamera.gameObject.SetActive(false);
        if (playerCamera != null) playerCamera.gameObject.SetActive(true);

        // 커서 원래 상태 복구 (잠금 상태로)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (uiDrawer != null) uiDrawer.Deactivate();

        Debug.Log("그림 그리기 모드 종료");
    }

    public void DrawDestroy()
    {
        if (isDrawingMode) ExitDrawingMode();
        transform.parent.gameObject.SetActive(false);
    }

    public void DrawSubmit()
    {
        ExitDrawingMode();
        canInteract = false;
    }

    public void DrawEnable() => canInteract = true;
}
