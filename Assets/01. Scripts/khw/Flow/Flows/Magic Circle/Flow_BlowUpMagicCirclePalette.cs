using UnityEngine;

public class Flow_BlowUpMagicCirclePalette : IFlow
{
    public void EndFlow()
    {

    }

    public void StartFlow()
    {
        // 마법진 팔레트가 폭발하는 연출을 시작
        DrawManager.Instance.DrawDestroy();
    }
}
