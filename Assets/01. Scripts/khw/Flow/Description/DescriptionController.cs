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

    public void ShowSubtitle(string descText, float postDelay)
    {
        UnDisplaySubtitle();

        StopAllCoroutines();

        SetText(descText);

        StartCoroutine(SubtitleShowCoroutine(postDelay));
    }

    #region utilities

    private void DisplaySubtitle()
    {
        gameObject.SetActive(true);
    }

    private void UnDisplaySubtitle()
    {
        gameObject.SetActive(false);
    }

    private void SetText(string text)
    {
        subtitleText.text = $"{text}";
    }

    IEnumerator SubtitleShowCoroutine(float delay)
    {
        DisplaySubtitle();

        yield return new WaitForSeconds(delay);

        UnDisplaySubtitle();
    }

    #endregion
}
