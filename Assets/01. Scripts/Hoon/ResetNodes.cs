using UnityEngine;

public class ResetNodes : MonoBehaviour
{

    private void OnMouseDown()
    {
        Puzzle2Manager.instance.ResetAllNodes();
    }
}
