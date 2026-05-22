using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Collections;
using System.IO;

public class ScreenHintService : MonoBehaviour
{
    public static ScreenHintService Instance { get; private set; }

    [Header("OpenAI Vision 설정")]
    [SerializeField] private ApiKeyConfig apiKeyConfig;
    [SerializeField] private string model = "gpt-5.4";

    [Header("현재 퍼즐 인덱스")]
    [Tooltip("0 = Draw, 1 = Mannequin")]
    public int curIndex = 0;

    [Header("퍼즐 힌트 전용 프롬프트")]
    [TextArea(4, 8)]
    [SerializeField]
    private string[] puzzle_Hint = { };

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
    [SerializeField] public bool isProcessing = false;

    [Header("대화 기록 UI")]
    [SerializeField] private ChatLogManager chatLogManager;

    private bool isScreenShotEnabled = false; // 대화 활성화 여부

    private void Awake()
    {
        // 싱글톤 초기화
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void EnableScreenShot()
    {
        isScreenShotEnabled = true;
        Debug.Log("[ScreenHintService] 스크린샷이 활성화되었습니다.");
    }

    public void DisableScreenShot()
    {
        isScreenShotEnabled = false;
        Debug.Log("[ScreenHintService] 스크린샷이 비활성화되었습니다.");
    }

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
        if (!isScreenShotEnabled) return;

        if (Input.GetKeyDown(KeyCode.H) && !isProcessing)
            StartCoroutine(CaptureAndHint());
    }

    public void OnHintButtonClicked()
    {
        if (!isProcessing)
            StartCoroutine(CaptureAndHint());
    }

    public IEnumerator CaptureAndHint()
    {
        isProcessing = true;
        Debug.Log("[ScreenHintService] 화면 캡처 중...");

        yield return new WaitForEndOfFrame();


        // 훈이 수정한 부분1 시작 =======================================
        // 현재 작동중인 퍼즐에 따른 사진 경로 저장
        string filePath = $"Assets/04. Data/Captures/Capture_{curIndex}.png";
        string base64Image = "";

        // 파일이 실제로 존재하는지 체크 후 변환 시도
        if (File.Exists(filePath))
        {
            byte[] pngBytes = File.ReadAllBytes(filePath);
            base64Image = System.Convert.ToBase64String(pngBytes);
            Debug.Log($"[Mannequin] {curIndex}번 사진 Base64 변환 성공");
        }
        // 훈이 수정한 부분1 끝 =======================================


        Debug.Log("[ScreenHintService] GPT Vision 분석 중...");

        var visionTask = AskVision(base64Image);

        yield return new WaitUntil(() => visionTask.IsCompleted);

        string hintText = visionTask.Result;


        // 훈이 수정한 부분2 시작 =======================================
        // curIndex에 맞는 각 함수 호출. 답 확인에 대한 로직은 각 매니저가 하도록 함.
        switch (curIndex)
        {
            case 0: // Draw일 때
                DrawManager.Instance.CheckAnswer(hintText);
                break;
            case 1: // Mannequin일 때
                MannequinManager.Instance.CheckAnswer(hintText);
                break;
        }
        // 훈이 수정한 부분2 끝 =======================================

        //chatLogManager.AddLog("AI", hintText);

        if (!string.IsNullOrEmpty(hintText))
        {
            Debug.Log($"[ScreenHintService] 힌트: {hintText}");

            //var ttsTask = Speak(hintText);
            //yield return new WaitUntil(() => ttsTask.IsCompleted);
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
                    ""content"": ""{EscapeJson(puzzle_Hint[curIndex])}""
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