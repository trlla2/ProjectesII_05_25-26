using UnityEngine;
using System.Collections.Generic;
using System.Net;

public class BoardManager : MonoBehaviour
{
    [SerializeField] private bool _forcedBoard;
    public bool ForcedBoard => _forcedBoard;

    [SerializeField] private bool _isTutorial;
    public bool IsTutorial => _isTutorial;

    [SerializeField] private bool _isHallOfEgress;
    public bool IsHallOfEgress => _isHallOfEgress;

    public StageLevelData currentStageLevelData;

    public BoardTile startingTile;
    public BoardTileData emptyTileData;
    public BoardTileData branchOriginTileData;
    public BoardTileData enemyTileData;
    public BoardTileData chestTileData;
    public BoardTileData bossTileData;
    public BoardTileData stairsTileData;
    public BoardTileData chestOfEgressTileData;
    public BoardTileData goldTileData;

    [SerializeField] private BoardGenerator boardGenerator;
    [SerializeField] private PlayerMovement _player;
    public PlayerMovement Player => _player;

    public List<BoardTile> generatedTiles;

    [SerializeField] private OpenChestScreen openChestPanel;
    [SerializeField] private NewSpellPanelBehaviour newSpellPanel;
    [SerializeField] private ChestOfEgressScreen chestOfEgressPanel;
    [SerializeField] private BranchChoicePanelHandler branchChoicePanel;
    [SerializeField] private ShopScreen shopPanel;
    [SerializeField] private SlotMachineScreen slotMachinePanel;


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

        return newTile;
    }

    public BranchOriginTile CreateBranchOriginTile(Vector2Int gridPosition, Transform parent)
    {
        BoardTile tile = CreateNewTile(branchOriginTileData, gridPosition, parent);
        return tile as BranchOriginTile;
    }

    public void ReplaceTileType(BoardTile tile, BoardTileData newData)
    {
        if (tile == null) return;
        BoardTile newTile = CreateNewTile(newData, tile.GetGridPosition(), tile.transform.parent);
        newTile.SetGridPosition(tile.GetGridPosition());

        foreach (BoardTile neighbor in tile.neighbors)
        {
            if (neighbor != null)
                neighbor.neighbors.Remove(tile);
        }

        UpdateNeighbors(newTile);

        if (newData == fogTileData) newTile.GetComponent<FogTile>().SetFoggedTileData(tile.GetData());

        // para no corromper la lista durante PutFogOnBranch
        if (newData != fogTileData && _revealedBranch != null && _revealedBranch.Contains(tile))
            _revealedBranch.Add(newTile);

        if (tile is BranchOriginTile)
        {
            BranchOriginTile branchOrigin = tile as BranchOriginTile;
            BranchOriginTile newBranchOrigin = newTile as BranchOriginTile;
            if (branchOrigin.GeneratedBranch != null && newTile is BranchOriginTile)
                newBranchOrigin.SetGeneratedBranch(branchOrigin.GeneratedBranch);
        }

        generatedTiles.Remove(tile);
        Destroy(tile.gameObject);
    }

    public void PutFogOnRevealedBranch()
    {
        if (_revealedBranch == null) return;

        List<BoardTile> branchToHide = new List<BoardTile>(_revealedBranch);
        _revealedBranch = null; // limpiar ANTES de iterar para evitar que ReplaceTileType la corrompa

        PutFogOnBranch(branchToHide);
    }

    public void PutFogOnTile(BoardTile tile)
    {
        if (tile.GetData() == stairsTileData) return;
        ReplaceTileType(tile, fogTileData);
    }

    public void PutFogOnBranch(List<BoardTile> branch)
    {
        List<Vector2Int> branchPositions = new List<Vector2Int>();
        foreach (BoardTile tile in branch)
            branchPositions.Add(tile.GetGridPosition());

        BranchOriginTile originTile = branch[0] as BranchOriginTile;

        foreach (BoardTile tile in branch)
        {
            if (tile == branch[0]) continue;
            ReplaceTileType(tile, fogTileData);
        }

        List<BoardTile> updatedBranch = new List<BoardTile>();
        foreach (Vector2Int pos in branchPositions)
        {
            BoardTile found = generatedTiles.Find(t => t.GetGridPosition() == pos);
            if (found != null)
                updatedBranch.Add(found);
        }

        originTile.SetGeneratedBranch(updatedBranch);
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

        BranchOriginTile originTile = updatedBranch.Count > 0 ? updatedBranch[0] as BranchOriginTile : null;
        if (originTile != null)
            originTile.SetGeneratedBranch(updatedBranch);

        _revealedBranch = updatedBranch;
    }

    public void ShowBranchChoice(BranchOriginTile originTile)
    {
        if (branchChoicePanel == null || originTile.GeneratedBranch == null
            || originTile.GeneratedBranch.Count < 2) return;

        Vector2Int originPos     = originTile.GetGridPosition();
        Vector2Int branchNextPos = originTile.GeneratedBranch[1].GetGridPosition();

        BoardTile mainNext = null;
        foreach (BoardTile neighbor in originTile.neighbors)
        {
            if (neighbor.GetGridPosition() == branchNextPos) continue;
            if (!neighbor.IsWalkable()) continue;

            Vector2Int delta = neighbor.GetGridPosition() - originPos;
            bool isForward = (delta.x > 0 && delta.y == 0)
                          || (delta.x == 0 && delta.y > 0)
                          || (delta.x > 0 && delta.y > 0);
            if (isForward)
            {
                mainNext = neighbor;
                break;
            }
        }

        if (mainNext == null) return;

        RevealBranch(originTile.GeneratedBranch);

        BoardTile freshBranchNext = generatedTiles.Find(t => t.GetGridPosition() == branchNextPos);
        if (freshBranchNext == null) return;


        BoardTile rightTile, leftTile;
        if (mainNext.GetGridPosition().x == _player.GetCurrentTile().GetGridPosition().x + 1)
        {
            rightTile = mainNext;
            leftTile = freshBranchNext;
        }
        else
        {
            if (mainNext.GetGridPosition().y == _player.GetCurrentTile().GetGridPosition().y + 1)
            {
                if (freshBranchNext.GetGridPosition().x == _player.GetCurrentTile().GetGridPosition().x + 1)
                {
                    rightTile = freshBranchNext;
                    leftTile = mainNext;
                }
                else
                {
                    rightTile = mainNext;
                    leftTile = freshBranchNext;
                }
            }
            else
            {
                rightTile = freshBranchNext;
                leftTile = mainNext;
            }
        }

            branchChoicePanel.Show(rightTile, leftTile);
    }

    public void OpenChest(ChestTile chestTile)
    {
        if (openChestPanel == null) return;
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

    public void OpenShop(ShopTile tile)
    {
        if (shopPanel != null)
            shopPanel.Open(tile);
        else
            Debug.LogWarning("BoardManager: shopPanel no está asignado.");
    }

    public void OpenSlotMachineScreen(SlotMachineTile tile)
    {
        if (slotMachinePanel != null)
            slotMachinePanel.Open(tile);
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
}
