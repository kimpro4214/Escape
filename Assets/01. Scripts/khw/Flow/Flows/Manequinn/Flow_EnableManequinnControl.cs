using UnityEngine;

public class Flow_EnableManequinnControl : IFlow
{
    public void EndFlow()
    {
    }
    public void StartFlow()
    {
        // 마네킹 조작이 가능하도록 설정
        MannequinManager.Instance.MannequinEnable();
    }
}
