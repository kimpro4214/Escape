using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.IO;

public class WhisperSTT : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private ApiKeyConfig apiKeyConfig; // 인스펙터에서 연결
    [SerializeField] private int sampleRate = 16000;
    [SerializeField] private int maxRecordingSeconds = 30;

    private AudioClip recordingClip;
    private bool isRecording = false;

    /// <summary>
    /// 마이크 녹음 시작
    /// </summary>
    public void StartRecording()
    {
        Debug.Log("현재 API Key: " + apiKeyConfig.openAIKey);

        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("마이크가 연결되어 있지 않습니다!");
            return;
        }

        recordingClip = Microphone.Start(null, false, maxRecordingSeconds, sampleRate);
        isRecording = true;
    }

    /// <summary>
    /// 녹음 종료 후 WAV 데이터 반환
    /// </summary>
    public byte[] StopRecordingAndGetAudio()
    {
        if (!isRecording) return null;

        int position = Microphone.GetPosition(null);
        Microphone.End(null);
        isRecording = false;

        if (position == 0)
        {
            Debug.LogWarning("녹음된 데이터가 없습니다.");
            return null;
        }

        // AudioClip → float 배열
        float[] samples = new float[position * recordingClip.channels];
        recordingClip.GetData(samples, 0);

        // float 배열 → WAV byte 배열
        return ConvertToWav(samples, recordingClip.channels, sampleRate);
    }

    /// <summary>
    /// OpenAI Whisper API 호출
    /// </summary>
    public async Task<string> TranscribeAudio(byte[] audioData)
    {
        if (audioData == null || audioData.Length == 0) return null;

        // 멀티파트 폼 데이터 생성
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", audioData, "audio.wav", "audio/wav");
        form.AddField("model", "whisper-1");
        form.AddField("language", "ko"); // 한국어 설정

        using (UnityWebRequest request = UnityWebRequest.Post(
            "https://api.openai.com/v1/audio/transcriptions", form))
        {
            request.SetRequestHeader("Authorization", $"Bearer {apiKeyConfig.openAIKey}");

            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Whisper API 에러: {request.error}\n{request.downloadHandler.text}");
                return null;
            }

            // JSON 응답에서 텍스트 추출
            var response = JsonUtility.FromJson<WhisperResponse>(request.downloadHandler.text);
            return response.text;
        }
    }

    /// <summary>
    /// float 샘플 배열 → WAV 포맷 변환
    /// </summary>
    private byte[] ConvertToWav(float[] samples, int channels, int sampleRate)
    {
        using (var stream = new MemoryStream())
        using (var writer = new BinaryWriter(stream))
        {
            int bitsPerSample = 16;
            int byteRate = sampleRate * channels * bitsPerSample / 8;
            int blockAlign = channels * bitsPerSample / 8;
            int dataSize = samples.Length * 2; // 16bit = 2bytes

            // WAV 헤더
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + dataSize);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
            writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((short)1); // PCM
            writer.Write((short)channels);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write((short)blockAlign);
            writer.Write((short)bitsPerSample);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            writer.Write(dataSize);

            // 오디오 데이터
            foreach (float sample in samples)
            {
                short intSample = (short)(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue);
                writer.Write(intSample);
            }

            return stream.ToArray();
        }
    }

    [System.Serializable]
    private class WhisperResponse
    {
        public string text;
    }
}
