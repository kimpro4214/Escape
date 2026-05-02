using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VoiceLine
{
    public VoiceLine(AudioClip clip = null, ESubtitleCharacters charType = ESubtitleCharacters.None, string texts = "", float postDelay = 0f, List<IFlow> flows = null)
    {
        this.clip = clip;
        this.characterName = charType;
        this.subtitle = texts;
        this.postDelay = postDelay;
        this.flows = flows;
    }

    public AudioClip clip;
    public ESubtitleCharacters characterName;

    public string subtitle;
    public float postDelay;

    public List<IFlow> flows;  //일괄 실시됨.
}

public class VoiceManager : MonoBehaviour
{
    public static VoiceManager Instance { get; private set; }

    private Queue<VoiceLine> voiceLineQueue = new Queue<VoiceLine>();

    private bool isPlaying = false;
    private SubtitleController subtitle;

    private AudioSource currentLeadingSource; // 현재 재생 중인 오디오를 추적 (강제 중단용)

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

            // 1. 캐릭터 참조 얻어오기
            CharacterBase character = CharacterFinder.FindCharacter(currentLine.characterName);

            if (character != null)
            {
                // 2. 해당 캐릭터의 AudioSource 참조
                currentLeadingSource = character.GetComponent<AudioSource>();

                if (currentLeadingSource != null)
                {
                    currentLeadingSource.clip = currentLine.clip;
                    currentLeadingSource.Play();
                }

                // 3. 자막 출력 (CharacterBase에 정의된 실제 이름을 사용하거나 함)
                subtitle.ShowSubtitle(character.characterName, currentLine.subtitle, currentLine.postDelay);

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
    }
}
