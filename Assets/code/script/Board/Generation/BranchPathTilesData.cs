using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BranchPathTilesData", menuName = "ScriptableObjects/Board/BranchPathTilesData")]
public class BranchPathTilesData : ScriptableObject
{
    public List<BoardTileData> branchPathTiles;
}
