using DG.Tweening;
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

    [Header("카메라 전환")]
    [SerializeField] float transitionDuration = 0.8f;
    [SerializeField] Ease transitionEase = Ease.InOutQuad;

    private Camera playerCamera;
    private GameObject playerOjbect;
    private PlayerMovement playerMovementScript;
    private PlayerCamera playerCameraScript;
    private MeshRenderer[] playerMeshRenderers;

    public DrawSystem drawSystem;
    public DrawCapturer drawCapturer;

    public bool canInteract = true;
    private bool isDrawingMode = false;
    private bool isTransitioning = false;

    // 원래 카메라 트랜스폼 저장
    private Vector3 savedPlayerCamPos;
    private Quaternion savedPlayerCamRot;
    private Transform savedPlayerCamParent;

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

    private void Update()
    {
        if (isDrawingMode && !isTransitioning && Input.GetKeyDown(KeyCode.Escape))
            ExitDrawingMode();
    }

    public void OnInteract()
    {
        if (!canInteract || isDrawingMode || isTransitioning) return;
        if (!ScreenHintService.Instance.isProcessing) ScreenHintService.Instance.curIndex = 0;

        EnterDrawingMode();
    }

    // ───────────────────────── 진입 ─────────────────────────
    private void EnterDrawingMode()
    {
        isDrawingMode = true;
        isTransitioning = true;

        // 플레이어 조작 먼저 막기
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        if (playerCameraScript != null) playerCameraScript.enabled = false;

        // 플레이어 카메라 원래 트랜스폼 저장
        Transform camT = playerCamera.transform;
        savedPlayerCamParent = camT.parent;
        savedPlayerCamPos = camT.localPosition;
        savedPlayerCamRot = camT.localRotation;

        // 월드 스페이스로 풀기
        camT.SetParent(null);

        // 커서 해제
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 플레이어 카메라에서 드로우 카메라로 전환
        Transform target = drawCamera.transform;

        Sequence seq = DOTween.Sequence();
        seq.Append(camT.DOMove(target.position, transitionDuration).SetEase(transitionEase));
        seq.Join(camT.DORotateQuaternion(target.rotation, transitionDuration).SetEase(transitionEase));
        seq.OnComplete(() =>
        {
            // 드로우 카메라 ON, 플레이어 카메라 OFF
            drawCamera.gameObject.SetActive(true);
            playerCamera.enabled = false;

            // 플레이어 카메라 원래 부모로 복귀
            camT.SetParent(savedPlayerCamParent);
            camT.localPosition = savedPlayerCamPos;
            camT.localRotation = savedPlayerCamRot;

            // 렌더러 끄기
            SetPlayerMeshRenderersEnabled(false);

            // 드로우 시스템 활성화
            DrawButtonController.Instance.ActivateButtons();
            if (drawSystem != null) drawSystem.Activate();

            isTransitioning = false;
            Debug.Log("그림 그리기 모드 진입");
        });
    }

    // ───────────────────────── 퇴장 ─────────────────────────
    public void ExitDrawingMode()
    {
        if (!isDrawingMode || isTransitioning) return;

        isTransitioning = true;

        // 드로우 시스템 비활성화
        if (drawSystem != null) drawSystem.Deactivate();
        DrawButtonController.Instance.DeactivateButtons();

        // 플레이어 카메라를 드로우 카메라 위치에서 시작
        Transform camT = playerCamera.transform;
        camT.SetParent(null);
        camT.position = drawCamera.transform.position;
        camT.rotation = drawCamera.transform.rotation;

        // 드로우 카메라 OFF, 플레이어 카메라 ON
        drawCamera.gameObject.SetActive(false);
        playerCamera.enabled = true;

        // 렌더러 복구
        SetPlayerMeshRenderersEnabled(true);

        // 원래 위치로 보간
        Vector3 worldTargetPos = savedPlayerCamParent.TransformPoint(savedPlayerCamPos);
        Quaternion worldTargetRot = savedPlayerCamParent.rotation * savedPlayerCamRot;

        Sequence seq = DOTween.Sequence();
        seq.Append(camT.DOMove(worldTargetPos, transitionDuration).SetEase(transitionEase));
        seq.Join(camT.DORotateQuaternion(worldTargetRot, transitionDuration).SetEase(transitionEase));
        seq.OnComplete(() =>
        {
            // 원래 부모로 복귀
            camT.SetParent(savedPlayerCamParent);
            camT.localPosition = savedPlayerCamPos;
            camT.localRotation = savedPlayerCamRot;

            // 플레이어 조작 복구
            if (playerMovementScript != null) playerMovementScript.enabled = true;
            if (playerCameraScript != null) playerCameraScript.enabled = true;

            // 커서 복구
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            isDrawingMode = false;
            isTransitioning = false;
            Debug.Log("그림 그리기 모드 종료");
        });
    }

    // ───────────────────────── 유틸 ─────────────────────────
    private void SetPlayerMeshRenderersEnabled(bool value)
    {
        if (playerMeshRenderers == null) return;
        for (int i = 0; i < playerMeshRenderers.Length; i++)
            if (playerMeshRenderers[i] != null) playerMeshRenderers[i].enabled = value;
    }

    private void SetPlayerComponentsEnabled(bool value)
    {
        if (playerMovementScript != null) playerMovementScript.enabled = value;
        if (playerCameraScript != null) playerCameraScript.enabled = value;
        SetPlayerMeshRenderersEnabled(value);
        if (playerCamera != null) playerCamera.enabled = value;
    }

    // ───────────────────────── 기존 기능 그대로 ─────────────────────────
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

    public void CheckAnswer(string answerText)
    {
        if (answerText == "fail") secondRoomStep.OnPuzzleFailed();
        else if (answerText == "pass") secondRoomStep.OnPuzzleSolved();
        else Debug.Log("[DrawManager] 답변이 fail이나 pass가 아님.");
    }
}