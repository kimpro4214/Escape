using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Hyeongyu
{
    public class StarSymbolInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerCamera playerCamera;
        [SerializeField] private InputActionReference closeAction;
        [SerializeField] private GameObject hintUI;
        [SerializeField] private float zoomFov = 20f;
        [SerializeField] private float zoomDuration = 0.3f;

        private bool _isViewing;
        private float _originalFov;
        private Camera _cam;
        private int _enterFrame;

        private void Start()
        {
            _cam = Camera.main;
        }

        private void Update()
        {
            if (!_isViewing) return;
            if (Time.frameCount <= _enterFrame) return;
            if (closeAction != null && closeAction.action.WasPressedThisFrame())
                StartCoroutine(ExitCloseup());
        }

        public void OnInteract()
        {
            if (_isViewing) return;
            StartCoroutine(EnterCloseup());
        }

        private IEnumerator EnterCloseup()
        {
            _isViewing = true;
            _enterFrame = Time.frameCount;
            playerMovement.enabled = false;
            playerCamera.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            hintUI?.SetActive(true);

            _originalFov = _cam.fieldOfView;
            yield return LerpFov(_cam.fieldOfView, zoomFov, zoomDuration);
        }

        private IEnumerator ExitCloseup()
        {
            _isViewing = false;
            hintUI?.SetActive(false);

            yield return LerpFov(_cam.fieldOfView, _originalFov, zoomDuration);

            playerMovement.enabled = true;
            playerCamera.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            ClueManager.Instance?.OnClueFound();
        }

        private IEnumerator LerpFov(float from, float to, float duration)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                _cam.fieldOfView = Mathf.Lerp(from, to, Mathf.Clamp01(t));
                yield return null;
            }
            _cam.fieldOfView = to;
        }
    }
}
