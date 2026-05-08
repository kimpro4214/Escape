public class Flow_ClueFound_First : IFlow
{
    public void StartFlow()
    {
        DescriptionManager.Instance?.AddDescription(
            new GameDescription("알아낸게 있어? 참고가 된다면 좋겠네.", 3f));
    }

    public void EndFlow() { }
}
