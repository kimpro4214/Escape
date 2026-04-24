using UnityEngine;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;

public class VoiceAIManager : MonoBehaviour
{
    [Header("컴포넌트 연결")]
    [SerializeField] private WhisperSTT whisperSTT;
    [SerializeField] private GPTService gptService;
    [SerializeField] private SupertoneTTS supertoneTTS;

    [Header("추리 게임 시나리오 설정")]
    public List<Scenario> scenarios = new List<Scenario>();
    private int currentScenarioIdx = 0;
    private int currentHintIdx = 0;

    private bool isProcessing = false;
    private bool isRecording = false;

    [Header("대화 기록 UI")]
    [SerializeField] private ChatLogManager chatLogManager; // 인스펙터에서 연결!

    private void Update()
    {
        if (isProcessing) return;

        // T키 누르면 녹음 시작
        if (Input.GetKeyDown(KeyCode.T) && !isRecording)
        {
            StartVoiceRecording();
        }

        // T키 떼면 녹음 종료
        if (Input.GetKeyUp(KeyCode.T) && isRecording)
        {
            StopVoiceRecording();
        }

        // P키 누르면 현재 문제 다시 들려주기
        if (Input.GetKeyDown(KeyCode.P))
        {
            PlayCurrentProblemText();
        }
    }

    // --- UI 버튼에서 호출할 수 있도록 추가된 public 함수들 ---

    public void StartVoiceRecording()
    {
        if (isProcessing || isRecording) return;

        isRecording = true;
        whisperSTT.StartRecording();
        Debug.Log("🎤 추리 시작...");
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

        chatLogManager.AddLog("AI", scenarios[currentScenarioIdx].openingText);

        _ = supertoneTTS.Speak(scenarios[currentScenarioIdx].openingText);
    }

    // ---------------------------------------------------------

    private async void ProcessVoiceInput()
    {
        isProcessing = true;
        try
        {
            // 1. STT 변환
            byte[] audioData = whisperSTT.StopRecordingAndGetAudio();
            string playerText = await whisperSTT.TranscribeAudio(audioData);
            Debug.Log($"📝 플레이어: {playerText}");

            if (string.IsNullOrEmpty(playerText)) return;

            // 플레이어의 음성 인식 결과를 로그에 추가!
            chatLogManager.AddLog("플레이어", playerText);

            Scenario current = scenarios[currentScenarioIdx];

            // 2. 가로채기 로직: 정답 체크
            if (playerText.Contains("정답") || playerText.Contains("답은"))
            {
                if (playerText.Contains(current.correctAnswer))
                {
                    await supertoneTTS.Speak($"정답입니다! 진실을 알려드릴게요. {current.secretTruth}");
                    currentScenarioIdx = (currentScenarioIdx + 1) % scenarios.Count; // 다음 문제로
                    currentHintIdx = 0;
                    return;
                }
            }

            // 3. 가로채기 로직: 힌트 체크
            if (playerText.Contains("힌트"))
            {
                string hint = current.hints[currentHintIdx % current.hints.Length];
                currentHintIdx++;

                // 힌트 메시지 로그에 추가!
                chatLogManager.AddLog("힌트", hint);

                await supertoneTTS.Speak(hint);
                return;
            }

            // 4. 가로채기 로직에 안 걸리면 GPT에게 질문 (추리 단계)
            string gptResponse = await gptService.GetResponse(playerText, current.gptInstruction);

            // GPT의 대답을 로그에 추가!
            chatLogManager.AddLog("AI", gptResponse);

            await supertoneTTS.Speak(gptResponse);
        }
        catch (System.Exception e) { Debug.LogError($"Error: {e.Message}"); }
        finally { isProcessing = false; }
    }
}