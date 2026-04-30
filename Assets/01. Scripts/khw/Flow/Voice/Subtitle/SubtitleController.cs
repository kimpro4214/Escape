using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SubtitleController : MonoBehaviour
{
    TextMeshProUGUI subtitleText;

    private void Awake()
    {
        subtitleText ??= GetComponentInChildren<SubtitleText>().GetComponent<TextMeshProUGUI>();
    }

    public void ShowSubtitle(string characterName, string subtitle, float postDelay)
    {
        UnDisplaySubtitle();

        StopAllCoroutines();

        SetText(characterName, subtitle);

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

    private void SetText(string charName, string text)
    {
        subtitleText.text = $"{charName}\n{text}";
    }

    IEnumerator SubtitleShowCoroutine(float delay)
    {
        DisplaySubtitle();

        yield return new WaitForSeconds(delay);

        UnDisplaySubtitle();
    }

    #endregion
}
