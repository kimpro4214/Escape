using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Hyeongyu
{
    public class BookViewerUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject[] pages;
        [SerializeField] private InputActionReference closeAction;
        [SerializeField] private float fadeDuration = 0.3f;

        private int _currentPage;
        private bool _isOpen;
        private int _openedFrame;
        private BookInteractable _owner;

        public void SetOwner(BookInteractable owner)
        {
            _owner = owner;
        }

        private void Awake()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        private void Update()
        {
            if (!_isOpen) return;
            if (Time.frameCount <= _openedFrame + 2) return;
            if (closeAction != null && closeAction.action.WasPressedThisFrame())
                Close();
        }

        public void Open()
        {
            _currentPage = 0;
            RefreshPages();
            Debug.Log("[BookViewerUI] Open() called");
            StartCoroutine(FadeIn());
        }

        public void NextPage()
        {
            if (_currentPage >= pages.Length - 1) return;
            pages[_currentPage].SetActive(false);
            _currentPage++;
            pages[_currentPage].SetActive(true);
        }

        public void PrevPage()
        {
            if (_currentPage <= 0) return;
            pages[_currentPage].SetActive(false);
            _currentPage--;
            pages[_currentPage].SetActive(true);
        }

        private void Close()
        {
            _isOpen = false;
            StartCoroutine(FadeOut());
        }

        private void RefreshPages()
        {
            for (int i = 0; i < pages.Length; i++)
                pages[i].SetActive(i == 0);
        }

        private IEnumerator FadeIn()
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            float t = 0f;
            Debug.Log("[BookViewerUI] FadeIn started");
            while (t < 1f)
            {
                t += Time.deltaTime / fadeDuration;
                canvasGroup.alpha = Mathf.Clamp01(t);
                yield return null;
            }
            canvasGroup.alpha = 1f;
            _openedFrame = Time.frameCount;
            _isOpen = true;
            Debug.Log("[BookViewerUI] FadeIn complete, _isOpen=true");
        }

        private IEnumerator FadeOut()
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            float t = 1f;
            while (t > 0f)
            {
                t -= Time.deltaTime / fadeDuration;
                canvasGroup.alpha = Mathf.Clamp01(t);
                yield return null;
            }
            canvasGroup.alpha = 0f;
            _owner?.OnBookViewerClosed();
        }
    }
}
