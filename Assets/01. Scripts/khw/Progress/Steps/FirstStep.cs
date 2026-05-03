using System.Collections.Generic;
using UnityEngine;

public class FirstStep : ProgressStepBase
{
    Progress_Door entryDoor;
    Progress_Door exitDoor;

    public override void OnEnterProgress()
    {
        // 1. 문 닫기
        entryDoor ??= GetComponentInChildren<Progress_Door>();

        FlowManager.Instance.StartFlow(new Flow_DoorClose(entryDoor));

        // 2. 마녀 대사 1
        VoiceManager.Instance.AddVoice(new VoiceLine(
            null,
            ESubtitleCharacters.witch,
            "앞의 모자가 보여? 네가 하는 질문에 대답해줄 모자야. 모자가 내는 수수께끼를 풀어보도록 해.",
            5f
        ));

        // 3. 마녀 대사 2 + AI 활성화 flows
        VoiceManager.Instance.AddVoice(new VoiceLine(
            null,
            ESubtitleCharacters.witch,
            "질문에는 예, 아니요로만 답변해줄테니까 질문을 잘 선택하는게 좋을거야.",
            5f,
            new List<IFlow>()
            {
                new Flow_EnableConversationWithMagicHat(),
                new Desc_MagicHatControl(),
                new Desc_RequestPlayerQuestionToMagic()
            }
        ));
    }

    private void OnSuccess()
    {
        FlowManager.Instance.StartFlow(new Flow_DisableConverationWithMagicHat());

        VoiceManager.Instance.AddVoice(new VoiceLine(
            null,
            ESubtitleCharacters.magicHat,
            "맞았어. 문을 열어줄테니까 다음 테스트를 받도록 해.",
            5f,
            new List<IFlow>()
            {
                new Flow_DoorOpen(exitDoor)
            }
        ));
    }

    private void OnFail()
    {
        VoiceManager.Instance.ForceStartVoice(new List<VoiceLine>()
        {
            new VoiceLine(
                null,
                ESubtitleCharacters.magicHat,
                "다시 생각해봐, 그래선 마녀님의 조수가 될 수 없어.",
                1f
            )
        });
    }

    public override void OnExitProgress() { }
    public override void OnUpdateProgress() { }
}