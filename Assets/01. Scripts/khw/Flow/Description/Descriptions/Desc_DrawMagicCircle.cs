using UnityEngine;

public class Desc_DrawMagicCircle : IFlow
{
    public void EndFlow()
    {
        //
    }

    public void StartFlow()
    {
        DescriptionManager.Instance.AddDescription(new GameDescription("올바른 마법진을 그리세요. 마법진의 정보는 방 곳곳에 있습니다.", 10f));
    }
}
