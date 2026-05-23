using System.Collections.Generic;
using UnityEngine;

public class TTSManager : MonoBehaviour
{
    public static TTSManager Instance { get; private set; }

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    // 이미 로드한 오디오 캐싱
    private readonly Dictionary<string, AudioClip> clipCache
        = new();

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (audioSource == null)
        {
            audioSource =
                gameObject.AddComponent<AudioSource>();
        }
    }

    /// <summary>
    /// 예:
    /// PlayTTS(TTSSpeaker.Witch, 0)
    /// → line_000.wav
    /// </summary>
    public void PlayTTS(
        TTSSpeaker speaker,
        int lineIndex,
        bool interrupt = true
    )
    {
        AudioClip clip =
            GetClip(speaker, lineIndex);

        if (clip == null)
        {
            Debug.LogError(
                $"TTS 없음: {speaker} line_{lineIndex:000}"
            );
            return;
        }

        // 현재 재생 중이면 무시
        if (!interrupt && audioSource.isPlaying)
            return;

        audioSource.clip = clip;
        audioSource.Play();
    }

    public AudioClip GetClip(
        TTSSpeaker speaker,
        int lineIndex
    )
    {
        string path =
            GetResourcePath(speaker, lineIndex);

        // 캐시 확인
        if (clipCache.TryGetValue(path, out AudioClip cached))
        {
            return cached;
        }

        AudioClip clip =
            Resources.Load<AudioClip>(path);

        if (clip == null)
        {
            Debug.LogError(
                $"Resources Load 실패: {path}"
            );
            return null;
        }

        // 캐싱
        clipCache[path] = clip;

        return clip;
    }

    private string GetResourcePath(
        TTSSpeaker speaker,
        int lineIndex
    )
    {
        string folder = speaker switch
        {
            TTSSpeaker.Witch => "WitchTTS",
            TTSSpeaker.Magichat => "MagichatTTS",
            _ => ""
        };

        return
            $"TTS/{folder}/line_{lineIndex:000}";
    }

    public void StopTTS()
    {
        audioSource.Stop();
    }

    public bool IsPlaying()
    {
        return audioSource.isPlaying;
    }
}