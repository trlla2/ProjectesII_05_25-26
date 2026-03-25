using UnityEngine;
using UnityEngine.UI;

public class BranchChoicePanelHandler : MonoBehaviour
{
    [SerializeField] private Button _rightButton;
    [SerializeField] private Button _leftButton;
    [SerializeField] private TileClickHandler _tileClickHandler;
    [SerializeField] private PlayerMovement _player;

    private BoardTile _mainTile;
    private BoardTile _branchTile;

    private void Awake()
    {
        _rightButton.onClick.AddListener(OnRightChosen);
        _leftButton.onClick.AddListener(OnLeftChosen);
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _rightButton.onClick.RemoveListener(OnRightChosen);
        _leftButton.onClick.RemoveListener(OnLeftChosen);
    }

    public void Show(BoardTile mainTile, BoardTile branchTile)
    {
        _mainTile = mainTile;
        _branchTile = branchTile;

        _tileClickHandler.HandleBoardRaycast(false);
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
        _tileClickHandler.HandleBoardRaycast(true);
    }

    private void OnRightChosen() => Choose(_mainTile);
    private void OnLeftChosen()  => Choose(_branchTile);

    private void Choose(BoardTile firstTileOfChosenPath)
    {
        if (firstTileOfChosenPath == null) return;

        Hide();

        _player.MoveUntilStopFrom(firstTileOfChosenPath, arrivedTile =>
        {
            if (arrivedTile == null) return;

            arrivedTile.OnPlayerArrive();

            if (arrivedTile.IsBranchOrigin())
            {
                arrivedTile.Interact();
            }
            else
            {
                BoardTile nextInteractable = _player
                    .GetNextForwardTileFrom(arrivedTile, walkableOnly: false);

                if (nextInteractable != null)
                    nextInteractable.Interact();
            }
        });
    }
}