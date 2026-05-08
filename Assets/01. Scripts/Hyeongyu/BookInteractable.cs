using System.Collections;
using UnityEngine;

namespace Hyeongyu
{
    public class BookInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private BookViewerUI bookViewerUI;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerCamera playerCamera;
        [SerializeField] private float floatHeight = 0.3f;
        [SerializeField] private float floatDuration = 0.6f;

        private Vector3 _originalPosition;
        private bool _isAnimating;

        private void Start()
        {
            bookViewerUI?.SetOwner(this);
        }

        public void OnInteract()
        {
            if (_isAnimating) return;
            _isAnimating = true;
            playerMovement.enabled = false;
            playerCamera.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            StartCoroutine(OpenBookSequence());
        }

        public void OnBookViewerClosed()
        {
            StartCoroutine(CloseBookSequence());
            ClueManager.Instance?.OnClueFound();
        }

        private IEnumerator OpenBookSequence()
        {
            _originalPosition = transform.position;
            yield return FloatTo(transform.position + Vector3.up * floatHeight, floatDuration);
            yield return new WaitForSeconds(0.2f);
            bookViewerUI.Open();
        }

        private IEnumerator CloseBookSequence()
        {
            yield return FloatTo(_originalPosition, floatDuration * 0.5f);
            playerMovement.enabled = true;
            playerCamera.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _isAnimating = false;
        }

        private IEnumerator FloatTo(Vector3 target, float duration)
        {
            Vector3 start = transform.position;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                transform.position = Vector3.Lerp(start, target, Mathf.Clamp01(t));
                yield return null;
            }
            transform.position = target;
        }
    }
}
