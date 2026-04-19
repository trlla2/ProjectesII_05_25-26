using UnityEngine;

public class FogTile : BoardTile
{
    [SerializeField] private BoardTileData _foggedTileData;
    public BoardTileData FoggedTileData => _foggedTileData;

    public override void Interact() 
    {
                
    }

    public void SetFoggedTileData(BoardTileData foggedTileData)
    {
        _foggedTileData = foggedTileData;
    }
}
