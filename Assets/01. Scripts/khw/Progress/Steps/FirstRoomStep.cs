using System.Collections.Generic;
using UnityEngine;

/*
 - [트리거 작동]
    - *<문이 닫힌다>
    IFlow → Flow_CloseDoor*
    - 마녀 : 앞의 모자가 보여? 네가 하는 질문에 대답해줄 모자야. 모자가 내는 수수께끼를 풀어보도록 해.
    - 마녀 : 질문에는 예, 아니요로만 답변해줄테니까 질문을 잘 선택하는게 좋을거야.
        - *<대화 ai를 사용할 수 있게된다> IFlow → Flow_EnableConversationWithMagicHat*
        - “질문하려면, V를 누르면서 마이크로 질문하세요.”
        IFlow → Desc_MagicHatControl
            - “질문을 통해 모자의 수수께끼를 해결하세요. 답변은 예, 아니요로만 가능합니다.” IFlow → Desc_RequestPlayerQuestionToMagicHat
 */

public class FirstRoomStep : ProgressStepBase
{
    Progress_Door door;

    public override void OnEnterProgress()
    {
        door ??= GetComponentInChildren<Progress_Door>();

        VoiceManager.Instance.AddVoice(new VoiceLine(null, ESubtitleCharacters.witch, "앞의 모자가 보여? 네가 하는 질문에 대답해줄 모자야. 모자가 내는 수수께끼를 풀어보도록 해.", 0f, null));

        VoiceManager.Instance.AddVoice(new VoiceLine(null, ESubtitleCharacters.witch, "질문에는 예, 아니요로만 답변해줄테니까 질문을 잘 선택하는게 좋을거야.", 0f, new List<IFlow>()
        {
            new Flow_EnableConversationWithMagicHat(), //보이스를 재생하는 동시에 대화 AI를 사용할 수 있게 함.
            new Desc_MagicHatControl(), //보이스를 재생하는 동시에 마법 모자와 대화하는 방법을 설명하는 설명을 표시함.
            new Desc_RequestPlayerQuestionToMagic(), //보이스를 재생하는 동시에 플레이어가 마법 모자에게 질문하도록 요청하는 설명을 표시함.
        }));
    }

    public override void OnUpdateProgress() //나중에 비활성화
    {
        base.OnUpdateProgress();

        if (Input.GetKeyDown(KeyCode.F1))
            OnPuzzleSolved();
        if (Input.GetKeyDown(KeyCode.F2))
            OnPuzzleFailed();
    }

    public override void OnExitProgress()
    {
        new Flow_DoorClose(door).StartFlow();
    }

    public void OnPuzzleSolved() //퍼즐이 풀렸을 때 호출
    {
        VoiceManager.Instance.AddVoice(new VoiceLine(null, ESubtitleCharacters.magicHat, "맞았어. 문을 열어줄테니까 다음 테스트를 받도록 해.", 0f, new List<IFlow> {
            new Flow_DisableConverationWithMagicHat(), //보이스를 재생하는 동시에 대화 AI를 사용할 수 없게 함.
            new Flow_DoorOpen(door) //보이스를 재생하는 동시에 문을 여는 플로우를 실행함.
        }));
    }

    public void OnPuzzleFailed() //퍼즐이 실패했을 때 호출
    {
        VoiceManager.Instance.AddVoice(new VoiceLine(null, ESubtitleCharacters.magicHat, "다시 생각해봐, 그래선 마녀님의 조수가 될 수 없어.", 0f, null));
    }
}
