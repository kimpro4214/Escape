using UnityEngine;

// 사용할 때
//IFlow openDoorFlow = new Flow_DoorOpen(targetDoor);
//openDoorFlow.StartFlow();
public class Flow_DoorOpen : IFlow
{
    private readonly Progress_Door _doorToOpen;

    public Flow_DoorOpen(Progress_Door door)
    {
        _doorToOpen = door;
    }

    public void EndFlow()
    {

    }

    public void StartFlow()
    {
        OpenDoor();
    }

    private void OpenDoor()
    {
        if (_doorToOpen != null)
        {
            _doorToOpen.OpenDoor();
        }
        else
        {
            Debug.LogWarning("Flow_DoorOpen: No door assigned to open.");
        }

    }
}
