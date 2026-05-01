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

        // 기존 카메라 설정 백업
        float originalSize = puzzleCamera.orthographicSize;
        bool originalOrtho = puzzleCamera.orthographic;
        RenderTexture originalRT = puzzleCamera.targetTexture;

        // 도화지 크기에 맞게 계산
        Vector3[] corners = new Vector3[4];
        drawArea.GetWorldCorners(corners);
        float worldHeight = Vector3.Distance(corners[0], corners[1]);

        int width = (int)(drawArea.rect.width * resolutionScale);
        int height = (int)(drawArea.rect.height * resolutionScale);

        // 캡처용으로 일시 변신
        puzzleCamera.orthographic = true;
        puzzleCamera.orthographicSize = worldHeight / 2f;
        puzzleCamera.aspect = (float)width / height; // 여기서 찌그러짐 발생

        RenderTexture rt = new RenderTexture(width, height, 24);
        rt.antiAliasing = 8;
        puzzleCamera.targetTexture = rt;
        puzzleCamera.Render();

        // 데이터 복사
        RenderTexture.active = rt;
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenShot.Apply();

        //카메라 원상복구
        puzzleCamera.targetTexture = originalRT;
        puzzleCamera.orthographicSize = originalSize;
        puzzleCamera.orthographic = originalOrtho;

        // 원래 비율로 돌아가기
        puzzleCamera.ResetAspect();

        RenderTexture.active = null;
        Destroy(rt);

        // 저장
        if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);
        byte[] bytes = screenShot.EncodeToPNG();
        File.WriteAllBytes(fullPath, bytes);

        Debug.Log($"캡처 성공! 경로: {fullPath}");
    }
}