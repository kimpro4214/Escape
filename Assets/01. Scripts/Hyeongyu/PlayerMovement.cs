using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 7.5f;
    [SerializeField] private float acceleration = 18f;
    [SerializeField] private float deceleration = 22f;
    [SerializeField, Range(0f, 1f)] private float airControl = 0.45f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedGravity = -2f;

    [Header("Step & Slope")]
    [SerializeField] private float stepOffset = 0.3f;
    [SerializeField] private float slopeLimit = 45f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.12f;

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference sprintAction;

    private CharacterController _controller;
    private Vector3 _horizontalVelocity;
    private float _verticalVelocity;
    private Vector2 _inputDir;
    private float _lastGroundedTime = float.NegativeInfinity;
    private float _lastJumpPressedTime = float.NegativeInfinity;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _controller.stepOffset = stepOffset;
        _controller.slopeLimit = slopeLimit;
    }

    private void OnEnable()
    {
        moveAction?.action.Enable();
        jumpAction?.action.Enable();
        sprintAction?.action.Enable();

        if (jumpAction != null)
            jumpAction.action.performed += OnJumpPerformed;
    }

    private void OnDisable()
    {
        if (jumpAction != null)
            jumpAction.action.performed -= OnJumpPerformed;

        moveAction?.action.Disable();
        jumpAction?.action.Disable();
        sprintAction?.action.Disable();
    }

    private void Update()
    {
        ReadInput();
        UpdateGroundedState();
        ApplyJump();
        ApplyGravity();
        ApplyMovement();
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        _lastJumpPressedTime = Time.time;
    }

    private void ReadInput()
    {
        Vector2 rawInput = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
        _inputDir = Vector2.ClampMagnitude(rawInput, 1f);
    }

    private void UpdateGroundedState()
    {
        if (_controller.isGrounded)
            _lastGroundedTime = Time.time;
    }

    private void ApplyJump()
    {
        bool canUseCoyoteTime = Time.time - _lastGroundedTime <= coyoteTime;
        bool hasBufferedJump = Time.time - _lastJumpPressedTime <= jumpBufferTime;

        if (!canUseCoyoteTime || !hasBufferedJump)
            return;

        _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        _lastJumpPressedTime = float.NegativeInfinity;
        _lastGroundedTime = float.NegativeInfinity;
    }

    private void ApplyGravity()
    {
        if (_controller.isGrounded)
        {
            if (_verticalVelocity < 0f)
                _verticalVelocity = groundedGravity;
        }
        else
        {
            _verticalVelocity += gravity * Time.deltaTime;
        }
    }

    private void ApplyMovement()
    {
        Vector3 targetDirection = transform.right * _inputDir.x + transform.forward * _inputDir.y;
        float targetSpeed = IsSprinting() ? sprintSpeed : moveSpeed;
        Vector3 targetVelocity = targetDirection * targetSpeed;

        float controlMultiplier = _controller.isGrounded ? 1f : airControl;
        float speedChange = (_inputDir.sqrMagnitude > 0.01f ? acceleration : deceleration) * controlMultiplier;

        _horizontalVelocity = Vector3.MoveTowards(
            _horizontalVelocity,
            targetVelocity,
            speedChange * Time.deltaTime
        );

        Vector3 motion = _horizontalVelocity;
        motion.y = _verticalVelocity;
        CollisionFlags collisionFlags = _controller.Move(motion * Time.deltaTime);

        if ((collisionFlags & CollisionFlags.Above) != 0 && _verticalVelocity > 0f)
            _verticalVelocity = 0f;
    }

    private bool IsSprinting()
    {
        return sprintAction != null && sprintAction.action.IsPressed();
    }
}
