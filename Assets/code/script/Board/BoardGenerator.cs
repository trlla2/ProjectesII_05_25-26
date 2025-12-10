using System.Collections.Generic;
using System.Linq;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    [SerializeField] float _spaceBetweenTiles;
    [SerializeField] int _numOfEmptyTilesInBoard;
    [SerializeField] int _numOfEnemyTilesInBoard;
    [SerializeField] int _numOfChestTilesInBoard;
    [SerializeField] int _numOfEgressTilesInBoard;
    [SerializeField] int _numOfMiniBosses;
    int _numOfTotalTilesInBoard;

    private void Awake()
    {
        _numOfTotalTilesInBoard = _numOfEmptyTilesInBoard + _numOfEnemyTilesInBoard + _numOfChestTilesInBoard + _numOfEgressTilesInBoard;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InstanceBoard();
        //InstanceHallOfEgress();
    }

    void InstanceBoard()
    {
        BoardTile currentTile = BoardManager.instance.startingTile;

        for (int i = 0; i < _numOfTotalTilesInBoard; i++)
        {
            Vector2Int nextGridPosition = GetRandomFreeNeighborPosition(currentTile);
            // falta seleccion de tipo de casilla (solo genera emptys)
            BoardTile nextTile = BoardManager.instance.CreateNewTile(BoardManager.instance.emptyTileData, nextGridPosition);

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

        for (int i = 0; i < BoardManager.instance.generatedTiles.Count; i++)
        {
            for (int j = 0; j < freePositions.Count; j++)
            {
                if (BoardManager.instance.generatedTiles[i].GetComponent<BoardTile>().GetGridPosition() == freePositions[j])
                {
                    freePositions.RemoveAt(j);
                }
            }
        }

        return freePositions[Random.Range(0, freePositions.Count)];
    }

    void AssignSpecialTiles()
    {
        BoardTile lastTile = BoardManager.instance.generatedTiles.ElementAt(BoardManager.instance.generatedTiles.Count - 1);
        List<BoardTile> availableTiles = new List<BoardTile>(BoardManager.instance.generatedTiles);
        // remove last 6 tiles (reserved for stairs, boss and chest of egress)
        availableTiles.RemoveRange(availableTiles.Count - 6, 6);
        // remove starting tile from special tiles gen
        availableTiles.Remove(BoardManager.instance.startingTile);
        Shuffle(availableTiles);

        int assignedEnemies = 0;
        int assignedChests = 0;

        foreach (var tile in availableTiles)
        {
            if (assignedEnemies < _numOfEnemyTilesInBoard)
            {
                BoardManager.instance.ReplaceTileType(tile, BoardManager.instance.enemyTileData);
                assignedEnemies++;
            }
            else if (assignedChests < _numOfChestTilesInBoard)
            {
                BoardManager.instance.ReplaceTileType(tile, BoardManager.instance.chestTileData);
                assignedChests++;
            }

            if (assignedEnemies >= _numOfEnemyTilesInBoard && assignedChests >= _numOfChestTilesInBoard)
                break;
        }

        
        //BoardTile lastTile = BoardManager.instance.generatedTiles.
        BoardManager.instance.ReplaceTileType(lastTile, BoardManager.instance.stairsTileData);

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

    void InstanceHallOfEgress()
    {
        BoardTile currentTile = BoardManager.instance.startingTile;

        List<Vector2Int> gridPositions = GenerateSquareGrid(currentTile.GetGridPosition(), 4);

        foreach (var pos in gridPositions)
        {
            if (pos == currentTile.GetGridPosition())
                continue;

            BoardTile nextTile = BoardManager.instance.CreateNewTile(BoardManager.instance.emptyTileData, pos);
            BoardManager.instance.UpdateNeighbors(currentTile);

            currentTile = nextTile;
        }
    }
}
