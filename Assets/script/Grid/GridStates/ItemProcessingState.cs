public class ItemProcessingState : IGridState
{
    private Grid grid;

    public ItemProcessingState(Grid gridController)
    {
        grid = gridController;
    }

    public void Enter() { }
    public void Update() { }
    public void Exit() { }
    public GridStateType GetStateType() => GridStateType.Processing;
}
