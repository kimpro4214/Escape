using UnityEngine;

public class Exiter : MonoBehaviour
{
    public bool isActive = false;

    public void Activate() => isActive = true;
    public void Deactivate() => isActive = false;


private void OnMouseDown()
    {
        if (!isActive) return;
        MannequinManager.Instance.MannequinExit();
    }
}
