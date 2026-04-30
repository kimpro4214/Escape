using UnityEngine;

public interface IFlow
{
    public void StartFlow();
    public void EndFlow();
}

public class FlowBase : MonoBehaviour, IFlow
{
    public virtual void EndFlow()
    {

    }

    public virtual void StartFlow()
    {

    }

}
