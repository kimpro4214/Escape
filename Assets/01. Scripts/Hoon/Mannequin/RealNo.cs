using UnityEngine;

public class RealNo : MonoBehaviour
{
    private void OnMouseDown()
    {
        MannequinManager.Instance.MannequinActivate();
        MannequinManager.Instance.checkReal.SetActive(false);
        transform.parent.gameObject.SetActive(false);
    }
}
