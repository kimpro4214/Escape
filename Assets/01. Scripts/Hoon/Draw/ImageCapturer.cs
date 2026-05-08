using UnityEngine;
using System.Collections;
using System.IO;

public class ImageCapturer : MonoBehaviour
{
    [Header("연결")]
    public Camera puzzleCamera;
    public RectTransform drawArea;

    [Header("저장 설정")]
    public string savePath = "C:/Temp";
    public string fileName = "Result.png";

    [Header("품질 설정")]
    public int resolutionScale = 4;

    [ContextMenu("Capture Drawing")]
    public void CaptureAndSave()
    {
        StartCoroutine(CaptureRoutine());
    }

private IEnumerator CaptureRoutine()
    {
        yield return new WaitForEndOfFrame();

        string fullPath = Path.Combine(savePath, fileName);

        int width = (int)(drawArea.rect.width * resolutionScale);
        int height = (int)(drawArea.rect.height * resolutionScale);

        RenderTexture originalRT = puzzleCamera.targetTexture;

        RenderTexture rt = new RenderTexture(width, height, 24);
        rt.antiAliasing = 8;
        puzzleCamera.targetTexture = rt;
        puzzleCamera.Render();

        RenderTexture.active = rt;
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenShot.Apply();

        puzzleCamera.targetTexture = originalRT;
        RenderTexture.active = null;
        Destroy(rt);

        if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);
        byte[] bytes = screenShot.EncodeToPNG();
        File.WriteAllBytes(fullPath, bytes);

        Debug.Log($"캡처 성공 경로: {fullPath}");
    }
}