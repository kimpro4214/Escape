using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedGravity = -2f;

    [Header("Step & Slope")]
    [SerializeField] private float stepOffset = 0.3f;
    [SerializeField] private float slopeLimit = 45f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 1.5f;

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;

    private CharacterController _controller;
    private Vector3 _velocity;
    private Vector2 _inputDir;

    /// <summary>
    /// CharacterController를 초기화하고 stepOffset, slopeLimit을 인스펙터 값으로 설정한다.
    /// </summary>
    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _controller.stepOffset = stepOffset;
        _controller.slopeLimit = slopeLimit;
    }

    /// <summary>
    /// 이동 및 점프 Input Action을 활성화하고 점프 이벤트 리스너를 등록한다.
    /// </summary>
    private void OnEnable()
    {
        moveAction?.action.Enable();
        jumpAction?.action.Enable();
        if (jumpAction != null)
            jumpAction.action.performed += OnJumpPerformed;
    }

    /// <summary>
    /// 이동 및 점프 Input Action을 비활성화하고 점프 이벤트 리스너를 해제한다.
    /// </summary>
    private void OnDisable()
    {
        moveAction?.action.Disable();
        jumpAction?.action.Disable();
        if (jumpAction != null)
            jumpAction.action.performed -= OnJumpPerformed;
    }

    /// <summary>
    /// 점프 입력 발생 시 지면 상태를 확인하고 수직 속도를 계산해 점프를 실행한다.
    /// </summary>
    /// <param name="ctx">Input System 콜백 컨텍스트.</param>
    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        if (_controller.isGrounded)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    /// <summary>
    /// 매 프레임 이동 입력 값을 읽어 _inputDir에 저장한다.
    /// </summary>
    private void Update()
    {
        _inputDir = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
    }

    /// <summary>
    /// 물리 주기마다 이동과 중력을 순서대로 적용한다.
    /// </summary>
    private void FixedUpdate()
    {
        ApplyMovement();
        ApplyGravity();
    }

    /// <summary>
    /// 입력 방향을 기반으로 CharacterController를 수평 이동시킨다.
    /// </summary>
    private void ApplyMovement()
    {
        Vector3 move = transform.right * _inputDir.x + transform.forward * _inputDir.y;
        _controller.Move(move * moveSpeed * Time.fixedDeltaTime);
    }

    /// <summary>
    /// 지면 여부에 따라 중력을 누적하거나 초기화하고, 수직 속도를 적용한다.
    /// </summary>
    private void ApplyGravity()
    {
        if (_controller.isGrounded)
        {
            _velocity.y = groundedGravity;
        }
        else
        {
            _velocity.y += gravity * Time.fixedDeltaTime;
        }

        _controller.Move(_velocity * Time.fixedDeltaTime);
    }
}
