using UnityEngine;

public abstract class ProgressStepBase : MonoBehaviour
{
    // 자식 클래스들이 공통으로 사용할 매니저 참조
    protected ProgressManager manager;

    protected virtual void Awake()
    {
        manager = ProgressManager.Instance;
    }

    // 자식들이 반드시 구현해야 할 핵심 로직 (추상 메서드)
    public abstract void OnEnterProgress();

    // 선택적으로 구현할 로직 (가상 메서드)
    public virtual void OnUpdateProgress() { }
    public virtual void OnExitProgress() { }
}
