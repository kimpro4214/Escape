using UnityEngine;

public class SolveNode : MonoBehaviour
{
    private void OnMouseDown()
    {
        Puzzle2Manager.instance.SolveMinimum();
    }
}
