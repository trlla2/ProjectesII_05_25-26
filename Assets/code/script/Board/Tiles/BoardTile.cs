using System.Collections.Generic;
using UnityEngine;

public abstract class BoardTile : MonoBehaviour
{
    [SerializeField] private BoardTileData _data;

    private Vector2Int _gridPos;
    private Renderer _renderer;

    public List<BoardTile> neighbors = new List<BoardTile>();

    private void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
        _renderer.material.color = _data.defaultColor;
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

    public BoardTileData GetData() { return this._data; }

    public Renderer GetRenderer() { return _renderer; }

    
}
