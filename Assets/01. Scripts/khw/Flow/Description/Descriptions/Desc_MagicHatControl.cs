using UnityEngine;

public class Desc_MagicHatControl : IFlow
{
    public void EndFlow()
    {
        //
    }

    public void StartFlow()
    {
        DescriptionManager.Instance.AddDescription(new GameDescription("질문하려면, V를 누르면서 마이크로 질문하세요.", 4f));
    }
}