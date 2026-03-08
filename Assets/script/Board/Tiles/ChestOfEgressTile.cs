using System.Collections.Generic;
using UnityEngine;

public class ChestOfEgressTile : BoardTile
{
    [SerializeField] private Color _chestColor;
    [SerializeField] private List<GameObject> _chestObjs;

    [SerializeField] private BoardTileData _postInteractionTile;

    private void Start()
    {
        foreach (GameObject chestObj in _chestObjs)
        {
            Renderer chestRenderer = chestObj.GetComponent<Renderer>();
            if (chestRenderer != null)
                chestRenderer.material.color = _chestColor;
            else
                Debug.LogWarning($"ChestOfEgressTile: No Renderer found on {chestObj.name}");
        }
    }

    public override void Interact()
    {
        _boardManager.OpenChestOfEgress(this);
    }

    public void Replace()
    {
        if (_postInteractionTile != null)
            _boardManager.ReplaceTileType(this, _postInteractionTile);
    }
}
