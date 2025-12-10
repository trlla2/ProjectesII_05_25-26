using System.Collections.Generic;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    [SerializeField] float _spaceBetweenTiles;
    [SerializeField] int _numOfEmptyTilesInBoard;
    [SerializeField] int _numOfEnemyTilesInBoard;
    [SerializeField] int _numOfChestTilesInBoard;
    [SerializeField] int _numOfEgressTilesInBoard;
    int _numOfTotalTilesInBoard;

    private void Awake()
    {
        _numOfTotalTilesInBoard = _numOfEmptyTilesInBoard + _numOfEnemyTilesInBoard + _numOfChestTilesInBoard + _numOfEgressTilesInBoard;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InstanceBoard();
    }

    // Update is called once per frame
    void Update()
    {
        
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
        List<BoardTile> availableTiles = new List<BoardTile>(BoardManager.instance.generatedTiles);
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
}
