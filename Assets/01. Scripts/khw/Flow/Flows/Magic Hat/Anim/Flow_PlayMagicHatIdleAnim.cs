using UnityEngine;

public class Flow_PlayMagicHatIdleAnim : IFlow
{

    public void EndFlow()
    {

    }

    public void StartFlow()
    {
        MagicHatAnimator.Instance.PlayIdle();
    }
}
