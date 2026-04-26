using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SubtitleManager : MonoBehaviour
{
    [Header("자막 UI 연결")]
    public Text subtitleText;        // 자막 텍스트 컴포넌트
    public GameObject subtitlePanel; // 자막 배경 패널 (반투명 검은색 배경 등, 없으면 비워둬도 됨)

    [Header("설정")]
    public float defaultDisplayTime = 5f; // 기본 자막 표시 시간

    private Coroutine hideCoroutine;

    private void Start()
    {
        // 시작할 때 자막을 숨깁니다.
        HideSubtitle();
    }

    // 자막을 화면에 띄우는 함수
    public void ShowSubtitle(string speaker, string message, float customTime = 0f)
    {
        // 화자에 따라 이름 색상을 다르게 지정
        string colorHex = speaker == "플레이어" ? "#5A9BD5" : "#ED7D31";

        // 텍스트 교체 (+= 가 아니라 = 입니다!)
        subtitleText.text = $"<color={colorHex}><b>[{speaker}]</b></color> {message}";

        // UI 켜기
        if (subtitlePanel != null) subtitlePanel.SetActive(true);
        subtitleText.gameObject.SetActive(true);

        // 기존에 돌고 있던 숨기기 타이머가 있다면 취소 (자막이 겹쳐서 빨리 사라지는 것 방지)
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        // customTime이 0보다 크면 그 시간만큼, 아니면 기본 시간(3초)만큼 띄움
        float displayTime = customTime > 0f ? customTime : defaultDisplayTime;

        // n초 뒤에 숨기는 타이머 시작
        hideCoroutine = StartCoroutine(HideAfterDelay(displayTime));
    }

    // 자막을 즉시 숨기는 함수
    public void HideSubtitle()
    {
        if (subtitleText != null) subtitleText.text = "";
        if (subtitlePanel != null) subtitlePanel.SetActive(false);
        if (subtitleText != null) subtitleText.gameObject.SetActive(false);
    }

    // 일정 시간 대기 후 숨기는 코루틴
    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideSubtitle();
    }
}