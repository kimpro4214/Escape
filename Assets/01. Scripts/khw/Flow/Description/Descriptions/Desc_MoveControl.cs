using UnityEngine;

public class Desc_MoveControl : IFlow
{
    public void EndFlow()
    {
        //
    }

    public void StartFlow()
    {
        DescriptionManager.Instance.AddDescription(new GameDescription("W,S,A,D 로 움직이세요.", 10f));
    }
}
