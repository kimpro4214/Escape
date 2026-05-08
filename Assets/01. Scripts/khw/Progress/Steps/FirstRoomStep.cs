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
    public override void OnEnterProgress()
    {
        
    }
}
