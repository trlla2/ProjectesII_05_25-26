using UnityEngine;

public enum TileType { EMPTY, ENEMY, CHEST, EGRESS }

[CreateAssetMenu(fileName = "BoardTileData", menuName = "ScriptableObjects/Board/BoardTileData")]
public class BoardTileData : ScriptableObject
{
    public TileType type;
    public GameObject tilePrefab;
    public bool walkable;
    
    public Color defaultColor;
    public Color highlightColor = Color.yellow;

}
