using UnityEngine;

public class Flow_PlayMagicHatTalk02Anim : IFlow
{
    public void EndFlow()
    {

    }

    public void StartFlow()
    {
        MagicHatAnimator.Instance.PlayTalk02Animation();
    }
}
