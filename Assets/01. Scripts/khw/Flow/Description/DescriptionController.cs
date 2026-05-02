using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class DescriptionController : MonoBehaviour
{
    private TextMeshProUGUI descriptionText;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        // 텍스트 컴포넌트 캐싱
        descriptionText ??= GetComponentInChildren<DescriptionText>().GetComponent<TextMeshProUGUI>();

        // CanvasGroup 캐싱
        canvasGroup = GetComponent<CanvasGroup>();

        // 초기 상태: 숨김
        UnDisplayDescription();
    }

    public void ShowDescription(string descText, float postDelay)
    {
        // 진행 중인 연출 중지 (중복 실행 방지)
        StopAllCoroutines();

        // 텍스트 설정
        SetText(descText);

        // 연출 시작
        StartCoroutine(DescriptionShowCoroutine(postDelay));
    }

    #region UI Control Logic

    private void DisplayDescription()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

    private void UnDisplayDescription()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    private void SetText(string text)
    {
        if (descriptionText != null)
        {
            descriptionText.text = text;
        }
    }

    IEnumerator DescriptionShowCoroutine(float delay)
    {
        DisplayDescription();

        // postDelay 동안 화면에 유지
        yield return new WaitForSeconds(delay);

        UnDisplayDescription();
    }

    #endregion
}
