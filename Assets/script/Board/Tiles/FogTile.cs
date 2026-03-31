using UnityEngine;

public class FogTile : BoardTile
{
    [SerializeField] private BoardTileData _foggedTileData;
    public BoardTileData FoggedTileData => _foggedTileData;

    public override void Interact() 
    {
        //if (_boardManager.HasChoosedBranchPath()) return;

        //foreach (BoardTile neighbor in neighbors)
        //    if (neighbor.generatedBranch != null && neighbor.generatedBranch.Count > 1 
        //        && neighbor.generatedBranch[1] == this)
        //    {
        //        _boardManager.RevealBranch(neighbor.generatedBranch);
        //        return;
        //    }
                
    }

    public void SetFoggedTileData(BoardTileData foggedTileData)
    {
        _foggedTileData = foggedTileData;
    }
}
