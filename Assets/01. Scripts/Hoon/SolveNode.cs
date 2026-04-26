using UnityEngine;

public class SolveNode : MonoBehaviour
{
    private void OnMouseDown()
    {
        GetComponentInParent<Puzzle2>().Solve();
    }
}
