using UnityEngine;

public class EmptyTile : BoardTile
{
    public override void Interact()
    {
        Debug.Log($"arrived to {this.GetGridPosition()}...");

        if (_isOriginOfBranch)
        {
            _boardManager.ShowBranchChoice(this);
            return;
        }

        TryToRevealGeneratedBranchIfHasOne();
    }
}
