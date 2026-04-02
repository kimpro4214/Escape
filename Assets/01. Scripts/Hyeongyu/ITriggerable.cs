/// <summary>
/// Collider 영역 진입/퇴장 이벤트를 처리하는 인터페이스.
/// 트리거 기반으로 반응해야 하는 오브젝트에 구현한다.
/// </summary>
public interface ITriggerable
{
    /// <summary>
    /// 플레이어(또는 지정 오브젝트)가 트리거 영역에 진입할 때 호출된다.
    /// </summary>
    void OnTriggerEnterAction();

    /// <summary>
    /// 플레이어(또는 지정 오브젝트)가 트리거 영역에서 퇴장할 때 호출된다.
    /// </summary>
    void OnTriggerExitAction();
}
