using UnityEngine;
using UnityEngine.UI; // Legacy Text와 ScrollRect, Button 등을 위해 필요

public class ChatLogManager : MonoBehaviour
{
    [Header("UI 컴포넌트 연결")]
    public GameObject chatPanel;       // 대화창 전체 패널 (껐다 켜기용)
    public Text chatText;              // 텍스트가 누적될 Legacy Text 컴포넌트 (수정됨)
    public ScrollRect scrollRect;      // 스크롤바

    private void Start()
    {
        // 시작할 때 텍스트를 비우고 패널을 숨깁니다.
        if (chatText != null) chatText.text = "";
        if (chatPanel != null) chatPanel.SetActive(false);
    }

    // 대화를 추가하는 함수
    public void AddLog(string speaker, string message)
    {
        // 화자에 따라 이름 색상을 다르게 지정 (Legacy Text도 Rich Text 지원)
        string colorHex = speaker == "플레이어" ? "#5A9BD5" : "#ED7D31";

        // 텍스트 누적
        chatText.text += $"<color={colorHex}><b>[{speaker}]</b></color> {message}\n\n";

        Debug.Log($"[현재 전체 텍스트 상태]\n{chatText.text}");

        // 텍스트가 추가되면 스크롤을 맨 아래로 자동으로 내려줍니다.
        Canvas.ForceUpdateCanvases();
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 0f;
    }

    // UI 버튼 이벤트용: 대화창 껐다 켜기
    public void ToggleChatWindow()
    {
        if (chatPanel != null)
        {
            chatPanel.SetActive(!chatPanel.activeSelf);
        }
    }
}