using UnityEngine;

public enum ESubtitleCharacters
{
     None,
     witch,
     magicHat
}

public class CharacterFinder : MonoBehaviour
{
    public static CharacterBase FindCharacter(ESubtitleCharacters character)
    {
        switch (character)
        {
            case ESubtitleCharacters.witch:
                return FindAnyObjectByType<Witch>();
            case ESubtitleCharacters.magicHat:
                return FindAnyObjectByType<MagicHat>();
            default:
                return null;
        }

    }
}
