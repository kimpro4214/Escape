using System.Collections.Generic;
using UnityEngine;

public class ThirdRoomStep : ProgressStepBase
{
    public override void OnEnterProgress()
    {
        VoiceManager.Instance.AddVoice(new VoiceLine(null, ESubtitleCharacters.witch, "마지막 테스트야. 상황을 묘사할 수 있는지도 테스트하겠어.", 0f, null));

        VoiceManager.Instance.AddVoice(new VoiceLine(null, ESubtitleCharacters.witch, "앞의 마네킹이 보여? 마네킹의 관절을 조절해서 […]를 하도록 만들어줘. 내가 직접 평가해줄게.", 0f, new List<IFlow>
        {
            new Desc_ManequinnControl()
        }));
    }

    public override void OnUpdateProgress() //나중에 비활성화
    {
        base.OnUpdateProgress();

        if (Input.GetKeyDown(KeyCode.F1))
            SubmitAnswer();

        if (Input.GetKeyDown(KeyCode.F2))
            OnPuzzleSolved();

        if(Input.GetKeyDown(KeyCode.F3))
            OnPuzzleFailed();
    }

    public void SubmitAnswer() // 플레이어가 답안을 제출했을 때 호출되는 메서드
    {
        new Flow_DisableManequinnControl().StartFlow();
    }

    public void OnPuzzleSolved()
    {
        VoiceManager.Instance.AddVoice(new VoiceLine(null, ESubtitleCharacters.witch, "괜찮은데? 이정도라면 앞으로 여러가지 일을 맡길 수 있겠어. 더 좋은 실험체가 소환될 때까진 잘 부탁해?", 0f, new List<IFlow>
        { 
            new Flow_BlowUpManequinn(), //보이스를 재생하는 동시에 마네킹을 폭발시키는 플로우를 실행함.

            //이후 화면 암전.

        }));
    }

    public void OnPuzzleFailed()
    {
        VoiceManager.Instance.AddVoice(new VoiceLine(null, ESubtitleCharacters.witch, "음, 조금 부족한 것 같은데? 더 노력해보는게 좋을거야.", 0f, new List<IFlow>
        {
            new Flow_EnableManequinnControl(),
        }));
    }
}


