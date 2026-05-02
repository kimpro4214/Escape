using UnityEngine;

public class FirstStep : ProgressStepBase
{
    public override void OnEnterProgress()
    {
         FlowManager.Instance.StartFlow(new Room1_EnableMove());
    }
}
