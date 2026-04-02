using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerBody;
    [SerializeField] private Camera playerCamera;

    [Header("Sensitivity")]
    [SerializeField] private float sensitivityX = 0.1f;
    [SerializeField] private float sensitivityY = 0.1f;

    [Header("Input")]
    [SerializeField] private InputActionReference lookAction;

    private float _xRotation;

    /// <summary>
    /// playerCamera가 미할당 시 Main Camera로 자동 초기화한다.
    /// </summary>
    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    /// <summary>
    /// 게임 시작 시 커서를 화면 중앙에 고정하고 숨긴다.
    /// </summary>
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// 시점 Input Action을 활성화한다.
    /// </summary>
    private void OnEnable()
    {
        lookAction?.action.Enable();
    }

    /// <summary>
    /// 시점 Input Action을 비활성화한다.
    /// </summary>
    private void OnDisable()
    {
        lookAction?.action.Disable();
    }

    /// <summary>
    /// 마우스 델타를 읽어 카메라 상하(Pitch) 및 플레이어 본체 좌우(Yaw) 회전을 적용한다.
    /// 수직 회전 각도는 -80 ~ 80도로 제한된다.
    /// </summary>
    private void Update()
    {
        if (lookAction == null || playerCamera == null || playerBody == null)
            return;

        Vector2 delta = lookAction.action.ReadValue<Vector2>();

        _xRotation -= delta.y * sensitivityY;
        _xRotation = Mathf.Clamp(_xRotation, -80f, 80f);

        playerCamera.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * delta.x * sensitivityX);
    }
}
