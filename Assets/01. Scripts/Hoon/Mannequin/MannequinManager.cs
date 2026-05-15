using System.Collections;
using UnityEngine;

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

    public bool canInteract = true;
    bool isInteracting = false;
    CursorLockMode originalLockState;
    bool originalCursorVisible;

    PlayerMovement playerMovementScript;
    PlayerCamera playerCameraScript;
    MeshRenderer[] playerMeshRenderers;
    Camera playerCamera;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        mannequinCamera.gameObject.SetActive(false);
    }
    void Start()
    {
        var pm = FindAnyObjectByType<PlayerMovement>();
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
        Debug.Log($"[Mannequin] playerCamera = {playerCamera}");
    }
    void Update()
    {
        if (isInteracting && Input.GetKeyDown(KeyCode.Escape))
            MannequinExit();
    }

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

    public void OnInteract()
    {
        if (!canInteract || isInteracting) return;

        MannequinActivate();

        // 커서 상태 저장
        originalLockState     = Cursor.lockState;
        originalCursorVisible = Cursor.visible;

        // 카메라 전환
        if (mannequinCamera != null) mannequinCamera.gameObject.SetActive(true);

        // 플레이어 비활성화 (AudioListener 유지 위해 GameObject는 끄지 않음)
        SetPlayerComponentsEnabled(false);

        // 커서 해제
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        isInteracting = true;
    }

    

    public void MannequinDestroy()
    { // 플레이어 시점으로 돌아간 후 마네킹 오브젝트 전체 비활성화.
        Destroy(Instantiate(destroyParticle, transform.position, transform.rotation), 2f);
        if (isInteracting) MannequinExit();
        transform.parent.gameObject.SetActive(false);
    }
    public void MannequinSubmit()
    {
        MannequinExit();
        canInteract = false;
    }
    public void MannequinEnable() => canInteract = true;

    public void MannequinExit()
    {
        Debug.Log("마네킹 Exit!");

        SetPlayerComponentsEnabled(true);

        // 카메라 전환 복구
        if (mannequinCamera != null) mannequinCamera.gameObject.SetActive(false);

        // 커서 복구
        Cursor.lockState = originalLockState;
        Cursor.visible   = originalCursorVisible;

        MannequinDeactivate();

        isInteracting = false;
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
}
