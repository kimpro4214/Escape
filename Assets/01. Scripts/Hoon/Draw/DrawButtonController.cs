using UnityEngine;
using UnityEngine.UI;

public class DrawButtonController : MonoBehaviour
{
    public Button[] buttons;
    public static DrawButtonController Instance;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        DeactivateButtons();
    }

    public void ActivateButtons()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                buttons[i].enabled = true;
            }
        }
    }

    public void DeactivateButtons()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                buttons[i].enabled = false;
            }
        }
    }
}
