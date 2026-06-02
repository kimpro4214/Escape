using UnityEngine;

public class Flow_PlayMagicHatTalk01Anim : IFlow
{
    public void EndFlow()
    {
        
    }

    public void StartFlow()
    {
        MagicHatAnimator.Instance.PlayTalk01Animation();
    }
}