using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Hyeongyu
{
    public class MagicCircleJudger : MonoBehaviour
    {
        [Header("판정 서비스")]
        [SerializeField] private VisionJudgeService visionJudgeService;

        [Header("캡처 대상")]
        [SerializeField] private Camera drawCamera;
        [SerializeField] private RectTransform drawArea;
        [SerializeField] private int resolutionScale = 4;

        [Header("결과 UI")]
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private GameObject hintPanel;
        [SerializeField] private TextMeshProUGUI hintText;

        public void CaptureAndJudge()
        {
            StartCoroutine(JudgeRoutine());
        }

        public void OnHintDismissed()
        {
            if (hintPanel != null) hintPanel.SetActive(false);
            DrawManager.Instance.DrawNo();
        }

        private IEnumerator JudgeRoutine()
        {
            yield return new WaitForEndOfFrame();

            string base64 = CaptureToBase64Jpeg();

            if (loadingPanel != null) loadingPanel.SetActive(true);

            var task = visionJudgeService.Judge(base64);
            while (!task.IsCompleted) yield return null;

            if (loadingPanel != null) loadingPanel.SetActive(false);

            var result = task.Result;
            if (result.isPass)
            {
                DrawManager.Instance.DrawSubmit();
            }
            else
            {
                if (hintPanel != null)
                {
                    if (hintText != null) hintText.text = result.hint;
                    hintPanel.SetActive(true);
                }
                else
                {
                    DrawManager.Instance.DrawNo();
                }
            }
        }

        private string CaptureToBase64Jpeg()
        {
            int width = (int)(drawArea.rect.width * resolutionScale);
            int height = (int)(drawArea.rect.height * resolutionScale);

            RenderTexture originalRT = drawCamera.targetTexture;
            RenderTexture rt = new RenderTexture(width, height, 24) { antiAliasing = 4 };

            drawCamera.targetTexture = rt;
            drawCamera.Render();

            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            drawCamera.targetTexture = originalRT;
            RenderTexture.active = null;
            Destroy(rt);

            byte[] bytes = tex.EncodeToJPG(75);
            Destroy(tex);
            return Convert.ToBase64String(bytes);
        }
    }
}
