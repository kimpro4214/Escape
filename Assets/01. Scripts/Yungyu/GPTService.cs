using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class GPTService : MonoBehaviour
{
    [Header("API 키 설정")]
    [SerializeField] private ApiKeyConfig apiKeyConfig; // 인스펙터에서 연결
    [SerializeField] private string model = "gpt-4o-mini";

    // 이제 GetResponse에서 지침(Instruction)을 매개변수로 받습니다.
    public async Task<string> GetResponse(string userMessage, string instruction)
    {
        var requestBody = new GPTRequest
        {
            model = model,
            messages = new GPTMessage[]
            {
                new GPTMessage { role = "system", content = instruction },
                new GPTMessage { role = "user", content = userMessage }
            },
            max_tokens = 150,
            temperature = 0.5f // 추리 게임이므로 일관성을 위해 온도를 낮춤
        };

        string jsonBody = JsonUtility.ToJson(requestBody);

        using (UnityWebRequest request = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {apiKeyConfig.openAIKey}");

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success) return "통신 오류가 발생했어.";

            var response = JsonUtility.FromJson<GPTResponse>(request.downloadHandler.text);
            return response.choices[0].message.content.Trim();
        }
    }

    [System.Serializable] public class GPTRequest { public string model; public GPTMessage[] messages; public int max_tokens; public float temperature; }
    [System.Serializable] public class GPTMessage { public string role; public string content; }
    [System.Serializable] public class GPTResponse { public GPTChoice[] choices; }
    [System.Serializable] public class GPTChoice { public GPTMessage message; }
}