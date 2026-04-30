using UnityEngine;
using System.IO;

public class ImageCapturer : MonoBehaviour
{
    [Header("연결")]
    public Camera puzzleCamera;
    public RectTransform drawArea;

    [Header("저장 설정")]
    [Tooltip("캡쳐 저장 경로")]
    public string savePath = "C:/Temp";

    [Tooltip("확장자를 포함한 png 파일 이름")]
    public string fileName = "Result.png";

    [Header("품질 설정")]
    public int resolutionScale = 4;

    [ContextMenu("Capture Drawing")]
    public void CaptureAndSave()
    {
        string fullPath = Path.Combine(savePath, fileName);

        // 해상도 설정
        int width = (int)drawArea.rect.width * resolutionScale;
        int height = (int)drawArea.rect.height * resolutionScale;

        // 렌더 텍스처 준비
        RenderTexture rt = new RenderTexture(width, height, 24);
        rt.antiAliasing = 8;
        puzzleCamera.targetTexture = rt;

        // 카메라 렌더링 및 Texture2D 복사
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
        puzzleCamera.Render();

        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenShot.Apply();

        // 정리
        puzzleCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        // 폴더가 없다면 생성
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        // PNG 저장
        byte[] bytes = screenShot.EncodeToPNG();
        File.WriteAllBytes(fullPath, bytes);

        Debug.Log($"그림 덮어쓰기 완료. 최종 경로: {fullPath}");
    }
}