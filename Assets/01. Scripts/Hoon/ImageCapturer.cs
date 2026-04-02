using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class ImageCapturer : MonoBehaviour
{
    [Header("설정")]
    public Camera targetCamera; // 이미지를 뽑을 카메라
    public int resWidth = 512;  // LLM은 512~1024면 충분합니다 (용량 절약)
    public int resHeight = 512;

    [Header("저장 경로 설정")]
    [Tooltip("Assets 폴더 기준 상대 경로 (예: Captures/LLM)")]
    public string folderName = "04. Data/Captures";

    public void CaptureForLLM()
    {
        string directoryPath = Path.Combine(Application.dataPath, folderName);
        // 1. 카메라가 그릴 가상 캔버스(RenderTexture) 생성
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        targetCamera.targetTexture = rt;

        // 2. 텍스처 데이터 공간 생성
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);

        // 3. 렌더링 및 픽셀 읽기
        targetCamera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        screenShot.Apply();

        // 4. 뒷정리 (메모리 누수 방지 핵심)
        targetCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        // 5. 파일 생성. 품질 85% 정도로 압축
        byte[] bytes = screenShot.EncodeToJPG(85);

        string fileName = $"LLM_Input_{System.DateTime.Now:yyyyMMdd_HHmmss}.jpg";

        string fullPath = Path.Combine(directoryPath, fileName);

        // 5. 저장 실행 (directoryPath 대신 fullPath 사용!)
        File.WriteAllBytes(fullPath, bytes);

        Debug.Log($"[LLM 이미지 생성 완료]\n경로: {fullPath}");

        // 생성된 Texture2D도 메모리에서 해제
        Destroy(screenShot);
    }

    // 테스트용으로 스페이스바 누르면 캡처
    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            CaptureForLLM();
        }
    }
}