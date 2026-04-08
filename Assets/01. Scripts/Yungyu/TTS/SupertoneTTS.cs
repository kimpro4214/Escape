using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System;

public class SupertoneTTS : MonoBehaviour
{
    [Header("Supertone 설정")]
    [SerializeField] private ApiKeyConfig apiKeyConfig;
    [SerializeField] private string voiceId = "18139042935bc2849cb6ca"; // 슈퍼톤에서 제공하는 보이스 ID


    [Header("상세 설정")]
    [SerializeField] private string language = "ko"; // 언어 (ko, en, ja)
    [SerializeField] private string model = "sona_speech_2_flash"; // 최신 빠른 모델
    [SerializeField] private string style = "neutral"; // 감정/말투

    [Header("컴포넌트")]
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public async Task Speak(string text)
    {
        byte[] audioData = await CallSupertoneAPI(text);

        if (audioData == null || audioData.Length == 0) return;

        AudioClip clip = await LoadAudioClip(audioData);

        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
            while (audioSource.isPlaying) await Task.Yield();
        }
    }

    private async Task<byte[]> CallSupertoneAPI(string text)
    {
        // 1. 슈퍼톤 공식 문서에 맞춘 Request Body 구조
        var requestBody = new SupertoneRequest
        {
            text = text,
            language = this.language,
            model = this.model,
            style = this.style
        };

        string jsonBody = JsonUtility.ToJson(requestBody);
        string url = $"https://supertoneapi.com/v1/text-to-speech/{voiceId}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // 2. 범인이었던 헤더 이름 수정 (x-sup-api-key)
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("x-sup-api-key", apiKeyConfig.supertoneKey);

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Supertone API Error: {request.error}\nServer Message: {request.downloadHandler?.text}");
                return null;
            }

            return request.downloadHandler.data;
        }
    }

    private async Task<AudioClip> LoadAudioClip(byte[] audioData)
    {
        string tempPath = System.IO.Path.Combine(Application.temporaryCachePath, "supertone_audio.wav");
        System.IO.File.WriteAllBytes(tempPath, audioData);

        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip("file://" + tempPath, AudioType.WAV))
        {
            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            return DownloadHandlerAudioClip.GetContent(request);
        }
    }

    [Serializable]
    private class SupertoneRequest
    {
        public string text;
        public string language;
        public string model;
        public string style;
    }
}