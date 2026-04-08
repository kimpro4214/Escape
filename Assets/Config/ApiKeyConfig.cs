using UnityEngine;

[CreateAssetMenu(fileName = "ApiKeyConfig", menuName = "Config/ApiKeyConfig")]
public class ApiKeyConfig : ScriptableObject
{
    [Header("OpenAI")]
    public string openAIKey;

    [Header("Supertone")]
    public string supertoneKey;
}
