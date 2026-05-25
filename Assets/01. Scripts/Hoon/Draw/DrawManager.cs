using Hyeongyu;
using UnityEngine;

public class DrawManager : MonoBehaviour, IInteractable
{
    public static DrawManager Instance;

    [Header("카메라 설정")]
    [Tooltip("그림그리기 퍼즐에서 사용하는 카메라")]
    public Camera drawCamera;

    [Header("파괴 파티클")]
    public GameObject destroyParticle;

    [Header("Check Real")]
    public GameObject checkReal;

    //[Header("마법진 판정")]
    //[SerializeField] private MagicCircleJudger magicCircleJudger;
    
    private Camera playerCamera;
    private GameObject playerOjbect;
    private PlayerMovement playerMovementScript;
    private PlayerCamera playerCameraScript;
    private MeshRenderer[] playerMeshRenderers;

    public DrawSystem drawSystem;
    public DrawCapturer drawCapturer;

    public bool canInteract = true;


    private bool isDrawingMode = false;


    public SecondRoomStep secondRoomStep;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
        secondRoomStep = FindAnyObjectByType<SecondRoomStep>();
        if (player != null)
        {
            playerOjbect = player.gameObject;
            playerCamera = player.GetComponentInChildren<Camera>(true);
            playerMovementScript = player;
            playerCameraScript = playerOjbect.GetComponentInChildren<PlayerCamera>(true);
            playerMeshRenderers = playerOjbect.GetComponentsInChildren<MeshRenderer>(true);
        }
        else
        {
            Debug.LogWarning("[DrawManager] PlayerMovement를 가진 오브젝트를 찾지 못했습니다.");
        }
        drawCamera.gameObject.SetActive(false);
        checkReal.SetActive(false);
    }
    public void OnInteract()
    {
        if (!canInteract || isDrawingMode) return;

        EnterDrawingMode();
        if (!ScreenHintService.Instance.isProcessing) ScreenHintService.Instance.curIndex = 0;
    }

    private void EnterDrawingMode()
    {
        DrawButtonController.Instance.ActivateButtons();
        if (drawSystem != null) drawSystem.Activate();

        drawCamera.gameObject.SetActive(true);
        isDrawingMode = true;

        // 플레이어 비활성화 (AudioListener 유지 위해 GameObject는 끄지 않음)
        SetPlayerComponentsEnabled(false);

        // 커서 보임 설정
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("그림 그리기 모드 진입");
    }

    // 나중에 그림그리기 모드에서 나올 때 부르는 함수
    public void ExitDrawingMode()
    {
        if (drawSystem != null) drawSystem.Deactivate();
        DrawButtonController.Instance.DeactivateButtons();
        if (!isDrawingMode) return;
        isDrawingMode = false;

        SetPlayerComponentsEnabled(true);
        drawCamera.gameObject.SetActive(false);

        // 커서 원래 상태 복구 (잠금 상태로)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;


        Debug.Log("그림 그리기 모드 종료");
    }

    private void SetPlayerComponentsEnabled(bool value)
    {
        if (playerMovementScript != null) playerMovementScript.enabled = value;
        if (playerCameraScript != null) playerCameraScript.enabled = value;
        if (playerMeshRenderers != null)
        {
            for (int i = 0; i < playerMeshRenderers.Length; i++)
                if (playerMeshRenderers[i] != null) playerMeshRenderers[i].enabled = value;
        }
        if (playerCamera != null) playerCamera.enabled = value;
    }

    public void DrawDestroy()
    {
        if (isDrawingMode) ExitDrawingMode();
        drawSystem.ResetDrawing();
        Destroy(Instantiate(destroyParticle, transform.position, transform.rotation), 2f);
        transform.parent.gameObject.SetActive(false);
    }

    public void DrawSubmit()
    {
        StartCoroutine(ScreenHintService.Instance.CaptureAndHint());
        ExitDrawingMode();
        canInteract = false;
    }

    public void DrawEnable() => canInteract = true;

    public void CheckRealSubmit()
    {
        checkReal.SetActive(true);
        drawSystem.Deactivate();
        DrawButtonController.Instance.DeactivateButtons();
    }

    public void DrawYes()
    {
        checkReal.SetActive(false);
        if (drawCapturer != null) drawCapturer.CaptureAndSave();
    }

    public void DrawNo()
    {
        checkReal.SetActive(false);
        drawSystem.Activate();
        DrawButtonController.Instance.ActivateButtons();
    }

    // LLM에서 이미지 및 프롬프트 전달 후 답변이 돌아왔을 때 (answerText)
    public void CheckAnswer(string answerText)
    {
        if (answerText == "fail") secondRoomStep.OnPuzzleFailed();
        else if (answerText == "pass") secondRoomStep.OnPuzzleSolved();
        else Debug.Log("[DrawManager] 답변이 fail이나 pass가 아님.");
    }
}