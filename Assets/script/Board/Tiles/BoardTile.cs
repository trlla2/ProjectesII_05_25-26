using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BoardTile : MonoBehaviour
{
    [SerializeField] private BoardTileData _data;

    [SerializeField] private bool isForcedPos;
    [SerializeField] private Vector2Int forcedPos = new Vector2Int();

    private Vector2Int _gridPos;
    private Renderer _renderer;

    public List<BoardTile> neighbors = new List<BoardTile>();
    protected BoardManager _boardManager;

    public List<BoardTile> generatedBranch;

    private void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
        _renderer.material.color = _data.defaultColor;

        _boardManager = FindObjectOfType<BoardManager>();

        if (isForcedPos)
            _gridPos = forcedPos;
        if (_boardManager.ForcedBoard)
            _gridPos = new Vector2Int((int)transform.position.x, (int)transform.position.z);

        
    }

    public void OnPlayerArrive()
    {
        if (_boardManager == null || _boardManager._revealedBranch == null || _boardManager._revealedBranch.Count == 0 || _boardManager.ForcedBoard) return;

        bool isOriginOfRevealedBranch = _boardManager._revealedBranch[0] == this;
        if (isOriginOfRevealedBranch) return;

        bool isInsideBranch = _boardManager._revealedBranch.Contains(this);

        if (isInsideBranch && !_boardManager.HasChoosedBranchPath())
        {
            _boardManager.BranchPathChoosed();

            List<BoardTile> snapshot = new List<BoardTile>(_boardManager.generatedTiles);
            foreach (BoardTile tile in snapshot)
            {
                if (_boardManager._revealedBranch.Contains(tile)) continue;
                _boardManager.PutFogOnTile(tile);
            }
            _boardManager.startingTile = _boardManager.generatedTiles.Where(tile => tile.GetGridPosition() == new Vector2Int(0, 0)).First();
        }
        else if (!isInsideBranch)
        {
            _boardManager.PutFogOnRevealedBranch();
        }
    }

    public void HighlightTile(bool on)
    {
        if (_renderer != null)
            _renderer.material.color = on ? _data.highlightColor : _data.defaultColor;
    }

    public bool IsHighlighted()
    {
        if (_renderer != null)
            return _renderer.material.color == _data.highlightColor;
        return false;
    }

    public bool IsWalkable() { return _data.walkable; }

    public abstract void Interact();

    protected void TryToRevealGeneratedBranchIfHasOne()
    {
        if (generatedBranch == null) return;

        bool branchIsHidden = generatedBranch.Count > 1
            && generatedBranch[1].GetData() == _boardManager.GetFogTileData();

        if (branchIsHidden)
            _boardManager.RevealBranch(generatedBranch);
    }

    public Vector2Int GetGridPosition() { return _gridPos; }
    public void SetGridPosition(Vector2Int gridPos) { _gridPos = gridPos; }

    public float GetYPosition() { return transform.position.y; }
    public void SetYPosition(float y) { transform.position = new Vector3(_gridPos.x, y, _gridPos.y); }

    public BoardTileData GetData() { return this._data; }
    public Renderer GetRenderer() { return _renderer; }
    public void SetBoardManager(BoardManager boardManager) { _boardManager = boardManager; }
}
