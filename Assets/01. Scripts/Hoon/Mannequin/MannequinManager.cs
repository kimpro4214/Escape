using System.Collections;
using UnityEngine;

public class MannequinManager : MonoBehaviour, IInteractable
{

    [Header("Cameras")]
    public Camera playerCamera;
    public Camera mannequinCamera;   // 위치/방향 기준점으로만 사용

    [Header("References")]
    public MannequinPoser poser;
    public MannequinCapturer capturer;

    [Header("Player")]
    public GameObject player;

    [Header("Interaction Control")]
    public bool canInteract = true;

    bool isInteracting = false;
    CursorLockMode originalLockState;
    bool originalCursorVisible;

    // 메인 카메라 원래 위치/회전 저장
    Vector3    savedPosition;
    Quaternion savedRotation;
    Transform  savedParent;
    public void OnInteract()
    {
        if (!canInteract || isInteracting) return;

        // 커서 상태 저장
        originalLockState     = Cursor.lockState;
        originalCursorVisible = Cursor.visible;

        // 메인 카메라 원래 상태 저장
        savedPosition = playerCamera.transform.position;
        savedRotation = playerCamera.transform.rotation;
        savedParent   = playerCamera.transform.parent;

        // 메인 카메라를 마네킹 카메라 위치/방향으로 이동 (설정은 그대로)
        playerCamera.transform.SetParent(null);
        playerCamera.transform.SetPositionAndRotation(
            mannequinCamera.transform.position,
            mannequinCamera.transform.rotation
        );

        if (player != null) player.SetActive(false);

        // 커서 해제
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        if (poser != null)    poser.Activate();
        if (capturer != null) capturer.Activate();

        isInteracting = true;
    }

    void Update()
    {
        if (isInteracting && Input.GetKeyDown(KeyCode.Escape))
            MannequinExit();
    }

    public void MannequinDestroy()
    { // 플레이어 시점으로 돌아간 후 마네킹 오브젝트 전체 비활성화. 여기에 파티클 넣어도 됨.
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
        // 메인 카메라 원래 위치/방향 복구
        playerCamera.transform.SetParent(savedParent);
        playerCamera.transform.SetPositionAndRotation(savedPosition, savedRotation);

        if (player != null) player.SetActive(true);

        // 커서 복구
        Cursor.lockState = originalLockState;
        Cursor.visible   = originalCursorVisible;

        if (poser != null)    poser.Deactivate();
        if (capturer != null) capturer.Deactivate();

        isInteracting = false;
    }


void Start()
    {
        var pm = FindAnyObjectByType<PlayerMovement>();
        if (pm != null)
        {
            player        = pm.gameObject;
            playerCamera  = pm.GetComponentInChildren<Camera>();
        }
    }
}
