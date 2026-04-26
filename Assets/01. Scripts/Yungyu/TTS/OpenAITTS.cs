using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class OpenAITTS : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private ApiKeyConfig apiKeyConfig; // 인스펙터에서 연결
    [SerializeField] private string model = "tts-1";
    [SerializeField] private string voice = "nova";
    [SerializeField] private float speed = 1.0f;

    [Header("컴포넌트")]
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    /// <summary>
    /// 텍스트를 음성으로 변환 후 재생
    /// </summary>
    public async Task Speak(string text)
    {
        byte[] audioData = await CallTTSAPI(text);

        if (audioData == null || audioData.Length == 0)
        {
            Debug.LogError("TTS 음성 데이터를 받지 못했습니다.");
            return;
        }

        // MP3 → AudioClip 변환 후 재생
        AudioClip clip = await LoadAudioClip(audioData);

        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();

            // 재생 완료까지 대기
            while (audioSource.isPlaying)
                await Task.Yield();
        }
    }

    /// <summary>
    /// OpenAI TTS API 호출
    /// </summary>
    private async Task<byte[]> CallTTSAPI(string text)
    {
        var requestBody = new TTSRequest
        {
            model = model,
            input = text,
            voice = voice,
            speed = speed
        };

        string jsonBody = JsonUtility.ToJson(requestBody);

        using (UnityWebRequest request = new UnityWebRequest(
            "https://api.openai.com/v1/audio/speech", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {apiKeyConfig.openAIKey}");

            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"TTS API 에러: {request.error}\n{request.downloadHandler.text}");
                return null;
            }

            return request.downloadHandler.data;
        }
    }

    /// <summary>
    /// MP3 바이트 데이터 → AudioClip (WAV 파일로 임시 저장 후 로드)
    /// </summary>
    private async Task<AudioClip> LoadAudioClip(byte[] audioData)
    {
        // 임시 파일로 저장 (Unity는 MP3 직접 로드가 제한적)
        string tempPath = System.IO.Path.Combine(Application.temporaryCachePath, "tts_response.mp3");
        System.IO.File.WriteAllBytes(tempPath, audioData);

        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(
            "file://" + tempPath, AudioType.MPEG))
        {
            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"오디오 로드 에러: {request.error}");
                return null;
            }

            return DownloadHandlerAudioClip.GetContent(request);
        }
    }

    [System.Serializable]
    private class TTSRequest
    {
        public string model;
        public string input;
        public string voice;
        public float speed;
    }
}
