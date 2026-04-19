using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BranchOriginTile : BoardTile
{
    private List<BoardTile> generatedBranch;
    public List<BoardTile> GeneratedBranch => generatedBranch;

    public UnityEvent OnInteract;
    public UnityEvent OnPathChosen;

    public override void Interact()
    {
        _boardManager.ShowBranchChoice(this);
        OnInteract?.Invoke();
    }

    public void TryToRevealGeneratedBranch()
    {
        if (generatedBranch == null) return;

        bool branchIsHidden = generatedBranch.Count > 1
            && generatedBranch[1].GetData() == _boardManager.GetFogTileData();

        if (branchIsHidden)
            _boardManager.RevealBranch(generatedBranch);
    }

    public void OnBranchPathChosen(List<BoardTile> chosenPath)
    {
        if (_boardManager == null || _boardManager._revealedBranch == null
            || _boardManager._revealedBranch.Count == 0 || _boardManager.ForcedBoard) return;

        bool isInsideRevealedBranch = _boardManager._revealedBranch.Contains(this);

        if (isInsideRevealedBranch && !_boardManager.HasChoosedBranchPath())
        {
            _boardManager.BranchPathChoosed();

            List<BoardTile> snapshot = new List<BoardTile>(_boardManager.generatedTiles);
            foreach (BoardTile tile in snapshot)
            {
                if (_boardManager._revealedBranch.Contains(tile)) continue;
                _boardManager.PutFogOnTile(tile);
            }

            _boardManager.startingTile = _boardManager.generatedTiles
                .Find(t => t.GetGridPosition() == new Vector2Int(0, 0));
        }
        else if (!isInsideRevealedBranch)
        {
            _boardManager.PutFogOnRevealedBranch();
        }

        OnPathChosen.Invoke();
    }

    public void SetGeneratedBranch(List<BoardTile> branchTiles)
    {
        generatedBranch = branchTiles;
    }
}
