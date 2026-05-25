using UnityEngine;
using System.IO;

public class DrawCapturer : MonoBehaviour
{
    [Header("연결")]
    public Camera puzzleCamera;

    [Header("저장 설정")]
    public string savePath = "Assets/04. Data/Captures/Capture_0.png";

    [Header("캡처 설정")]
    [Tooltip("캡처 해상도 (픽셀)")]
    public int captureWidth = 512;
    public int captureHeight = 512;

    [Tooltip("배경 색상")]
    public Color backgroundColor = Color.white;

    [Tooltip("캡처할 레이어")]
    public LayerMask captureLayers = ~0; // 기본: Everything, 인스펙터에서 UI만 선택

    [Tooltip("안티앨리어싱 (1, 2, 4, 8)")]
    [Range(1, 8)]
    public int antiAliasing = 4;

    [ContextMenu("Capture Drawing")]
    public void CaptureAndSave()
    {
        CaptureToFile(savePath);
    }

    public void CaptureToFile(string path)
    {
        // 폴더 생성
        string dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        // 렌더 텍스처 생성 (인스펙터에서 설정한 해상도 사용)
        RenderTexture rt = new RenderTexture(captureWidth, captureHeight, 24);
        rt.antiAliasing = antiAliasing;

        // 원래 카메라 설정 백업
        RenderTexture originalRT = puzzleCamera.targetTexture;
        CameraClearFlags originalFlags = puzzleCamera.clearFlags;
        Color originalBG = puzzleCamera.backgroundColor;
        int originalMask = puzzleCamera.cullingMask;
        float originalOrthoSize = puzzleCamera.orthographicSize;
        Rect originalRect = puzzleCamera.rect;

        // 캡처용 설정
        puzzleCamera.targetTexture = rt;
        puzzleCamera.clearFlags = CameraClearFlags.SolidColor;
        puzzleCamera.backgroundColor = backgroundColor;
        puzzleCamera.cullingMask = captureLayers;
        puzzleCamera.rect = new Rect(0, 0, 1, 1);

        
        puzzleCamera.Render();
        RenderTexture.active = rt;
        Texture2D screenShot = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);
        screenShot.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
        screenShot.Apply();

        // 카메라 설정 복원
        puzzleCamera.targetTexture = originalRT;
        puzzleCamera.clearFlags = originalFlags;
        puzzleCamera.backgroundColor = originalBG;
        puzzleCamera.cullingMask = originalMask;
        puzzleCamera.orthographicSize = originalOrthoSize;
        puzzleCamera.rect = originalRect;
        RenderTexture.active = null;
        Destroy(rt);

        // 파일 저장
        byte[] bytes = screenShot.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        Destroy(screenShot);

        Debug.Log($"[DrawCapturer] 캡처 완료 ({captureWidth}x{captureHeight}): {path}");

        if (DrawManager.Instance != null && DrawManager.Instance.secondRoomStep != null)
            DrawManager.Instance.secondRoomStep.OnSubmitDrawing();
    }
}