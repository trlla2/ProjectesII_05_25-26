using System.Collections.Generic;
using UnityEngine;

public class ChestOfEgressTile : BoardTile
{
    [SerializeField] private Color _chestColor;
    [SerializeField] private List<GameObject> _chestObjs;

    private void Start()
    {
        foreach (GameObject chestObj in _chestObjs)
        {
            Renderer chestRenderer = chestObj.GetComponent<Renderer>();
            if (chestRenderer != null)
                chestRenderer.material.color = _chestColor;
        }
    }

    public override void Interact()
    {
        _boardManager.OpenChestOfEgress(this);
    }

    public void Replace()
    {
        ReplaceWithPostInteraction();
    }
}
