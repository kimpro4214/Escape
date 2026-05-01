using System.Collections;
using TMPro;
using UnityEngine;

public class DescriptionController : MonoBehaviour
{
    TextMeshProUGUI subtitleText;

    private void Awake()
    {
        subtitleText ??= GetComponentInChildren<DescriptionText>().GetComponent<TextMeshProUGUI>();
    }

    public void ShowDescription(string descText, float postDelay)
    {
        UnDisplayDescription();

        StopAllCoroutines();

        SetText(descText);

        StartCoroutine(DescriptionShowCoroutine(postDelay));
    }

    #region utilities

    private void DisplayDescription()
    {
        gameObject.SetActive(true);
    }

    private void UnDisplayDescription()
    {
        gameObject.SetActive(false);
    }

    private void SetText(string text)
    {
        subtitleText.text = $"{text}";
    }

    IEnumerator DescriptionShowCoroutine(float delay)
    {
        DisplayDescription();

        yield return new WaitForSeconds(delay);

        UnDisplayDescription();
    }

    #endregion
}
