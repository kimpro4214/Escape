using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VoiceLine
{
    public VoiceLine(AudioClip clip, string charName, string texts, float postDelay)
    {
        this.clip = clip;
        this.characterName = charName;
        this.subtitle = texts;
        this.postDelay = postDelay;
    }

    public AudioClip clip;
    public string characterName;
    public string subtitle;
    public float postDelay;
}

public class VoiceManager : MonoBehaviour
{
    private Queue<VoiceLine> voiceLineQueue = new Queue<VoiceLine>();
    private AudioSource audioSource;
    private bool isPlaying = false;
    private SubtitleController subtitle;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (subtitle == null) subtitle = GetComponentInChildren<SubtitleController>();
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
        StopAllCoroutines(); 
        audioSource.Stop();  
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

            audioSource.clip = currentLine.clip;
            audioSource.Play();

            subtitle.ShowSubtitle(currentLine.characterName, currentLine.subtitle, currentLine.postDelay);

            yield return new WaitForSeconds(currentLine.clip.length + currentLine.postDelay);
        }

        isPlaying = false;
    }
}
