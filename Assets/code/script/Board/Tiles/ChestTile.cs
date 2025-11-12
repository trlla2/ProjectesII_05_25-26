using UnityEngine;

public class ChestTile : BoardTile
{
    [SerializeField] private BoardTileData _postInteractionTile;

    public override void Interact()
    {
        Debug.Log("opening chest...");

        // once interacted with, convert to other type of tile
        BoardManager.instance.ReplaceTileType(this, _postInteractionTile);
    }
    

}
