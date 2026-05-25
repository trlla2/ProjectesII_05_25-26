// cada state porta la seva condició de transició que comprova al Update. La grid té una funció
// ChangeState que canvia d'estat i crida les funcions Enter i Exit corresponents.
public interface IGridState
{
    void Enter();
    void Update();
    void Exit();
    GridStateType GetStateType();
}
