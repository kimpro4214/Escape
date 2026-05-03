using UnityEngine;

public interface IProgressExecuter
{
    public void OpenDoor();
    public void CloseDoor();
}

public class Progress_Door : MonoBehaviour, IProgressExecuter
{
    [SerializeField] Animator animator;
    [SerializeField] AudioSource doorOpenSource;

    public void CloseDoor()
    {
        animator.SetTrigger("Close");
        doorOpenSource.Play();
    }

    public void OpenDoor()
    {
        animator.SetTrigger("Open");
        doorOpenSource.Play();
    }

    void Awake()
    {
        animator ??= GetComponent<Animator>();
        doorOpenSource =GetComponent<AudioSource>();
    }
}
