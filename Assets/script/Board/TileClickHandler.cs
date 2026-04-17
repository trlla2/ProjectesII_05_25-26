using UnityEngine;
using UnityEngine.EventSystems;

public class TileClickHandler : MonoBehaviour
{
    [SerializeField] Camera cam;

    private Ray mouseRay;
    private BoardTile _lastTileHovered;
    [SerializeField] private BoardManager _boardManager;

    [SerializeField] private bool raycastActive;

    private void Awake()
    {
        cam = gameObject.GetComponent<Camera>();
    }

    void Update()
    {
        if (_boardManager.IsHallOfEgress)
            mouseRay = cam.ScreenPointToRay(Input.mousePosition);

        if (!_boardManager.Player.isMoving && raycastActive)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (_boardManager.IsHallOfEgress)
                    HandleTileClick();
                else
                    HandleScreenClick();
            }
        }
    }

    void HandleTileClick()
    {
        if (IsPointerOverUI()) return;

        if (Physics.Raycast(mouseRay, out RaycastHit hit))
        {
            BoardTile clickedTile = hit.collider.GetComponent<BoardTile>();
            if (clickedTile == null) return;

            clickedTile.HighlightTile(true);
            _boardManager.Player.TryMoveTo(clickedTile, () =>
            {
                clickedTile.HighlightTile(false);

                Vector2Int playerGridPos = new Vector2Int(
                    Mathf.RoundToInt(_boardManager.Player.transform.position.x),
                    Mathf.RoundToInt(_boardManager.Player.transform.position.z));

                if (playerGridPos == clickedTile.GetGridPosition())
                    clickedTile.OnPlayerArrive();

                clickedTile.Interact();
            });

            if (!_boardManager.Player.isMoving && clickedTile.IsHighlighted())
                clickedTile.HighlightTile(false);
        }
    }

    void HandleScreenClick()
    {
        if (IsPointerOverUI()) return;

        _boardManager.Player.MoveUntilStop/*From*/(/*_boardManager.Player.GetCurrentTile(),*/ arrivedTile =>
        {
            if (arrivedTile == null) return;

            // arrivedTile es SIEMPRE la tile donde está el jugador (EmptyTile o BranchOrigin)
            arrivedTile.OnPlayerArrive();

            if (arrivedTile.IsBranchOrigin())
            {
                // El jugador está sobre la branch origin: interactúa con ella
                // y espera el input del panel de elección
                arrivedTile.Interact();
            }
            else
            {
                // El jugador está en una EmptyTile normal: interactúa con la
                // siguiente tile no-walkable sin moverse
                BoardTile nextInteractable = _boardManager.Player
                    .GetNextForwardTileFrom(arrivedTile, walkableOnly: false);

                if (nextInteractable != null)
                    nextInteractable.Interact();
            }
        });
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    public void HandleBoardRaycast(bool enable)
    {
        raycastActive = enable;
    }
}
