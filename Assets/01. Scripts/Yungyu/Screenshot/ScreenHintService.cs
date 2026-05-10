using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Collections;

public class ScreenHintService : MonoBehaviour
{
    [Header("OpenAI Vision 설정")]
    [SerializeField] private ApiKeyConfig apiKeyConfig;
    [SerializeField] private string model = "gpt-5.4"; 

    [Header("퍼즐 힌트 전용 프롬프트")]
    [TextArea(4, 8)]
    [SerializeField]
    private string puzzle_Hint =
        "당신은 방탈출 게임의 퍼즐 힌트 도우미입니다. " +
        "지금 보이는 화면 속 퍼즐이나 단서를 분석하고, " +
        "플레이어가 막혀있을 것 같은 부분에 대해 " +
        "너무 직접적이지 않게 한 문장으로 힌트를 주세요.";

    [Header("TTS 설정")]
    [SerializeField] private TTSType ttsType = TTSType.Supertone;
    [SerializeField] private SupertoneTTS supertoneTTS;
    [SerializeField] private OpenAITTS openAITTS;

    public enum TTSType
    {
        Supertone,
        OpenAI
    }

    [Header("상태")]
    [SerializeField] private bool isProcessing = false;

    [Header("대화 기록 UI")]
    [SerializeField] private ChatLogManager chatLogManager;

    [Header("자막 UI")]
    [SerializeField] private SubtitleController subtitleController;

    private async Task Speak(string text)
    {
        switch (ttsType)
        {
            case TTSType.Supertone:
                if (supertoneTTS != null)
                    await supertoneTTS.Speak(text);
                else
                    Debug.LogError("[ScreenHintService] SupertoneTTS가 연결되지 않았습니다.");
                break;

            case TTSType.OpenAI:
                if (openAITTS != null)
                    await openAITTS.Speak(text);
                else
                    Debug.LogError("[ScreenHintService] OpenAITTS가 연결되지 않았습니다.");
                break;
        }
    }

    // 런타임 TTS 전환 (UI 버튼 등에서 호출 가능)
    public void SetTTSType(int index)
    {
        ttsType = (TTSType)index;
        Debug.Log($"[ScreenHintService] TTS 변경: {ttsType}");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H) && !isProcessing)
            StartCoroutine(CaptureAndHint());
    }

    public void OnHintButtonClicked()
    {
        if (!isProcessing)
            StartCoroutine(CaptureAndHint());
    }

    private IEnumerator CaptureAndHint()
    {
        isProcessing = true;
        Debug.Log("[ScreenHintService] 화면 캡처 중...");

        yield return new WaitForEndOfFrame();

        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();

        byte[] pngBytes = screenshot.EncodeToPNG();
        Destroy(screenshot);
        string base64Image = System.Convert.ToBase64String(pngBytes);

        Debug.Log("[ScreenHintService] GPT Vision 분석 중...");

        var visionTask = AskVision(base64Image);
        yield return new WaitUntil(() => visionTask.IsCompleted);

        string hintText = visionTask.Result;

        chatLogManager.AddLog("AI", hintText);
        subtitleController.ShowSubtitle("AI", hintText, 5f);

        if (!string.IsNullOrEmpty(hintText))
        {
            Debug.Log($"[ScreenHintService] 힌트: {hintText}");

            var ttsTask = Speak(hintText);
            yield return new WaitUntil(() => ttsTask.IsCompleted);
        }
        else
        {
            Debug.LogWarning("[ScreenHintService] 힌트 텍스트가 비어있습니다.");
        }

        isProcessing = false;
    }

    private async Task<string> AskVision(string base64Image)
    {
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
            ""max_completion_tokens"": 150
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

            var response = JsonUtility.FromJson<GPTService.GPTResponse>(request.downloadHandler.text);
            return response.choices[0].message.content.Trim();
        }
    }

    private string EscapeJson(string s)
    {
        return s.Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "");
    }
}