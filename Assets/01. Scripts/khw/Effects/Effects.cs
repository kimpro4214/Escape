using UnityEngine;

public abstract class Effects : MonoBehaviour
{
    [SerializeField] private float maxPlayTime;
    private float currentPlayTime;

    public abstract void PlayEffect();
    
    public abstract void EndEffect();

    void Update()
    {
        UpdateTime();
        CheckTime();
    }

    void UpdateTime()
    {
        currentPlayTime += Time.deltaTime;
    }

    void CheckTime()
    {
        if(currentPlayTime > maxPlayTime) Destroy(gameObject);
    }
}
