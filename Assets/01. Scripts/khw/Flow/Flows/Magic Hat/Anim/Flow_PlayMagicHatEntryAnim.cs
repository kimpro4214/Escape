using UnityEngine;

public class Flow_PlayMagicHatEntryAnim : IFlow
{

    public void EndFlow()
    {

    }

    public void StartFlow()
    {
        MagicHatAnimator.Instance.PlayIdle();
    }
}
