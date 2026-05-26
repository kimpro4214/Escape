using DG.Tweening;
//using Unity.VisualScripting;
using UnityEngine;
using static MCPForUnity.External.Tommy.TomlInteger;

public class MannequinManager : MonoBehaviour, IInteractable
{
    public static MannequinManager Instance;

    [Header("Cameras")]
    public Camera mannequinCamera;

    [Header("References")]
    public MannequinPoser poser;
    public MannequinCapturer capturer;
    public Exiter exiter;

    [Header("Player")]
    public GameObject player;

    [Header("Check Real")]
    public GameObject checkReal;

    [Header("파괴 파티클")]
    public GameObject destroyParticle;

    [Header("Static Mannequin")]
    public GameObject staticMannequin;

    [Header("카메라 전환")]
    [SerializeField] float transitionDuration = 0.8f;
    [SerializeField] Ease transitionEase = Ease.InOutQuad;

    public bool canInteract = true;
    bool isInteracting = false;
    bool isTransitioning = false;

    CursorLockMode originalLockState;
    bool originalCursorVisible;

    // 퇴장 시 복귀할 원래 트랜스폼
    Vector3 savedPlayerCamPos;
    Quaternion savedPlayerCamRot;
    Transform savedPlayerCamParent;

    PlayerMovement playerMovementScript;
    PlayerCamera playerCameraScript;
    MeshRenderer[] playerMeshRenderers;
    Camera playerCamera;

    public ThirdRoomStep thirdRoomStep;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        mannequinCamera.gameObject.SetActive(false);
    }

    void Start()
    {
        var pm = FindAnyObjectByType<PlayerMovement>();
        thirdRoomStep = FindAnyObjectByType<ThirdRoomStep>();
        if (pm != null)
        {
            player = pm.gameObject;
            playerCamera = pm.GetComponentInChildren<Camera>();
            playerMovementScript = pm;
            playerCameraScript = player.GetComponentInChildren<PlayerCamera>(true);
            playerMeshRenderers = player.GetComponentsInChildren<MeshRenderer>(true);
        }
        else
        {
            Debug.LogWarning("[MannequinManager] PlayerMovement를 가진 오브젝트를 찾지 못했습니다.");
        }
        checkReal.SetActive(false);
    }

    void Update()
    {
        if (isInteracting && !isTransitioning && Input.GetKeyDown(KeyCode.Escape))
            MannequinExit();
    }

    // ───────────────────────── 진입 ─────────────────────────
    public void OnInteract()
    {
        if (!canInteract || isInteracting || isTransitioning) return;
        if (!ScreenHintService.Instance.isProcessing) ScreenHintService.Instance.curIndex = 1;

        isInteracting = true;
        isTransitioning = true;

        // 커서 상태 저장
        originalLockState = Cursor.lockState;
        originalCursorVisible = Cursor.visible;

        // 플레이어 조작만 먼저 막기 (카메라는 아직 살려둠)
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        if (playerCameraScript != null) playerCameraScript.enabled = false;

        // 플레이어 카메라의 원래 로컬 트랜스폼 저장
        Transform camT = playerCamera.transform;
        savedPlayerCamParent = camT.parent;
        savedPlayerCamPos = camT.localPosition;
        savedPlayerCamRot = camT.localRotation;

        // 보간을 위해 월드 스페이스로 풀기
        camT.SetParent(null);

        // 커서 해제
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // DOTween 보간: 플레이어 카메라 → 마네킹 카메라 위치/회전
        Transform target = mannequinCamera.transform;

        Sequence seq = DOTween.Sequence();
        seq.Append(camT.DOMove(target.position, transitionDuration).SetEase(transitionEase));
        seq.Join(camT.DORotateQuaternion(target.rotation, transitionDuration).SetEase(transitionEase));
        seq.OnComplete(() =>
        {
            // 마네킹 카메라 ON, 플레이어 카메라 OFF
            mannequinCamera.gameObject.SetActive(true);
            playerCamera.enabled = false;

            // 플레이어 카메라를 원래 부모로 복귀 (다음 퇴장 대비)
            camT.SetParent(savedPlayerCamParent);
            camT.localPosition = savedPlayerCamPos;
            camT.localRotation = savedPlayerCamRot;

            // 나머지 플레이어 렌더러 끄기
            SetPlayerMeshRenderersEnabled(false);

            MannequinActivate();
            isTransitioning = false;
        });
    }

    // ───────────────────────── 퇴장 ─────────────────────────
    public void MannequinExit()
    {
        if (isTransitioning) return;

        Debug.Log("마네킹 Exit!");
        isTransitioning = true;

        MannequinDeactivate();

        // 플레이어 카메라를 마네킹 카메라 위치에서 시작
        Transform camT = playerCamera.transform;
        camT.SetParent(null);
        camT.position = mannequinCamera.transform.position;
        camT.rotation = mannequinCamera.transform.rotation;

        // 마네킹 카메라 OFF, 플레이어 카메라 ON
        mannequinCamera.gameObject.SetActive(false);
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
            Cursor.lockState = originalLockState;
            Cursor.visible = originalCursorVisible;

            isInteracting = false;
            isTransitioning = false;
        });
    }

    // ───────────────────────── 유틸 ─────────────────────────
    private void SetPlayerMeshRenderersEnabled(bool value)
    {
        if (playerMeshRenderers == null) return;
        for (int i = 0; i < playerMeshRenderers.Length; i++)
            if (playerMeshRenderers[i] != null) playerMeshRenderers[i].enabled = value;
    }

    // ───────────────────────── 기존 기능 그대로 ─────────────────────────
    public void MannequinActivate()
    {
        if (poser != null) poser.Activate();
        if (capturer != null) capturer.Activate();
        if (exiter != null) exiter.Activate();
    }

    public void MannequinDeactivate()
    {
        if (poser != null) poser.Deactivate();
        if (capturer != null) capturer.Deactivate();
        if (exiter != null) exiter.Deactivate();
    }

    public void MannequinDestroy()
    {
        Destroy(Instantiate(destroyParticle, transform.position, transform.rotation), 2f);
        if (isInteracting) MannequinExit();
        transform.parent.gameObject.SetActive(false);
        if (staticMannequin != null) staticMannequin.SetActive(false);
    }

    public void MannequinSubmit()
    {
        MannequinExit();
        canInteract = false;
    }

    public void MannequinEnable() => canInteract = true;

    public void OnClickYes()
    {
        capturer.CaptureToFile();
        ScreenHintService.Instance.OnHintButtonClicked();
        checkReal.SetActive(false);
    }

    public void OnClickNo()
    {
        MannequinActivate();
        checkReal.SetActive(false);
    }

    public void CheckAnswer(string answerText)
    {
        if (answerText == "fail") thirdRoomStep.OnPuzzleFailed();
        else if (answerText == "pass") thirdRoomStep.OnPuzzleSolved();
        else Debug.Log("[MannequinManager] 답변이 fail이나 pass가 아님.");
    }
}