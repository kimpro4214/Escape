using UnityEngine;

public class Flow_DisableConverationWithMagicHat : IFlow
{
    public void EndFlow()
    {

    }

    public void StartFlow()
    {
        //마법모자와 대화할 수 있도록 한다.
        VoiceAIManager.Instance.DisableConversation();
    }
}
