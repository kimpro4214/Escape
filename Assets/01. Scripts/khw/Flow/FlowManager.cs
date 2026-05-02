using System.Collections.Generic;
using UnityEngine;
// 플로우 관리를 담당하는 매니저 클래스

public class FlowManager : MonoBehaviour
{
    public static FlowManager Instance;

    public void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    #region callables

    public void StartFlow(IFlow flow)
    {
        flow.StartFlow();
    }

    public void StopFlow(IFlow flow)
    {
        flow.EndFlow();
    }

    #endregion
}
