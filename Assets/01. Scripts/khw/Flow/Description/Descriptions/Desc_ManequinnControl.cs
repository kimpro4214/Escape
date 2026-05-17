using UnityEngine;

public class Desc_ManequinnControl : IFlow
{
    public void EndFlow()
    {
        //
    }
    public void StartFlow()
    {
        DescriptionManager.Instance.AddDescription(new GameDescription("오른쪽 인형의 관절을 조작해 방패로 방어 하도록 하세요.", 4f));
    }
}

