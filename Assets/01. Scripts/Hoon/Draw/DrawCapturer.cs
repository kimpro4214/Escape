using UnityEngine;
using System.Collections;
using System.IO;

public class DrawCapturer : MonoBehaviour
{
    [Header("연결")]
    public Camera puzzleCamera;
    public RectTransform drawArea;

    [Header("저장 설정")]
    public string savePath = "Assets/04. Data/Captures/Capture_0.png";

    // 인스펙터에서 편하게 호출하기 위한 컨텍스트 메뉴
    [ContextMenu("Capture Drawing")]
    public void CaptureAndSave()
    {
        CaptureToFile(savePath);
    }

    public void CaptureToFile(string path)
    {
        string dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        // 2. 해상도 결정 (스케일이 너무 크면 8K가 되니 주의하세요, 보통 1이나 2가 적당합니다)
        int width = (int)drawArea.rect.width;
        int height = (int)drawArea.rect.height;

        // 3. 렌더 텍스처 생성 및 카메라 세팅
        RenderTexture rt = new RenderTexture(width, height, 24);
        rt.antiAliasing = 8;

        RenderTexture originalRT = puzzleCamera.targetTexture;
        puzzleCamera.targetTexture = rt;

        // 4. 강제 렌더링 실시 (MannequinCapturer와 동일한 방식)
        puzzleCamera.Render();

        // 5. 픽셀 데이터 추출
        RenderTexture.active = rt;
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenShot.Apply();

        // 6. 뒷정리 (메모리 누수 방지)
        puzzleCamera.targetTexture = originalRT;
        RenderTexture.active = null;
        Destroy(rt);

        // 7. 파일 저장
        byte[] bytes = screenShot.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        Destroy(screenShot); // 생성한 텍스처 오브젝트 메모리 해제

        Debug.Log($"[DrawCapturer] 캡처 성공 경로: {path}");

        // 8. 다음 스텝 매니저 호출
        if (DrawManager.Instance != null && DrawManager.Instance.secondRoomStep != null)
        {
            DrawManager.Instance.secondRoomStep.OnSubmitDrawing();
        }
    }
}