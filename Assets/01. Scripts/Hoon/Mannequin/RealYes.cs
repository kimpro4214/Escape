using UnityEngine;

public class RealYes : MonoBehaviour
{
    public MannequinCapturer mannequinCapturer;
    private void OnMouseDown()
    {
        mannequinCapturer.CaptureToFile();
        MannequinManager.Instance.checkReal.SetActive(false);
    }
}
