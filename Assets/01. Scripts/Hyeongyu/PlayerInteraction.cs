using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactLayer = ~0;

    [Header("Input")]
    [SerializeField] private InputActionReference interactAction;

    private IInteractable _currentInteractable;

    /// <summary>
    /// playerCamera가 미할당 시 Main Camera로 자동 초기화한다.
    /// </summary>
    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    /// <summary>
    /// 상호작용 Input Action을 활성화하고 이벤트 리스너를 등록한다.
    /// </summary>
    private void OnEnable()
    {
        if (interactAction != null)
            interactAction.action.performed += OnInteractPerformed;
        interactAction?.action.Enable();
    }

    /// <summary>
    /// 상호작용 Input Action을 비활성화하고 이벤트 리스너를 해제한다.
    /// </summary>
    private void OnDisable()
    {
        if (interactAction != null)
            interactAction.action.performed -= OnInteractPerformed;
        interactAction?.action.Disable();
    }

    /// <summary>
    /// 매 프레임 화면 중앙 기준으로 Raycast를 발사해 탐지 대상을 갱신한다.
    /// </summary>
    private void Update()
    {
        DetectInteractable();
    }

    /// <summary>
    /// 카메라 뷰포트 중앙에서 Raycast를 발사해 IInteractable 오브젝트를 탐지하고
    /// _currentInteractable을 갱신한다.
    /// </summary>
    private void DetectInteractable()
    {
        if (playerCamera == null)
        {
            _currentInteractable = null;
            return;
        }

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayer))
        {
            IInteractable found = hit.collider.GetComponent<IInteractable>();

            if (found != null)
            {
                if (!ReferenceEquals(found, _currentInteractable))
                {
                    _currentInteractable = found;
                    Debug.Log($"[PlayerInteraction] Targeting: {hit.collider.gameObject.name}");
                }
            }
            else
            {
                _currentInteractable = null;
            }
        }
        else
        {
            _currentInteractable = null;
        }
    }

    /// <summary>
    /// 상호작용 입력 발생 시 현재 탐지된 IInteractable의 OnInteract()를 호출한다.
    /// </summary>
    /// <param name="ctx">Input System 콜백 컨텍스트.</param>
    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        _currentInteractable?.OnInteract();
    }
}
