using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))] // CanvasGroup이 반드시 필요함
public class SubtitleController : MonoBehaviour
{
    private TextMeshProUGUI subtitleText;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        // 최적화: GetComponentInChildren 보다는 명확한 참조가 좋지만, 구조를 유지하며 캐싱합니다.
        subtitleText ??= GetComponentInChildren<SubtitleText>().GetComponent<TextMeshProUGUI>();

        // CanvasGroup 캐싱
        canvasGroup = GetComponent<CanvasGroup>();

        // 시작할 때 보이지 않게 초기화
        UnDisplaySubtitle();
    }

    public void ShowSubtitle(string characterName, string subtitle, float postDelay)
    {
        // 진행 중이던 연출 중지
        StopAllCoroutines();

        // 텍스트 먼저 셋팅
        SetText(characterName, subtitle);

        // 연출 시작
        StartCoroutine(SubtitleShowCoroutine(postDelay));
    }

    #region UI Control Logic

    private void DisplaySubtitle()
    {
        // 알파를 1로 만들고, 클릭 등의 상호작용이 필요하다면 활성화
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

    private void UnDisplaySubtitle()
    {
        // 알파를 0으로 만들어 감춤
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    private void SetText(string charName, string text)
    {
        if (subtitleText != null)
        {
            string coloredName = $"<color=#FFFF00>{charName}</color>";

            subtitleText.text = $"{coloredName}\n{text}";
        }
    }

    IEnumerator SubtitleShowCoroutine(float delay)
    {
        DisplaySubtitle();

        // 만약 여기서 "페이드 인" 효과를 넣고 싶다면 별도의 Lerp 로직을 추가할 수 있습니다.

        yield return new WaitForSeconds(delay);

        UnDisplaySubtitle();
    }

    #endregion
}
