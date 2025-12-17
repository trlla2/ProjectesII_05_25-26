using System.Collections.Generic;
using UnityEngine;

public abstract class BoardTile : MonoBehaviour
{
    [SerializeField] private BoardTileData _data;

    [SerializeField] private bool isForcedPos;
    [SerializeField] private Vector2Int forcedPos = new Vector2Int();

    private Vector2Int _gridPos;
    private Renderer _renderer;

    public List<BoardTile> neighbors = new List<BoardTile>();

    private void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
        _renderer.material.color = _data.defaultColor;
        if(isForcedPos)
            _gridPos = forcedPos;
    }

    public void HighlightTile(bool on)
    {
        if (_renderer != null)
        {
            _renderer.material.color = on ? _data.highlightColor : _data.defaultColor;
        }
    }

    public bool IsWalkable()
    {
        return _data.walkable;
    }

    public abstract void Interact();

    public Vector2Int GetGridPosition() { return _gridPos; }
    public void SetGridPosition(Vector2Int gridPos) { _gridPos = gridPos; }

    public float GetYPosition() { return transform.position.y; }
    public void SetYPosition(float y) { transform.position = new Vector3(_gridPos.x, y, _gridPos.y); }

    public BoardTileData GetData() { return this._data; }

    public Renderer GetRenderer() { return _renderer; }

    
}
