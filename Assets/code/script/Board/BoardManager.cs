using UnityEngine;
using System.Collections.Generic;
using System.Net;

public class BoardManager : MonoBehaviour
{
    [SerializeField] private bool _forcedBoard;
    public bool ForcedBoard => _forcedBoard;
    //[SerializeField] private bool _isTutorial;
    public StageLevelData currentStageLevelData;

    public BoardTile startingTile;
    public BoardTileData emptyTileData;
    public BoardTileData enemyTileData;
    public BoardTileData chestTileData;
    public BoardTileData bossTileData;
    public BoardTileData stairsTileData;
    public BoardTileData chestOfEgressTileData;

    [SerializeField] private BoardGenerator boardGenerator;

    public List<BoardTile> generatedTiles;

    [SerializeField] private OpenChestScreen openChestPanel;
    [SerializeField] private NewSpellPanelBehaviour newSpellPanel;
    [SerializeField] private ChestOfEgressScreen chestOfEgressPanel;
    [SerializeField] private BoardTileData fogTileData;

    [Header("Encounters")]
    [SerializeField] private EncountersManager encountersManager;
    public EncountersManager EncountersManager => encountersManager;
    private int _enemiesDefeated = 0;

    private bool _branchPathChoosed = false;
    public List<BoardTile> _revealedBranch;

    private Camera boardCamera;

    private void Awake()
    {
        if (!_forcedBoard)
        {
            if (encountersManager == null)
                encountersManager = GetComponent<EncountersManager>();

            if (encountersManager == null)
            {
                enabled = false;
                return;
            }

            if (GameManager.Instance == null)
            {
                enabled = false;
                return;
            }

            currentStageLevelData = GameManager.Instance.GetCurrentStageData();
            if (currentStageLevelData == null)
            {
                Debug.LogError("BoardManager: currentStageLevelData es null.");
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
        }
        

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

        if (newData == fogTileData) newTile.GetComponent<FogTile>().SetFoggedTileData(tile.GetData());

        newTile.generatedBranch = tile.generatedBranch;

        generatedTiles.Remove(tile);
        Destroy(tile.gameObject);
    }

    public void PutFogOnRevealedBranch()
    {
        if (_revealedBranch == null) return;

        PutFogOnBranch(_revealedBranch);

        if (_revealedBranch != null) _revealedBranch = null;
    }


    public void PutFogOnTile(BoardTile tile)
    {
        if (tile.GetData() == stairsTileData) return;
        ReplaceTileType(tile, fogTileData);
    }

    public void PutFogOnBranch(List<BoardTile> branch)
    {
        // Guardar las posiciones antes de destruir los tiles
        List<Vector2Int> branchPositions = new List<Vector2Int>();
        foreach (BoardTile tile in branch)
            branchPositions.Add(tile.GetGridPosition());

        BoardTile originTile = branch[0]; // la EmptyTile origen no se reemplaza

        foreach (BoardTile tile in branch)
        {
            if (tile == branch[0]) continue;
            ReplaceTileType(tile, fogTileData);
        }

        // Reconstruir la lista de la branch con las FogTiles recién creadas
        List<BoardTile> updatedBranch = new List<BoardTile>();
        foreach (Vector2Int pos in branchPositions)
        {
            BoardTile found = generatedTiles.Find(t => t.GetGridPosition() == pos);
            if (found != null)
                updatedBranch.Add(found);
        }

        originTile.generatedBranch = updatedBranch;
    }

    public void RevealBranch(List<BoardTile> branch)
    {
        List<BoardTile> snapshot = new List<BoardTile>(branch);

        List<Vector2Int> branchPositions = new List<Vector2Int>();
        foreach (BoardTile tile in snapshot)
            branchPositions.Add(tile.GetGridPosition());

        foreach (BoardTile tile in snapshot)
        {
            if (tile != null && tile.GetData() == fogTileData)
            {
                FogTile fogTile = tile.GetComponent<FogTile>();
                ReplaceTileType(tile, fogTile.FoggedTileData);
            }
        }

        List<BoardTile> updatedBranch = new List<BoardTile>();
        foreach (Vector2Int pos in branchPositions)
        {
            BoardTile found = generatedTiles.Find(t => t.GetGridPosition() == pos);
            if (found != null)
                updatedBranch.Add(found);
        }

        // Actualizar generatedBranch del origin con las referencias actuales
        BoardTile originTile = updatedBranch.Count > 0 ? updatedBranch[0] : null;
        if (originTile != null)
            originTile.generatedBranch = updatedBranch;

        _revealedBranch = updatedBranch;
    }


    public void OpenChest(ChestTile chestTile)
    {
        openChestPanel.openedChest = chestTile;
        openChestPanel.Pull(currentStageLevelData.GetPullItemRarity());
        openChestPanel.gameObject.SetActive(true);
    }

    public void OpenChestOfEgress(ChestOfEgressTile tile)
    {
        if (chestOfEgressPanel != null)
            chestOfEgressPanel.Open(tile);
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

    public BoardGenerator GetBoardGenerator() { return boardGenerator; }

    public BoardTileData GetFogTileData() { return fogTileData; }

    public bool HasChoosedBranchPath() { return _branchPathChoosed; }
    public void BranchPathChoosed() { _branchPathChoosed = true; }

    //public bool IsTutorial() { return _isTutorial; }
}
