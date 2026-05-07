public class Flow_ClueFound_Middle : IFlow
{
    public void StartFlow()
    {
        DescriptionManager.Instance?.AddDescription(
            new GameDescription("하나 더 찾아냈네, 완성한다면 어떤 모양일지 생각해봐.", 3f));
    }

    public void EndFlow() { }
}
