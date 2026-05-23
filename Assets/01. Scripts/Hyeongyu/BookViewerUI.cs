using System.Collections;
using UnityEngine;
using TMPro;
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
        private BookPageContent[] _currentBookPages;
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
            Open(null);
        }

        public void Open(BookPageContent[] bookPages)
        {
            _currentBookPages = bookPages;
            _currentPage = 0;
            ApplyBookPages();
            RefreshPages();
            Debug.Log($"[BookViewerUI] Open() called with {GetPageCount()} page(s)");
            StartCoroutine(FadeIn());
        }

        public void NextPage()
        {
            if (_currentPage >= GetPageCount() - 1) return;
            SetPageActive(_currentPage, false);
            _currentPage++;
            SetPageActive(_currentPage, true);
        }

        public void PrevPage()
        {
            if (_currentPage <= 0) return;
            SetPageActive(_currentPage, false);
            _currentPage--;
            SetPageActive(_currentPage, true);
        }

        private void Close()
        {
            _isOpen = false;
            StartCoroutine(FadeOut());
        }

        private void RefreshPages()
        {
            int pageCount = GetPageCount();
            for (int i = 0; i < pages.Length; i++)
                SetPageActive(i, i == 0 && i < pageCount);
        }

        private void ApplyBookPages()
        {
            if (_currentBookPages == null) return;

            int pageCount = Mathf.Min(_currentBookPages.Length, pages.Length);
            for (int i = 0; i < pageCount; i++)
            {
                if (pages[i] == null || _currentBookPages[i] == null) continue;

                TextMeshProUGUI[] texts = pages[i].GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (TextMeshProUGUI text in texts)
                {
                    if (text.name == "Title")
                        text.text = _currentBookPages[i].Title;
                    else if (text.name == "Subtitle")
                        text.text = _currentBookPages[i].Body;
                }
            }
        }

        private int GetPageCount()
        {
            if (pages == null || pages.Length == 0) return 0;
            if (_currentBookPages == null || _currentBookPages.Length == 0) return pages.Length;
            return Mathf.Min(_currentBookPages.Length, pages.Length);
        }

        private void SetPageActive(int pageIndex, bool active)
        {
            if (pages == null || pageIndex < 0 || pageIndex >= pages.Length || pages[pageIndex] == null) return;
            pages[pageIndex].SetActive(active);
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
