using UnityEngine;

//캐릭터 정보클래스 - 캐릭터에 부착

public class CharacterBase : MonoBehaviour
{
    [SerializeField] public ESubtitleCharacters character;
    [SerializeField] public string characterName;

    private void Awake()
    {
        if (string.IsNullOrEmpty(characterName))
        {
            characterName = character.ToString();
        }
    }
}
