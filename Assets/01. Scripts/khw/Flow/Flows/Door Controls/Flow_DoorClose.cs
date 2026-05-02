using UnityEngine;

// 사용할 때
//IFlow closeDoorFlow = new Flow_DoorClose(targetDoor);
//closeDoorFlow.StartFlow();
public class Flow_DoorClose : IFlow
{
    private readonly Progress_Door _doorToClose;

    public Flow_DoorClose(Progress_Door door)
    {
        _doorToClose = door;
    }

    public void EndFlow()
    {

    }

    public void StartFlow()
    {
        CloseDoor();
    }

    private void CloseDoor()
    {
        if (_doorToClose != null)
        {
            _doorToClose.CloseDoor();
        }
        else
        {
            Debug.LogWarning("Flow_DoorClose: No door assigned to close.");
        }

    }
}

