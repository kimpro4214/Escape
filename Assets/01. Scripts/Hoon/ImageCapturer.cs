using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class ImageCapturer : MonoBehaviour
{
    [Header("핵심 대상 설정")]
    public Camera targetCamera;

    [Tooltip("퍼즐 오브젝트들이 속한 레이어를 선택하세요.")]
    public LayerMask puzzleLayer; // 새로 추가된 부분

    [Header("해상도 설정")]
    public int resWidth = 512;
    public int resHeight = 512;

    [Header("저장 경로 설정")]
    public string folderName = "04. Data/Captures";

    private void Start()
    {
        if (targetCamera == null) targetCamera = Camera.main;
    }

    public void CaptureOnlyPuzzle()
    {
        if (targetCamera == null) return;

        // 1. 카메라 상태 백업 (마스크, 배경색 등)
        int originalMask = targetCamera.cullingMask;
        Color originalBG = targetCamera.backgroundColor;
        CameraClearFlags originalFlags = targetCamera.clearFlags;

        // 2. 촬영용 필터 세팅
        // 특정 레이어만 보이게 하고, 나머지는 검은색(혹은 투명)으로 비웁니다.
        targetCamera.cullingMask = puzzleLayer;
        targetCamera.clearFlags = CameraClearFlags.SolidColor;
        targetCamera.backgroundColor = Color.black; // LLM 인식을 위해 검은 배경 추천

        // --- 캡처 로직 (기존과 동일) ---
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        targetCamera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);

        targetCamera.Render(); // 이 순간에만 퍼즐 레이어만 그려짐

        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        screenShot.Apply();

        // 3. 뒷정리 (카메라를 원래대로 돌려놓기)
        targetCamera.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);

        targetCamera.cullingMask = originalMask; // 원래 레이어 설정 복구
        targetCamera.clearFlags = originalFlags;
        targetCamera.backgroundColor = originalBG;

        // 4. 저장
        byte[] bytes = screenShot.EncodeToJPG(85);
        string directoryPath = Path.Combine(Application.dataPath, folderName);
        if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

        string fileName = "LLM_Input_Current.jpg";
        string fullPath = Path.Combine(directoryPath, fileName);
        File.WriteAllBytes(fullPath, bytes);

        DestroyImmediate(screenShot);
        Debug.Log($"[퍼즐 전용 캡처 완료] 경로: {fullPath}");
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            CaptureOnlyPuzzle();
        }
    }
}