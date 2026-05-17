using UnityEngine;
using System.IO;

public class MannequinCapturer : MonoBehaviour
{
    [Header("Capture Settings")]
    public Camera captureCamera;
    public int width = 1920;
    public int height = 1080;
    public string savePath = "Assets/04. Data/Captures/Mannequin.png";

    // 인스펙터에서 캡처할 레이어를 선택
    [Header("Layer Settings")]
    public LayerMask captureMask;


    bool isActive = false;

    public void Activate() => isActive = true;
    public void Deactivate() => isActive = false;

    public void OnSubmitClick()
    {
        if (!isActive) return;
        MannequinManager.Instance.MannequinDeactivate();
        MannequinManager.Instance.checkReal.SetActive(true);
    }

    public Texture2D Capture()
    {
        // 카메라의 현재 컬링 마스크 저장
        int originalMask = captureCamera.cullingMask;

        // 캡처용 렌더 텍스처 준비
        RenderTexture rt = new RenderTexture(width, height, 24);
        captureCamera.targetTexture = rt;

        // 카메라가 인스펙터에서 지정한 레이어만 보게 함
        captureCamera.cullingMask = captureMask;

        // 렌더링 실행
        captureCamera.Render();

        // 픽셀 데이터 읽기
        RenderTexture.active = rt;
        Texture2D result = new Texture2D(width, height, TextureFormat.RGB24, false);
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        result.Apply();

        // 뒷정리
        captureCamera.targetTexture = null;
        RenderTexture.active = null;
        captureCamera.cullingMask = originalMask;

        Destroy(rt);

        return result;
    }

    public void CaptureToFile() => CaptureToFile(savePath);

    public void CaptureToFile(string path)
    {
        string dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        Texture2D tex = Capture(); // 마네킹 정면에서 찍은 Texture2D 파일. 여기 이후에 윤규랑 연결하면 됨.
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Destroy(tex);
        Debug.Log($"[MannequinCapturer] 특정 레이어({captureMask.value}) 캡처 완료: {path}");
        MannequinManager.Instance.MannequinSubmit();
    }
}