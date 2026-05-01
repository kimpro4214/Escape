using UnityEngine;
using System.IO;

public class MannequinCapturer : MonoBehaviour
{
    [Header("Capture Settings")]
    public Camera captureCamera;
    public int width = 1920;
    public int height = 1080;
    public string savePath = "Assets/Captures/capture.png";

    // 캡처 후 Texture2D 반환 (호출 측에서 Destroy 책임)
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            CaptureToFile();
    }

    
public Texture2D Capture()
    {
        RenderTexture rt = new RenderTexture(width, height, 24);
        captureCamera.targetTexture = rt;
        captureCamera.Render();

        RenderTexture.active = rt;
        Texture2D result = new Texture2D(width, height, TextureFormat.RGB24, false);
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        result.Apply();

        captureCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        return result;
    }

    // 인스펙터에서 지정한 경로로 저장
    public void CaptureToFile()
    {
        CaptureToFile(savePath);
    }

    // 코드에서 경로 지정해서 저장
    public void CaptureToFile(string path)
    {
        string dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        Texture2D tex = Capture();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Destroy(tex);
        Debug.Log($"[MannequinCapturer] 저장 완료: {path}");
    }
}
