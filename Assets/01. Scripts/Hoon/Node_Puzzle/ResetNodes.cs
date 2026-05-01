using UnityEngine;

public class ResetNodes : MonoBehaviour
{

    private void OnMouseDown()
    {
        GetComponentInParent<Puzzle2>().Reset();
    }
}
