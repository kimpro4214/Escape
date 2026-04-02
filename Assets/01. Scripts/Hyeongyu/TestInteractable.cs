using UnityEngine;

/// <summary>
/// IInteractable 구현 테스트용 컴포넌트. 상호작용 시 콘솔에 로그를 출력한다.
/// </summary>
public class TestInteractable : MonoBehaviour, IInteractable
{
    /// <summary>
    /// 플레이어가 이 오브젝트와 상호작용할 때 호출되며 오브젝트 이름을 콘솔에 출력한다.
    /// </summary>
    public void OnInteract()
    {
        Debug.Log($"[TestInteractable] '{gameObject.name}' interacted!");
    }
}
