using UnityEngine;

[System.Serializable]
public class BoardGenerationConfig
{
    [Header("Reserved Zones")]
    public int EndZoneReservedTiles;
    public int BossTileDistanceFromEnd;
    public int StartZoneReserveTiles;

    [Header("Branched Paths")]
    [Range(0, 2)] public int MinDistBetweenPaths;

    [Header("Branch Neighbor Tiles Spawn")]
    public bool AllowBranchUp;
    public bool AllowBranchDown;
    public bool AllowBranchLeft;
    public bool AllowBranchRight;

    public int ChestOfEgressTileDistFromEnd => BossTileDistanceFromEnd - 2;
}
