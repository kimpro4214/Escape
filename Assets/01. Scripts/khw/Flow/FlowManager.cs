using System.Collections.Generic;
using UnityEngine;
// 플로우 관리를 담당하는 매니저 클래스

public class FlowManager : MonoBehaviour
{
    public static FlowManager instance;

    public void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    #region callables

    //이전 플로우 리스트가 완료되면 재생될 플로우를 추가
    public void AddFlow(List<FlowBase> flows)
    {

    }

    //플로우 강제 시작. 이전 플로우 리스트를 모두 제거
    public void ForceStartFlow(List<FlowBase> flows)
    {

    }

    #endregion

    #region utilities

    void HandleFlowList(ref List<FlowBase> newFlowList)
    {

    }

    #endregion
}
