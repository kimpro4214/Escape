using UnityEngine;

public class GameStartStep : ProgressStepBase
{
    Progress_Door door;

    public override void OnEnterProgress()
    {
        door ??= GetComponentInChildren<Progress_Door>();

        Invoke("OpenDoor", 5f);
    }

    public override void OnExitProgress()
    {

    }

    public override void OnUpdateProgress()
    {

    }

    public void OpenDoor()
    {
        door.OpenDoor();
    }
}
