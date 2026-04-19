using UnityEngine;

public class ChestTile : BoardTile
{
    [SerializeField] private AudioSource openChestSource;

    public override void Interact()
    {
        openChestSource.Play();
        Camera.main.GetComponent<TileClickHandler>().HandleBoardRaycast(false);
        _boardManager.OpenChest(this);
    }

    public void Replace()
    {
        ReplaceWithPostInteraction();
    }
}
