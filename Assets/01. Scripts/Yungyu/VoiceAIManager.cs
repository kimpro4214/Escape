using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class VoiceAIManager : MonoBehaviour
{
    public static VoiceAIManager Instance { get; private set; }

    [Header("컴포넌트 연결")]
    [SerializeField] private WhisperSTT whisperSTT;
    [SerializeField] private GPTService gptService;
    [SerializeField] private SupertoneTTS supertoneTTS;
    [SerializeField] private OpenAITTS openAITTS;

    [Header("TTS 설정")]
    [SerializeField] private TTSType ttsType = TTSType.Supertone;

    public enum TTSType
    {
        Supertone,
        OpenAI
    }

    [Header("추리 게임 시나리오 설정")]
    public List<Scenario> scenarios = new List<Scenario>();
    private int currentScenarioIdx = 0;
    private int currentHintIdx = 0;

    private bool isProcessing = false;
    private bool isRecording = false;
    private bool isConversationEnabled = false; // 대화 활성화 여부

    [Header("대화 기록 UI")]
    [SerializeField] private ChatLogManager chatLogManager;

    [Header("자막 UI")]
    [SerializeField] private SubtitleController subtitleController;

    // 싱글톤
    private void Awake()
    {
        // 싱글톤 초기화
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // "Flow"에서 대화를 활성화할 때 호출
    public void EnableConversation()
    {
        isConversationEnabled = true;
        Debug.Log("[VoiceAIManager] 마법모자와의 대화가 활성화되었습니다. V키를 눌러 말하세요.");
    }

    // "Flow"에서 대화를 비활성화할 때 호출
    public void DisableConversation()
    {
        isConversationEnabled = false;

        // 녹음 중이었다면 즉시 중단
        if (isRecording)
        {
            isRecording = false;
            whisperSTT.StopRecordingAndGetAudio(); // 버퍼 클리어
        }

        Debug.Log("[VoiceAIManager] 대화가 비활성화되었습니다.");
    }

    private async Task Speak(string text)
    {
        switch (ttsType)
        {
            case TTSType.Supertone:
                if (supertoneTTS != null)
                    await supertoneTTS.Speak(text);
                else
                    Debug.LogError("SupertoneTTS가 연결되지 않았습니다.");
                break;

            case TTSType.OpenAI:
                if (openAITTS != null)
                    await openAITTS.Speak(text);
                else
                    Debug.LogError("OpenAITTS가 연결되지 않았습니다.");
                break;
        }
    }

    private void Update()
    {
        if (!isConversationEnabled) return;
        if (isProcessing) return;

        if (Input.GetKeyDown(KeyCode.V) && !isRecording)
            StartVoiceRecording();

        if (Input.GetKeyUp(KeyCode.V) && isRecording)
            StopVoiceRecording();

        if (Input.GetKeyDown(KeyCode.P))
            PlayCurrentProblemText();
    }

    public void StartVoiceRecording()
    {
        if (isProcessing || isRecording) return;
        isRecording = true;
        whisperSTT.StartRecording();
        Debug.Log(" 녹음 시작...");
    }

    public void StopVoiceRecording()
    {
        if (!isRecording) return;
        isRecording = false;
        ProcessVoiceInput();
    }

    public void PlayCurrentProblemText()
    {
        if (isProcessing) return;
        //chatLogManager.AddLog("AI", scenarios[currentScenarioIdx].openingText);
        //subtitleController.ShowSubtitle("AI", scenarios[currentScenarioIdx].openingText, 5f);
        _ = Speak(scenarios[currentScenarioIdx].openingText);
    }

    // 런타임에서 TTS 전환용 (UI 버튼 등에서 호출 가능)
    public void SetTTSType(int index)
    {
        ttsType = (TTSType)index;
        Debug.Log($"TTS 변경: {ttsType}");
    }

    private async void ProcessVoiceInput()
    {
        isProcessing = true;
        try
        {
            byte[] audioData = whisperSTT.StopRecordingAndGetAudio();
            string playerText = await whisperSTT.TranscribeAudio(audioData);
            Debug.Log($"플레이어: {playerText}");

            if (string.IsNullOrEmpty(playerText)) return;

            //chatLogManager.AddLog("플레이어", playerText);
            //subtitleController.ShowSubtitle("플레이어", playerText, 5f);

            Scenario current = scenarios[currentScenarioIdx];

            if (playerText.Contains("정답") || playerText.Contains("답은")) //정답일경우 호출되는 파트
            {
                if (playerText.Contains(current.correctAnswer))
                {
                    await Speak($"정답입니다!"); //진실을 알려드릴게요. {current.secretTruth}
                    currentScenarioIdx = (currentScenarioIdx + 1) % scenarios.Count;
                    currentHintIdx = 0;

                    //first step 의 문 열기.
                    FindAnyObjectByType<FirstRoomStep>()?.OnPuzzleSolved();

                    return;
                }
            }

            if (playerText.Contains("힌트"))
            {
                string hint = current.hints[currentHintIdx % current.hints.Length];
                currentHintIdx++;
                //chatLogManager.AddLog("힌트", hint);
                //subtitleController.ShowSubtitle("힌트", hint, 5f);
                await Speak(hint);
                return;
            }

            string gptResponse = await gptService.GetResponse(playerText, current.gptInstruction);
            //chatLogManager.AddLog("AI", gptResponse);
            //subtitleController.ShowSubtitle("AI", gptResponse, 5f);
            await Speak(gptResponse);
        }
        catch (System.Exception e) { Debug.LogError($"Error: {e.Message}"); }
        finally { isProcessing = false; }
    }
}