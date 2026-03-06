using UnityEngine;
using UnityEngine.Tilemaps;

public class TileClickHandler : MonoBehaviour
{
    [SerializeField] Camera cam;

    private Ray mouseRay;
    private BoardTile _lastTileHovered;
    [SerializeField] PlayerMovement player;

    [SerializeField] private bool raycastActive = true;

    private void Awake()
    {
        //player = GetComponent<PlayerMovement>();
        cam = gameObject.GetComponent<Camera>();
    }
    void Update()
    {
        mouseRay = cam.ScreenPointToRay(Input.mousePosition);

        if (!player.isMoving/*FORCED*/ && raycastActive) 
        {
            //HandleMouseHover();
            if (Input.GetMouseButtonDown(0))
                HandleTileClick();
        }
    }

    void HandleMouseHover()
    {
        if (Physics.Raycast(mouseRay, out RaycastHit hit))
        {
            if (hit.collider.TryGetComponent(out BoardTile hoverTile))
            {
                if (_lastTileHovered != null)
                    _lastTileHovered.HighlightTile(false);
                _lastTileHovered = hoverTile;
                hoverTile.HighlightTile(true);

            }
        }
        else
        {
            if (_lastTileHovered != null)
                _lastTileHovered.HighlightTile(false);
        }
    }

    void HandleTileClick()
    {
        if (Physics.Raycast(mouseRay, out RaycastHit hit))
        {
            BoardTile clickedTile = hit.collider.GetComponent<BoardTile>();
            if (clickedTile == null)
                return;

            clickedTile.HighlightTile(true);
            player.TryMoveTo(clickedTile, () =>
            {
                clickedTile.HighlightTile(false);

                Vector2Int playerGridPos = new Vector2Int(Mathf.RoundToInt(player.transform.position.x), Mathf.RoundToInt(player.transform.position.z));
                if (playerGridPos == clickedTile.GetGridPosition())
                    clickedTile.OnPlayerArrive(); // oculta branch revelada si clickedTile no es su origin
                clickedTile.Interact();
            });

            if (!player.isMoving && clickedTile.IsHighlighted())
                clickedTile.HighlightTile(false);
        }
    }

    public void HandleBoardRaycast(bool enable)
    {
        raycastActive = enable;
    }
}
