using UnityEngine;

public class DrawManager : MonoBehaviour, IInteractable
{
    public static DrawManager Instance;

    [Header("카메라 설정")]
    [Tooltip("그림그리기 퍼즐에서 사용하는 카메라")]
    public Camera drawCamera;

    [Header("파괴 파티클")]
    public GameObject destroyParticle;
    
    private Camera playerCamera;
    private GameObject playerOjbect;

    public DrawSystem uiDrawer;

    public bool canInteract = true;


    private bool isDrawingMode = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null)
        {
            playerOjbect = player.gameObject;
            playerCamera = player.GetComponentInChildren<Camera>(true);
        }
        else
        {
            Debug.LogWarning("[DrawManager] PlayerMovement를 가진 오브젝트를 찾지 못했습니다.");
        }
        drawCamera.gameObject.SetActive(false);
    }
    public void OnInteract()
    {
        if (!canInteract || isDrawingMode) return;

        EnterDrawingMode();
    }

    private void EnterDrawingMode()
    {
        drawCamera.gameObject.SetActive(true);
        isDrawingMode = true;
        playerOjbect.SetActive(false);

        // 카메라 전환
        if (playerCamera != null) playerCamera.gameObject.SetActive(false);
        if (drawCamera != null) drawCamera.gameObject.SetActive(true);

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
        drawCamera.gameObject.SetActive(false);

        // 카메라 전환 복구
        if (drawCamera != null) drawCamera.gameObject.SetActive(false);
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
        Destroy(Instantiate(destroyParticle, transform.position, transform.rotation), 2f);
        transform.parent.gameObject.SetActive(false);
    }

    public void DrawSubmit()
    {
        ExitDrawingMode();
        canInteract = false;
    }

    public void DrawEnable() => canInteract = true;

}