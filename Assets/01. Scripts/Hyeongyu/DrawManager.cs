using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class DrawManager : MonoBehaviour
{
    public static DrawManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [Header("의존성 (인스펙터에서 연결)")]
    [SerializeField] private VisionJudgeService visionJudgeService;
    [SerializeField] private ImageCapturer imageCapturer;

    public event Action OnDrawSuccess;
    public event Action<string> OnDrawFail;

    private bool _isProcessing;

    public void DrawSubmit()
    {
        if (_isProcessing)
        {
            Debug.Log("[DrawManager] 이미 판정 중입니다.");
            return;
        }
        StartCoroutine(SubmitCoroutine());
    }

    private IEnumerator SubmitCoroutine()
    {
        _isProcessing = true;
        Debug.Log("[DrawManager] 마법진 판정 시작");

        yield return new WaitForEndOfFrame();

        // ImageCapturer로 파일 저장
        imageCapturer.CaptureOnlyPuzzle();

        // 저장된 파일 읽어서 base64 변환
        string filePath = Path.Combine(Application.dataPath, "04. Data/Captures/LLM_Input_Current.jpg");
        if (!File.Exists(filePath))
        {
            Debug.LogError("[DrawManager] 캡처 파일을 찾을 수 없습니다.");
            _isProcessing = false;
            yield break;
        }

        byte[] imageBytes = File.ReadAllBytes(filePath);
        string base64 = Convert.ToBase64String(imageBytes);

        var judgeTask = visionJudgeService.Judge(base64);
        yield return new WaitUntil(() => judgeTask.IsCompleted);

        if (judgeTask.IsFaulted)
        {
            Debug.LogError($"[DrawManager] 판정 예외: {judgeTask.Exception}");
            OnDrawFail?.Invoke("판정 중 오류가 발생했습니다.");
            _isProcessing = false;
            yield break;
        }

        VisionJudgeResult result = judgeTask.Result;
        if (result.isPass)
        {
            Debug.Log("[DrawManager] 판정 성공");
            OnDrawSuccess?.Invoke();
        }
        else
        {
            Debug.Log($"[DrawManager] 판정 실패. 힌트: {result.hint}");
            OnDrawFail?.Invoke(result.hint);
        }

        _isProcessing = false;
    }
}
