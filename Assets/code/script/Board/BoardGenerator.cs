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
        InstanceBoard(this.transform);
        //InstanceHallOfEgress();
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
}
