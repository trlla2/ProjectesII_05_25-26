using UnityEngine;

public class ChestTile : BoardTile
{
    [SerializeField] private BoardTileData _postInteractionTile;
    [SerializeField] private AudioSource openChestSource;
    //bool isOpen = false;

    public override void Interact()
    {
        Debug.Log("opening chest...");
        openChestSource.Play();
        Camera.main.GetComponent<TileClickHandler>().HandleBoardRaycast(false);
        BoardManager.instance.OpenChest(this);
    }

    public void Replace()
    {
        BoardManager.instance.ReplaceTileType(this, _postInteractionTile);
    }
    

}
