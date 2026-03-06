using UnityEngine;

public class EmptyTile : BoardTile
{
    public override void Interact()
    {
        Debug.Log($"arrived to {this.GetGridPosition()}...");

        if (generatedBranch == null) return;

        bool branchIsHidden = generatedBranch.Count > 1
            && generatedBranch[1].GetData() == _boardManager.GetFogTileData();

        if (branchIsHidden)
            _boardManager.RevealBranch(generatedBranch);
    }
}
