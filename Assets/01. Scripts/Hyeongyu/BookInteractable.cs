using System.Collections;
using UnityEngine;

namespace Hyeongyu
{
    [System.Serializable]
    public class BookPageContent
    {
        [SerializeField] private string title;
        [SerializeField, TextArea] private string body;

        public string Title => title;
        public string Body => body;
    }

    public class BookInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private BookViewerUI bookViewerUI;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private BookPageContent[] pages;
        [SerializeField] private PlayerCamera playerCamera;
        [SerializeField] private float floatHeight = 0.3f;
        [SerializeField] private float floatDuration = 0.6f;

        private Vector3 _originalPosition;
        private bool _isAnimating;
        private bool _isFound;

        private void Awake()
        {
            ResolveRuntimeReferences();
        }

        public void OnInteract()
        {
            if (_isAnimating) return;

            ResolveRuntimeReferences();
            Debug.Log($"[BookInteractable] OnInteract: {name}");

            if (bookViewerUI == null)
            {
                Debug.LogError($"[BookInteractable] BookViewerUI not found for {name}.");
                return;
            }

            bookViewerUI.SetOwner(this);
            _isAnimating = true;

            if (playerMovement != null)
                playerMovement.enabled = false;
            else
                Debug.LogWarning($"[BookInteractable] PlayerMovement not found for {name}.");

            if (playerCamera != null)
                playerCamera.enabled = false;
            else
                Debug.LogWarning($"[BookInteractable] PlayerCamera not found for {name}.");

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            StartCoroutine(OpenBookSequence());
        }

        public void OnBookViewerClosed()
        {
            StartCoroutine(CloseBookSequence());

            if (_isFound)
            {
                Debug.Log($"[BookInteractable] OnInfoFound() skipped because {name} was already found.");
                return;
            }

            _isFound = true;
            SecondRoomStep secondRoomStep = FindFirstObjectByType<SecondRoomStep>();
            if (secondRoomStep == null)
            {
                Debug.LogWarning($"[BookInteractable] SecondRoomStep not found. OnInfoFound() was not called for {name}.");
                return;
            }

            Debug.Log($"[BookInteractable] OnInfoFound() requested by {name}.");
            secondRoomStep.OnInfoFound();
        }

        private IEnumerator OpenBookSequence()
        {
            _originalPosition = transform.position;
            yield return FloatTo(transform.position + Vector3.up * floatHeight, floatDuration);
            yield return new WaitForSeconds(0.2f);
            if (bookViewerUI != null)
                bookViewerUI.Open(pages);
            else
            {
                Debug.LogError($"[BookInteractable] bookViewerUI is null on {name}, restoring player");
                StartCoroutine(CloseBookSequence());
            }
        }

        private IEnumerator CloseBookSequence()
        {
            yield return FloatTo(_originalPosition, floatDuration * 0.5f);

            ResolveRuntimeReferences();
            if (playerMovement != null)
                playerMovement.enabled = true;
            if (playerCamera != null)
                playerCamera.enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _isAnimating = false;
        }

        private void ResolveRuntimeReferences()
        {
            if (bookViewerUI == null)
                bookViewerUI = FindFirstObjectByType<BookViewerUI>();

            if (playerMovement == null || playerCamera == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    if (playerMovement == null)
                        playerMovement = player.GetComponent<PlayerMovement>();
                    if (playerCamera == null)
                        playerCamera = player.GetComponent<PlayerCamera>();
                }
            }
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
