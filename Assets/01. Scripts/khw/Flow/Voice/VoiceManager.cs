using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class VoiceLine
{
    public VoiceLine(AudioClip clip = null, ESubtitleCharacters charType = ESubtitleCharacters.None, string texts = "", float postDelay = 0f, List<IFlow> flows = null, int ttsLineIndex = -1)
    {
        this.clip = clip;
        this.characterName = charType;
        this.subtitle = texts;
        this.postDelay = postDelay;
        this.flows = flows;
        this.ttsLineIndex = ttsLineIndex;
    }

    public AudioClip clip;
    public ESubtitleCharacters characterName;

    public string subtitle;
    public float postDelay;

    public List<IFlow> flows;  //일괄 실시됨.

    // TTS wav 순서 index
    public int ttsLineIndex = -1;
}

public class VoiceManager : MonoBehaviour
{
    public static VoiceManager Instance { get; private set; }

    private Queue<VoiceLine> voiceLineQueue = new Queue<VoiceLine>();

    private bool isPlaying = false;
    private SubtitleController subtitle;

    private AudioSource currentLeadingSource; // 현재 재생 중인 오디오를 추적 (강제 중단용)

    // TTS
    [SerializeField] private OpenAITTS openAITTS;
    [SerializeField] private SupertoneTTS supertoneTTS;


    private void Awake()
    {
        if (subtitle == null) subtitle = GetComponentInChildren<SubtitleController>();

        if (Instance == null) Instance = this;
    }

    public void AddVoice(VoiceLine voiceLine)
    {
        voiceLineQueue.Enqueue(voiceLine);
        
        if (!isPlaying)
        {
            StartCoroutine(PlayVoiceQueue());
        }
    }

    public void AddVoice(List<VoiceLine> voiceLines)
    {
        foreach (var line in voiceLines)
        {
            voiceLineQueue.Enqueue(line);
        }
        
        if (!isPlaying)
        {
            StartCoroutine(PlayVoiceQueue());
        }
    }

    public void ForceStartVoice(List<VoiceLine> voiceLines)
    {
        if (currentLeadingSource != null && currentLeadingSource.isPlaying)
        {
            currentLeadingSource.Stop(); // 현재 말하고 있는 사람 소리 끄기
        }
        StopAllCoroutines();

        voiceLineQueue.Clear(); 
        isPlaying = false;

        AddVoice(voiceLines);
    }

    private IEnumerator PlayVoiceQueue()
    {
        isPlaying = true;

        while (voiceLineQueue.Count > 0)
        {
            VoiceLine currentLine = voiceLineQueue.Dequeue();
            Debug.Log($"재생 시도 - 캐릭터: {currentLine.characterName}, 텍스트: {currentLine.subtitle}");

            // 1. 캐릭터 참조 얻어오기
            CharacterBase character = CharacterFinder.FindCharacter(currentLine.characterName);
            Debug.Log($"캐릭터 찾기 결과: {(character == null ? "null" : character.name)}");

            if (character != null)
            {
                // 2. 해당 캐릭터의 AudioSource 참조
                currentLeadingSource = character.GetComponent<AudioSource>();

                // TTS 생성 로직 추가
                // clip null이면 캐릭터별 TTS로 생성
                if (currentLine.clip == null && !string.IsNullOrEmpty(currentLine.subtitle))
                {
                    if (currentLine.ttsLineIndex >= 0)
                    {
                        Debug.Log(
                            $"사전 녹음 TTS 로드: " +
                            $"{currentLine.characterName} " +
                            $"line_{currentLine.ttsLineIndex:000}"
                        );

                        currentLine.clip =
                            GetPreRecordedClip(
                                currentLine.characterName,
                                currentLine.ttsLineIndex
                            );
                    }
                    else
                    {
                        Debug.Log("TTS 호출 시작");
                        var task = GetTTSClip(currentLine.characterName, currentLine.subtitle);
                        yield return new WaitUntil(() => task.IsCompleted);
                        currentLine.clip = task.Result;
                        Debug.Log($"TTS 결과 clip: {(currentLine.clip == null ? "null" : "생성됨")}");
                    }

                   
                }

                if (currentLeadingSource != null)
                {
                    currentLeadingSource.clip = currentLine.clip;
                    currentLeadingSource.Play();
                }

                // 3. 자막 출력 (CharacterBase에 정의된 실제 이름을 사용하거나 함)
                // 잠시 비활성화
                // --------------------------------------| 꼭 다시 활성화 하기 |--------------------------
                // 오디오가 끝나고 postDelay만큼 더 보여주고 싶을 때
                float displayTime = currentLine.clip.length + currentLine.postDelay;
                subtitle.ShowSubtitle(character.characterName, currentLine.subtitle, displayTime);


                // 4. 플로우 실행
                if (currentLine.flows != null)
                {
                    foreach (var flow in currentLine.flows)
                    {
                        flow.StartFlow();
                    }
                }
            }

            // 4. 대기 로직 (클립이 없을 경우를 대비한 0f 처리)
            float clipLength = currentLine.clip != null ? currentLine.clip.length : 0f;
            yield return new WaitForSeconds(clipLength + currentLine.postDelay);
        }

        currentLeadingSource = null;
        isPlaying = false;
        Debug.Log("잘 꺼짐");
    }

    private async Task<AudioClip> GetTTSClip(ESubtitleCharacters charType, string text)
    {
        string voice = charType switch
        {
            ESubtitleCharacters.witch => "18139042935bc2849cb6ca",
            ESubtitleCharacters.magicHat => "709bebd6baa7cc0d9610c3",
            _ => null
        };
        return await supertoneTTS.GetClip(text, voice);
    }

    private readonly Dictionary<string, AudioClip>
    clipCache = new();

    private AudioClip GetPreRecordedClip(
        ESubtitleCharacters charType,
        int lineIndex
    )
    {
        string folder = charType switch
        {
            ESubtitleCharacters.witch
                => "WitchTTS",

            ESubtitleCharacters.magicHat
                => "MagichatTTS",

            _ => null
        };

        if (string.IsNullOrEmpty(folder))
            return null;

        string path =
            $"TTS/{folder}/line_{lineIndex:000}";

        // 캐시 확인
        if (clipCache.TryGetValue(
            path,
            out AudioClip cached))
        {
            return cached;
        }

        AudioClip clip =
            Resources.Load<AudioClip>(path);

        if (clip == null)
        {
            Debug.LogError(
                $"TTS wav 파일 없음: {path}"
            );

            return null;
        }

        clipCache[path] = clip;

        return clip;
    }
}

