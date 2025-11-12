using UnityEngine;

public class ChestTile : BoardTile
{
    [SerializeField] private BoardTileData _postInteractionTile;

    //bool isOpen = false;

    public override void Interact()
    {
        Debug.Log("opening chest...");
        BoardManager.instance.OpenChest(this);

        //BoardManager.instance.ReplaceTileType(this, _postInteractionTile);
    }

    public void Replace()
    {
        BoardManager.instance.ReplaceTileType(this, _postInteractionTile);
    }
    

}
