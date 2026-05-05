using UnityEngine;

public class Exiter : MonoBehaviour
{
    private void OnMouseDown()
    {
        MannequinManager.Instance.MannequinExit();
    }
}
