using System.Collections.Generic;
using UnityEngine;

public class SecondRoomStep : ProgressStepBase
{
    //정보를 찾을 때 카운트 하나씩 증가.
    int countOfFoundInfo = 0;

    //문.
    Progress_Door door;

    public override void OnEnterProgress()
    {
        door ??= GetComponentInChildren<Progress_Door>();

        VoiceManager.Instance.AddVoice(new VoiceLine(null, ESubtitleCharacters.witch, "대화 능력은 확인했어. 그렇다면 그림도 그릴수 있을까? 나중에 마법진을 그려줄 조수가 필요하거든.", 0f, null, 4));

        VoiceManager.Instance.AddVoice(new VoiceLine(null, ESubtitleCharacters.witch, "마법진을 그리려면 정보가 필요하겠지? 방 안에서 정보를 찾아서 거기에 맞는 마법진을 그려줘. 잘 그린다면 마법이 발동될거야. ", 0f, new List<IFlow>()
        {
            new Desc_DrawMagicCircle()
        },
        5));
    }

    public override void OnUpdateProgress()
    {
        base.OnUpdateProgress();

        if(Input.GetKeyDown(KeyCode.F1)) 
        {
            OnInfoFound();
        }
        if (Input.GetKeyDown(KeyCode.F2))
        { 
            OnSubmitDrawing();
        }
        if(Input.GetKeyDown(KeyCode.F3))
        {  
            OnPuzzleSolved();
        }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            OnPuzzleFailed();
        }
    }
    public override void OnExitProgress()
    {
        new Flow_DoorClose(door).StartFlow();
    }

    public void OnInfoFound() //정보 찾을때마다 호출
    {
        countOfFoundInfo++;
        Debug.Log($"[SecondRoomStep] OnInfoFound() 호출 횟수: {countOfFoundInfo}");

        switch(countOfFoundInfo)
        {
            case 1: OnFirstInfoFound(); break;
            case 2: OnSecondInfoFound(); break;
            case 3: OnThirdInfoFound(); break;
        }
    }

    private void OnFirstInfoFound()
    {
        VoiceManager.Instance.AddVoice(new VoiceLine(null, ESubtitleCharacters.witch, "알아낸게 있어? 참고가 된다면 좋겠네.", 0f, null,6));
    }

    private void OnSecondInfoFound()
    {
        VoiceManager.Instance.AddVoice(new VoiceLine(null, ESubtitleCharacters.witch, "하나 더 찾아냈네, 완성한다면 어떤 모양일지 생각해봐.", 0f, null, 7));
    }

    private void OnThirdInfoFound()
    {
        VoiceManager.Instance.AddVoice(new VoiceLine(null, ESubtitleCharacters.witch, "다 찾은 것 같네, 마법진을 그릴 시간이야.", 0f, null, 8));
    }

    public void OnSubmitDrawing() //그림 제출 시 잠시 그림 그리기 기능을 막기
    {
        VoiceManager.Instance.AddVoice(new VoiceLine(null, ESubtitleCharacters.witch, "흐음.. 잠시 그림을 좀 평가해보겠어.", 0f, new List<IFlow>()
        {
            new Flow_DisableAccessToDrawMagicCircle()
        },
        9));
    }

    public void OnPuzzleSolved()
    {
        VoiceManager.Instance.AddVoice(new VoiceLine(null, ESubtitleCharacters.witch, "이정도면 마법이 잘 발동되겠는걸, 대단하잖아? 이제 다음 방에서 마지막 테스트를 받도록 해.", 0f, new List<IFlow>()
        {
            new Flow_BlowUpMagicCirclePalette(), //보이스를 재생하는 동시에 팔레트 폭파.
            new Flow_DoorOpen(door) //보이스를 재생하는 동시에 문을 여는 플로우를 실행함.
        },
        10));
    }

    public void OnPuzzleFailed()
    {
        VoiceManager.Instance.AddVoice(new VoiceLine(null, ESubtitleCharacters.witch, "이건… 아쉬운데, 정보를 잘 조합했는지 다시 확인해봐.", 0f, new List<IFlow>
        {
            new Flow_EnableAccessToDrawMagicCircle() //다시 활성화
        },
        11));
    }
}
