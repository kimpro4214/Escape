using UnityEngine;

public class Flow_DisableManequinnControl : IFlow
{
    public void EndFlow()
    {

    }

    public void StartFlow()
    {
        // 마네킹 조작이 불가능하도록 설정
        MannequinManager.Instance.MannequinSubmit();
    }
}
