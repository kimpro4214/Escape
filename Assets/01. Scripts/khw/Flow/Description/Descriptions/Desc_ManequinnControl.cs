using UnityEngine;

public class Desc_ManequinnControl : IFlow
{
    public void EndFlow()
    {
        //
    }
    public void StartFlow()
    {
        DescriptionManager.Instance.AddDescription(new GameDescription("마네킹의 관절을 조작해 […] 하도록 만드세요.", 4f));
    }
}

