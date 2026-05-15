using System.Collections.Generic;
using UnityEngine;

public class TTSVoiceTest : MonoBehaviour
{
    private void Update()
    {
        // 1 누르면 마녀 목소리
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("눌렀냐>?");
            VoiceManager.Instance.AddVoice(new VoiceLine(
                null,
                ESubtitleCharacters.witch,
                "앞의 모자가 보여? 네가 하는 질문에 대답해줄 모자야.",
                5f
            ));
        }

        // 2 누르면 모자 목소리
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("MagicHat : 누름");
            VoiceManager.Instance.AddVoice(new VoiceLine(
                null,
                ESubtitleCharacters.magicHat,
                "맞았어. 문을 열어줄테니까 다음 테스트를 받도록 해.",
                1f
            ));
        }

        // 3 누르면 연속 재생 테스트
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            VoiceManager.Instance.AddVoice(new List<VoiceLine>()
            {
                new VoiceLine(null, ESubtitleCharacters.witch, "질문을 잘 선택하는게 좋을거야.", 1f),
                new VoiceLine(null, ESubtitleCharacters.magicHat, "다시 생각해봐, 그래선 마녀님의 조수가 될 수 없어.", 1f)
            });
        }
    }
}