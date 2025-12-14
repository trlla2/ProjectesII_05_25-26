using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    [SerializeField] float _spaceBetweenTiles;

    [SerializeField] int _numOfEmptyTiles;
    [SerializeField] int _numOfStairs;

    [SerializeField] int _numOfEnemyTiles;
    [SerializeField] int _numOfMiniBossTiles;
    [SerializeField] int _numOfBossesTiles;

    [SerializeField] int _numOfChestTiles;
    [SerializeField] int _numOfEgressTiles;
    
    int _numOfTotalTilesInBoard;

    private void Awake()
    {
        _numOfTotalTilesInBoard = _numOfEmptyTiles + _numOfEnemyTiles + _numOfChestTiles + _numOfEgressTiles;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InstanceBoard(this.transform);
        //InstanceHallOfEgress(this.transform);
    }

    void InstanceBoard(Transform parent)
    {
        BoardTile currentTile = BoardManager.instance.startingTile;

        for (int i = 0; i < _numOfTotalTilesInBoard; i++)
        {
            Vector2Int nextGridPosition;
            if (i == _numOfTotalTilesInBoard - 1)
                nextGridPosition = currentTile.GetGridPosition() + new Vector2Int(0, 1);
            else
                nextGridPosition = GetRandomFreeNeighborPosition(currentTile);

            BoardTile nextTile = BoardManager.instance.CreateNewTile(BoardManager.instance.emptyTileData, nextGridPosition, parent);

            BoardManager.instance.UpdateNeighbors(currentTile);

            currentTile = nextTile;

        }

        AssignSpecialTiles();

    }

    Vector2Int GetRandomFreeNeighborPosition(BoardTile current)
    {
        List<Vector2Int> freePositions = new List<Vector2Int>();
        //if (current.GetGridPosition() != startingTile.GetGridPosition())
        //    freePositions.Add(new Vector2Int(current.GetGridPosition().x-1, current.GetGridPosition().y));
        freePositions.Add(new Vector2Int(current.GetGridPosition().x + 1, current.GetGridPosition().y));
        //freePositions.Add(new Vector2Int(current.GetGridPosition().x, current.GetGridPosition().y - 1));
        freePositions.Add(new Vector2Int(current.GetGridPosition().x, current.GetGridPosition().y + 1));

        //for (int i = 0; i < BoardManager.instance.generatedTiles.Count; i++)
        //{
        //    for (int j = 0; j < freePositions.Count; j++)
        //    {
        //        if (BoardManager.instance.generatedTiles[i].GetComponent<BoardTile>().GetGridPosition() == freePositions[j])
        //        {
        //            freePositions.RemoveAt(j);
        //        }
        //    }
        //}
        foreach(BoardTile tile in BoardManager.instance.generatedTiles)
        {
            foreach(Vector2Int pos in freePositions)
            {
                if (tile.GetGridPosition() == pos)
                {
                    freePositions.Remove(pos);
                }
            }
        }

        return freePositions[Random.Range(0, freePositions.Count)];
    }

    void AssignSpecialTiles()
    {
        List<BoardTile> stairsTiles = new List<BoardTile>();
        for (int i = 0; i < _numOfStairs; i++)
        {
            stairsTiles.Add(BoardManager.instance.generatedTiles.ElementAt(BoardManager.instance.generatedTiles.Count-1 - i));
        }
        
        List<BoardTile> availableTiles = new List<BoardTile>(BoardManager.instance.generatedTiles);

                                                        // also reserved 1 empty tile between each different one
        int numOfTilesReservedForEndZone = _numOfStairs +1+ _numOfBossesTiles +1+ _numOfEgressTiles + 1;
        availableTiles.RemoveRange(availableTiles.Count - numOfTilesReservedForEndZone, numOfTilesReservedForEndZone);

        availableTiles.Remove(BoardManager.instance.startingTile);
        Shuffle(availableTiles);

        int assignedEnemies = 0;
        int assignedChests = 0;

        foreach (var tile in availableTiles)
        {
            if (assignedEnemies < _numOfEnemyTiles)
            {
                BoardManager.instance.ReplaceTileType(tile, BoardManager.instance.enemyTileData);
                assignedEnemies++;
            }
            else if (assignedChests < _numOfChestTiles)
            {
                BoardManager.instance.ReplaceTileType(tile, BoardManager.instance.chestTileData);
                assignedChests++;
            }

            if (assignedEnemies >= _numOfEnemyTiles && assignedChests >= _numOfChestTiles)
                break;
        }

        for (int i = 0; i < stairsTiles.Count; i++)
        {
            BoardManager.instance.ReplaceTileType(stairsTiles[i], BoardManager.instance.stairsTileData);
            if (i != 0)
                stairsTiles[i].SetGridPosition(stairsTiles[0].GetGridPosition() + new Vector2Int(0, 1));

            stairsTiles[i].SetYPosition(stairsTiles[i].GetYPosition() - (0.5f*i));
        }

        //foreach(BoardTile tile in stairsTiles)
        //{
        //    BoardManager.instance.ReplaceTileType(tile, BoardManager.instance.stairsTileData);
        //    if (tile != stairsTiles[0])
        //    {
        //        tile.SetGridPosition(stairsTiles[0].GetGridPosition() + new Vector2Int(0, 1));
        //        tile.SetYPosition(stairsTiles.ElementAt(tile.).GetYPosition() - 0.5f);
        //    }
                
        //}
        //BoardTile lastTile = BoardManager.instance.generatedTiles.
        

        Debug.Log($"Asignadas {assignedEnemies} tiles de enemigos y {assignedChests} tiles de cofres.");
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }



    // WADIYATALKINABEET

    List<Vector2Int> GenerateSquareGrid(Vector2Int origin, int size)
    {
        List<Vector2Int> positions = new List<Vector2Int>();

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                positions.Add(new Vector2Int(origin.x + x, origin.y + y));
            }
        }

        return positions;
    }

    void InstanceHallOfEgress(Transform parent)
    {
        BoardTile currentTile = BoardManager.instance.startingTile;

        List<Vector2Int> gridPositions = GenerateSquareGrid(currentTile.GetGridPosition(), 4);

        foreach (var pos in gridPositions)
        {
            if (pos == currentTile.GetGridPosition())
                continue;

            BoardTile nextTile = BoardManager.instance.CreateNewTile(BoardManager.instance.emptyTileData, pos, parent);
            BoardManager.instance.UpdateNeighbors(currentTile);

            currentTile = nextTile;
        }
    }
}
