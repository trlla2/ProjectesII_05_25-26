using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BranchChoicePanelHandler : MonoBehaviour
{
    [SerializeField] private Button _rightButton;
    [SerializeField] private Button _leftButton;
    [SerializeField] private TileClickHandler _tileClickHandler;
    [SerializeField] private PlayerMovement _player;
    [SerializeField] private BoardCameraController _boardCameraController;

    private BoardTile _rightTile;
    private BoardTile _leftTile;

    private void Start()
    {
        _rightButton.onClick.AddListener(OnRightChosen);
        _leftButton.onClick.AddListener(OnLeftChosen);
        StartCoroutine(DisableNextFrame());
    }

    // SetActive(false) deferred one frame so UI mesh generation can finish first
    private IEnumerator DisableNextFrame()
    {
        yield return null;
        Debug.LogWarning("[BRANCH] DisableNextFrame -> SetActive(false)");
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _rightButton.onClick.RemoveListener(OnRightChosen);
        _leftButton.onClick.RemoveListener(OnLeftChosen);
    }

    public void Show(BoardTile rightTile, BoardTile leftTile)
    {
        Debug.LogWarning($"[BRANCH] Panel.Show called. activeBefore={gameObject.activeSelf}");
        _rightTile = rightTile;
        _leftTile = leftTile;

        _tileClickHandler.HandleBoardRaycast(false);
        gameObject.SetActive(true);
        _boardCameraController?.ZoomIn();
        Debug.LogWarning($"[BRANCH] Panel.Show done. activeAfter={gameObject.activeSelf} activeInHierarchy={gameObject.activeInHierarchy}");
    }

    private void Hide()
    {
        gameObject.SetActive(false);
        _tileClickHandler.HandleBoardRaycast(true);
        _boardCameraController?.ZoomOut();
    }

    private void OnRightChosen() => Choose(_rightTile);
    private void OnLeftChosen()  => Choose(_leftTile);

    private void Choose(BoardTile firstTileOfChosenPath)
    {
        if (firstTileOfChosenPath == null) return;

        Hide();
        _player.TryMoveTo(firstTileOfChosenPath, () =>
        {
            firstTileOfChosenPath.OnPlayerArrive();
            _player.MoveForward(/*arrivedTile*/() =>
            {
                //if (arrivedTile == null) return;
                //arrivedTile.OnPlayerArrive();
                //if (arrivedTile.IsBranchOrigin())
                //{
                //    arrivedTile.Interact();
                //}
            });
        });
    }
}