using System.Collections.Generic;
using UnityEngine;

/*
- 마녀 : 어서와, 너는 실험체로써 몇가지 테스트를 받아줘야겠어. 능력을 보여준다면 내 조수가 될거야. (1s)
- 마녀 : 몸은 움직일 수 있지? 앞의 방으로 들어가서 첫 번째 테스트를 받도록 해. (1s)
    - <움직일 수 있게 된다.>
    - <문이 열린다>
    - “w,s,a,d 로 움직이세요.” (10s, 1s)
*/

// 게임 시작 시 자동 실행됩니다!
public class GameStartStep : ProgressStepBase
{
    Progress_Door door;

    public override void OnEnterProgress()
    {
        //대사에 트리거를 넣는 예시.
        door ??= GetComponentInChildren<Progress_Door>();

        VoiceManager.Instance.AddVoice(new VoiceLine(null, ESubtitleCharacters.witch, "빨리 움직이기.", 1f, new List<IFlow>()
        {
            new Desc_MoveControl(), //보이스를 재생하는 동시에 이동 조작 표시 설명을 표시함.
            new Flow_DoorOpen(door) //보이스를 재생하는 동시에 문을 여는 플로우를 실행함.
        }));
    }

    public override void OnExitProgress()
    {

    }

    public override void OnUpdateProgress()
    {

    }
}
