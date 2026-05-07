public class Flow_ClueFound_Last : IFlow
{
    public void StartFlow()
    {
        DescriptionManager.Instance?.AddDescription(
            new GameDescription("다 찾은 것 같네, 마법진을 그릴 시간이야.", 4f));
    }

    public void EndFlow() { }
}
