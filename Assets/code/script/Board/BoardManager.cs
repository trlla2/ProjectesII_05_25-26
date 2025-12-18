using UnityEngine;
using System.Collections.Generic;


public class BoardManager : MonoBehaviour
{
    public static BoardManager instance { get; private set; }

    public BoardTile startingTile;
    public BoardTileData emptyTileData;
    public BoardTileData enemyTileData;
    public BoardTileData chestTileData;
    public BoardTileData bossTileData;
    public BoardTileData stairsTileData;

    public List<BoardTile> generatedTiles;

    [SerializeField] private OpenChestScreen openChestPanel;
    [SerializeField] private NewSpellPanelBehaviour newSpellPanel;



    private Camera boardCamera;

    private void Awake()
    {
        if (instance != null && instance != this) 
            Destroy(this.gameObject);
        else
            instance = this;

        startingTile.SetGridPosition(new Vector2Int(0, 0));
        generatedTiles.Add(startingTile);

        boardCamera = Camera.main;
    }

    public void UpdateNeighbors(BoardTile tile)
    {

        List<Vector2Int> possiblePositions = new List<Vector2Int>();
        possiblePositions.Add(new Vector2Int(-1, 0));
        possiblePositions.Add(new Vector2Int(1, 0));
        possiblePositions.Add(new Vector2Int(0, -1));
        possiblePositions.Add(new Vector2Int(0, 1));

        for (int i = 0; i < generatedTiles.Count; i++)
        {
            for (int j = 0; j < possiblePositions.Count; j++)
            {
                if (generatedTiles[i].GetComponent<BoardTile>().GetGridPosition() == tile.GetGridPosition() + possiblePositions[j])
                {

                    if (!tile.neighbors.Contains(generatedTiles[i]))
                        tile.neighbors.Add(generatedTiles[i]); 

                    if (!generatedTiles[i].neighbors.Contains(tile))
                        generatedTiles[i].neighbors.Add(tile);
                }
            }
        }
    }

    public BoardTile CreateNewTile(BoardTileData tileData, Vector2Int gridPosition, Transform parent)
    {
        GameObject newTileInstance = Instantiate(tileData.tilePrefab,
                new Vector3(gridPosition.x, startingTile.transform.position.y, gridPosition.y),
                startingTile.gameObject.transform.rotation, parent);

        BoardTile newTile = newTileInstance.GetComponent<BoardTile>();
        newTile.SetGridPosition(gridPosition);

        generatedTiles.Add(newTile);
        Debug.Log($"casilla generada en{gridPosition}");

        return newTile;
    }

    public void ReplaceTileType(BoardTile tile, BoardTileData newData)
    {
        //Vector3 pos = tile.transform.position;
        //Quaternion rot = tile.transform.rotation;
        //Transform parent = tile.transform.parent;

        BoardTile newTile = CreateNewTile(newData, tile.GetGridPosition(), tile.transform.parent);
        newTile.SetGridPosition(tile.GetGridPosition());

        UpdateNeighbors(newTile);

        generatedTiles.Remove(tile);

        Destroy(tile.gameObject);
    }

    public void OpenChest(ChestTile chestTile)
    {
        openChestPanel.openedChest = chestTile;
        openChestPanel.Pull();

        boardCamera.gameObject.GetComponent<TileClickHandler>().HandleBoardRaycast(false);

        openChestPanel.gameObject.SetActive(true);
    }

    public void GetNewSpell(NewSpellTile tile)
    {
        boardCamera.gameObject.GetComponent<TileClickHandler>().HandleBoardRaycast(false);
        newSpellPanel.Show(tile);
    }

    public void HandleBoardCamera(bool enable)
    {
        boardCamera.gameObject.SetActive(enable);
    }
}
