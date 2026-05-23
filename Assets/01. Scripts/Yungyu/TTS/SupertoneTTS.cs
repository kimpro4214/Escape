using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System;

public class SupertoneTTS : MonoBehaviour
{
    public static SupertoneTTS Instance { get; private set; }

    [Header("Supertone 설정")]
    [SerializeField] private ApiKeyConfig apiKeyConfig;
    [SerializeField] private string voiceId = "709bebd6baa7cc0d9610c3"; // magichat

    [Header("음 설정")]
    [SerializeField] private string language = "ko"; // 언어 (ko, en, ja)
    [SerializeField] private string model = "sona_speech_2_flash"; // 최신 모델 명
    [SerializeField] private string style = "neutral"; // 스타일/감정

    [Header("컴포넌트")]
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // 싱글톤 초기화
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public async Task Speak(string text)
    {
        byte[] audioData = await CallSupertoneAPI(text, voiceId);

        if (audioData == null || audioData.Length == 0) return;

        AudioClip clip = await LoadAudioClip(audioData);

        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
            while (audioSource.isPlaying) await Task.Yield();
        }
    }

    /// <summary>
    /// voiceId를 지정해서 AudioClip만 받아오기 (VoiceManager에서 사용)
    /// voiceId가 null이면 인스펙터 기본값 사용
    /// </summary>
    public async Task<AudioClip> GetClip(string text, string overrideVoiceId = null)
    {
        string useVoiceId = string.IsNullOrEmpty(overrideVoiceId) ? this.voiceId : overrideVoiceId;
        byte[] audioData = await CallSupertoneAPI(text, useVoiceId);
        if (audioData == null || audioData.Length == 0) return null;
        return await LoadAudioClip(audioData);
    }

    private async Task<byte[]> CallSupertoneAPI(string text, string targetVoiceId)
    {
        var requestBody = new SupertoneRequest
        {
            text = text,
            language = this.language,
            model = this.model,
            style = this.style
        };

        string jsonBody = JsonUtility.ToJson(requestBody);
        string url = $"https://supertoneapi.com/v1/text-to-speech/{targetVoiceId}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

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