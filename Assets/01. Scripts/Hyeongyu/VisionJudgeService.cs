using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class VisionJudgeResult
{
    public bool isPass;
    public string hint;
}

public class VisionJudgeService : MonoBehaviour
{
    [Header("OpenAI Vision 설정")]
    [SerializeField] private ApiKeyConfig apiKeyConfig;
    [SerializeField] private string model = "gpt-4o-mini";

    [Header("마법진 판정 프롬프트")]
    [TextArea(5, 10)]
    [SerializeField]
    private string judgePrompt =
        "당신은 방탈출 게임의 마법진 판정관입니다. " +
        "플레이어가 그린 마법진 이미지를 보고 올바른 마법진인지 판단하세요. " +
        "올바르다면 'PASS'라고만 답하세요. " +
        "올바르지 않다면 'FAIL: ' 뒤에 무엇이 잘못되었는지 한 문장으로 힌트를 주세요. " +
        "예시: 'FAIL: 마법진의 중앙 원이 닫혀 있지 않습니다.' " +
        "반드시 'PASS' 또는 'FAIL: 힌트' 형식만 사용하세요.";

    public async Task<VisionJudgeResult> Judge(string base64Image)
    {
        string raw = await CallVisionAPI(base64Image);
        return ParseResponse(raw);
    }

    private async Task<string> CallVisionAPI(string base64Image)
    {
        string jsonBody = $@"{{
            ""model"": ""{model}"",
            ""messages"": [
                {{""role"": ""system"", ""content"": ""{EscapeJson(judgePrompt)}""}},
                {{
                    ""role"": ""user"",
                    ""content"": [
                        {{""type"": ""image_url"", ""image_url"": {{""url"": ""data:image/jpeg;base64,{base64Image}""}}}},
                        {{""type"": ""text"", ""text"": ""이 마법진이 올바르게 그려졌는지 판정해주세요.""}}
                    ]
                }}
            ],
            ""max_tokens"": 100
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
                Debug.LogError($"[VisionJudgeService] API 오류: {request.error}\n{request.downloadHandler.text}");
                return null;
            }

            var response = JsonUtility.FromJson<GPTService.GPTResponse>(request.downloadHandler.text);
            return response?.choices?[0]?.message?.content?.Trim();
        }
    }

    private VisionJudgeResult ParseResponse(string raw)
    {
        var result = new VisionJudgeResult();

        if (string.IsNullOrEmpty(raw))
        {
            result.isPass = false;
            result.hint = "판정 서버와 통신에 실패했습니다. 다시 시도해주세요.";
            return result;
        }

        if (raw.StartsWith("PASS"))
        {
            result.isPass = true;
            result.hint = string.Empty;
        }
        else if (raw.StartsWith("FAIL:"))
        {
            result.isPass = false;
            result.hint = raw.Substring("FAIL:".Length).Trim();
        }
        else
        {
            Debug.LogWarning($"[VisionJudgeService] 예상치 못한 응답 포맷: {raw}");
            result.isPass = false;
            result.hint = raw;
        }

        Debug.Log($"[VisionJudgeService] 판정: isPass={result.isPass}, hint={result.hint}");
        return result;
    }

    private string EscapeJson(string s) =>
        s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "");
}
