using UnityEngine;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public StageLevelData currentStageLevelData;

    public BoardTile startingTile;
    public BoardTileData emptyTileData;
    public BoardTileData enemyTileData;
    public BoardTileData chestTileData;
    public BoardTileData bossTileData;
    public BoardTileData stairsTileData;

    [SerializeField] private BoardGenerator boardGenerator;

    public List<BoardTile> generatedTiles;

    [SerializeField] private OpenChestScreen openChestPanel;
    [SerializeField] private NewSpellPanelBehaviour newSpellPanel;

    [Header("Encounters")]
    [SerializeField] private EncountersManager encountersManager;
    public EncountersManager EncountersManager => encountersManager;
    private int _enemiesDefeated = 0;

    private Camera boardCamera;

    //public PlayerMovement player;

    private void Awake()
    {
        Debug.Log($"[BoardManager] Awake en '{name}'. encountersManager (serializado) = {encountersManager}");

        if (encountersManager == null)
            encountersManager = GetComponent<EncountersManager>();

        Debug.Log($"[BoardManager] Tras GetComponent, encountersManager = {encountersManager}");

        if (encountersManager == null)
        {
            Debug.LogError("BoardManager: EncountersManager es null. Asegúrate de que el componente está en el mismo GameObject o asignado en el inspector.");
            enabled = false;
            return;
        }

        if (G.GM == null)
        {
            Debug.LogError("BoardManager: GameManager (G.GM) es null. La escena de tablero se está cargando sin GameManager.");
            enabled = false;
            return;
        }

        currentStageLevelData = G.GM.GetCurrentStageData();
        if (currentStageLevelData == null)
        {
            Debug.LogError("BoardManager: currentStageLevelData es null. Revisa _stageDatas y stageLevel en GameManager.");
            enabled = false;
            return;
        }

        var enemies = currentStageLevelData.GetEnemyEncounters() ?? new List<EnemyType>();
        var order = currentStageLevelData.GetEncountersOrder() ?? new List<EnemyDifficulty>();
        var bosses = currentStageLevelData.GetBossEncounters() ?? new List<EnemyType>();

        Debug.Log($"BoardManager: cargando StageLevelData con {enemies.Count} enemigos, {order.Count} encuentros, {bosses.Count} bosses.");

        encountersManager.LoadEnemyEncounters(enemies);
        encountersManager.LoadEnemyEncountersOrder(order);
        encountersManager.LoadBossesEncounters(bosses);

        startingTile.SetGridPosition(new Vector2Int(0, 0));
        generatedTiles.Add(startingTile);

        boardCamera = Camera.main;
    }

    public void UpdateNeighbors(BoardTile tile)
    {
        List<Vector2Int> possiblePositions = new List<Vector2Int>
        {
            new Vector2Int(-1, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(0, 1)
        };

        for (int i = 0; i < generatedTiles.Count; i++)
        {
            for (int j = 0; j < possiblePositions.Count; j++)
            {
                if (generatedTiles[i].GetGridPosition() == tile.GetGridPosition() + possiblePositions[j])
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
        newTile.SetBoardManager(this);

        generatedTiles.Add(newTile);
        Debug.Log($"casilla generada en {gridPosition}");

        return newTile;
    }

    public void ReplaceTileType(BoardTile tile, BoardTileData newData)
    {
        if (tile == null) return;
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

    public void IncrementEnemyDefeatedCount()
    {
        _enemiesDefeated++;
    }

    //public PlayerMovement GetPlayer() { return player; }

    public BoardGenerator GetBoardGenerator() { return boardGenerator; }
}
