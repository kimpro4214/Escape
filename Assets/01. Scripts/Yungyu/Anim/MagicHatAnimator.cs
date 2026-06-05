using UnityEngine;

public class MagicHatAnimator : MonoBehaviour
{
    public static MagicHatAnimator Instance;
    private Animator animator;

    void Start()
    {
        Instance = this;
        animator = GetComponent<Animator>();
    }

    // 말하는 순간 이 함수 호출
    public void PlayTalk01Animation()
    {
        animator.SetTrigger("Talk01");
    }

    public void PlayTalk02Animation()
    {
        animator.SetTrigger("Talk02");
    }

    public void PlayEntry()
    {
        animator.SetTrigger("Entry");
    }

    public void PlayIdle()
    {
        animator.SetTrigger("Idle");
    }
}