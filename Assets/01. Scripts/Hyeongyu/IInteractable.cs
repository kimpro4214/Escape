/// <summary>
/// Raycast 기반 시선 탐지 및 상호작용 키 입력 시 호출되는 인터페이스.
/// 상호작용 가능한 오브젝트에 구현한다.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// 플레이어가 상호작용 키를 눌렀을 때 호출된다.
    /// </summary>
    void OnInteract();
}
