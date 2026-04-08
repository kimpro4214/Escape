using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Collections;

/// <summary>
/// 기존 시나리오/음성 로직과 완전 분리된 퍼즐 힌트 전용 스크립트
/// 화면을 캡처 → GPT Vision으로 분석 → TTS로 힌트 읽어줌
/// </summary>
public class ScreenHintService : MonoBehaviour
{
    [Header("OpenAI Vision 설정")]
    [SerializeField] private ApiKeyConfig apiKeyConfig; // 인스펙터에서 연결
    [SerializeField] private string model = "gpt-4o-mini";

    [Header("퍼즐 힌트 전용 프롬프트 (puzzle_Hint)")]
    [TextArea(4, 8)]
    [SerializeField]
    private string puzzle_Hint =
        "당신은 방탈출 게임의 퍼즐 힌트 도우미입니다. " +
        "지금 보이는 화면 속 퍼즐이나 단서를 분석하고, " +
        "플레이어가 막혀있을 것 같은 부분에 대해 " +
        "너무 직접적이지 않게 한 문장으로 힌트를 주세요.";

    [Header("힌트 전용 TTS (기존 SupertoneTTS와 별개 보이스)")]
    [SerializeField] private SupertoneTTS hintTTS; // 힌트 전용 SupertoneTTS 오브젝트 연결

    [Header("상태")]
    [SerializeField] private bool isProcessing = false;

    // ─────────────────────────────────────────────
    // 키보드 단축키 (H키)
    // ─────────────────────────────────────────────
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H) && !isProcessing)
        {
            StartCoroutine(CaptureAndHint());
        }
    }

    // ─────────────────────────────────────────────
    // UI 버튼 OnClick()에서 호출 가능
    // ─────────────────────────────────────────────
    public void OnHintButtonClicked()
    {
        if (!isProcessing)
            StartCoroutine(CaptureAndHint());
    }

    // ─────────────────────────────────────────────
    // 메인 흐름: 캡처 → Vision → TTS
    // ─────────────────────────────────────────────
    private IEnumerator CaptureAndHint()
    {
        isProcessing = true;
        Debug.Log("[ScreenHintService] 화면 캡처 중...");

        // 렌더링 완료 후 캡처 (필수)
        yield return new WaitForEndOfFrame();

        // 스크린샷 → Base64
        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();

        byte[] pngBytes = screenshot.EncodeToPNG();
        Destroy(screenshot);
        string base64Image = System.Convert.ToBase64String(pngBytes);

        Debug.Log("[ScreenHintService] GPT Vision 분석 중...");

        // GPT Vision 호출
        var visionTask = AskVision(base64Image);
        yield return new WaitUntil(() => visionTask.IsCompleted);

        string hintText = visionTask.Result;

        if (!string.IsNullOrEmpty(hintText))
        {
            Debug.Log($"[ScreenHintService] 힌트: {hintText}");

            // 힌트 전용 TTS로 읽기 (기존 시나리오 TTS와 완전 분리)
            var ttsTask = hintTTS.Speak(hintText);
            yield return new WaitUntil(() => ttsTask.IsCompleted);
        }
        else
        {
            Debug.LogWarning("[ScreenHintService] 힌트 텍스트가 비어있습니다.");
        }

        isProcessing = false;
    }

    // ─────────────────────────────────────────────
    // GPT Vision API 호출 (puzzle_Hint 프롬프트 사용)
    // ─────────────────────────────────────────────
    private async Task<string> AskVision(string base64Image)
    {
        // Vision API는 중첩 구조라 JsonUtility 대신 수동 JSON 작성
        string jsonBody = $@"{{
            ""model"": ""{model}"",
            ""messages"": [
                {{
                    ""role"": ""system"",
                    ""content"": ""{EscapeJson(puzzle_Hint)}""
                }},
                {{
                    ""role"": ""user"",
                    ""content"": [
                        {{
                            ""type"": ""image_url"",
                            ""image_url"": {{
                                ""url"": ""data:image/png;base64,{base64Image}""
                            }}
                        }},
                        {{
                            ""type"": ""text"",
                            ""text"": ""이 화면을 보고 퍼즐 힌트를 줘.""
                        }}
                    ]
                }}
            ],
            ""max_tokens"": 150
        }}";

        using (UnityWebRequest request = new UnityWebRequest(
            "https://api.openai.com/v1/chat/completions", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {apiKeyConfig.openAIKey}");

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[ScreenHintService] Vision API 오류: {request.error}\n{request.downloadHandler.text}");
                return null;
            }

            // 기존 GPTService의 public 클래스 재사용
            var response = JsonUtility.FromJson<GPTService.GPTResponse>(request.downloadHandler.text);
            return response.choices[0].message.content.Trim();
        }
    }

    // ─────────────────────────────────────────────
    // JSON 이스케이프 유틸
    // ─────────────────────────────────────────────
    private string EscapeJson(string s)
    {
        return s.Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "");
    }
}
