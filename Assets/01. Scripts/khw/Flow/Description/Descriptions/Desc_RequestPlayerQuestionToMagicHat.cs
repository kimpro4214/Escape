using UnityEngine;

public class Desc_RequestPlayerQuestionToMagic : IFlow
{
    public void EndFlow()
    {
        //
    }

    public void StartFlow()
    {
        DescriptionManager.Instance.AddDescription(new GameDescription("질문을 통해 모자의 수수께끼를 해결하세요. 답변은 예, 아니요로만 가능합니다.", 10f));
    }
}
