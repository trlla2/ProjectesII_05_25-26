using UnityEngine;

public class EnemyTile : BoardTile
{
    [SerializeField] private BoardTileData _postInteractionTile;
    [SerializeField] private int lootRatePercentage;

    private TileClickHandler _boardRaycast;

    private void Start()
    {
        _boardRaycast = Camera.main.GetComponent<TileClickHandler>();
    }

    public override void Interact()
    {
        Debug.Log("starting combat...");

        EnemyType enemyToFight = ResolveEnemy();
        if (enemyToFight == null)
        {
            Debug.LogError("EnemyTile: ResolveEnemy() devolvió null, no se inicia combate.");
            return;
        }

        G.GM.SetPendingEnemy(enemyToFight);

        _boardRaycast.HandleBoardRaycast(false);
        SceneTransition.Instance.LoadScene("BattlePhone", true);
        G.GM.EnterBattle();

        G.GM.OnEnterBoard += Replace;
        //if (G.GM.CombatHandler != null)
        //    G.GM.CombatHandler.OnEndBattle += Replace;
    }

    protected virtual EnemyType ResolveEnemy()
    {
        if (_boardManager == null || _boardManager.EncountersManager == null)
        {
            Debug.LogError("EnemyTile: BoardManager o EncountersManager es null.");
            return null;
        }

        return _boardManager.EncountersManager.GetNextEncounter();
    }

    public void Replace()
    {
        _boardRaycast.HandleBoardRaycast(true);

        //if (G.GM != null && G.GM.CombatHandler != null)
        //    G.GM.CombatHandler.OnEndBattle -= Replace;

        _boardManager.ReplaceTileType(this, _postInteractionTile);
        _boardManager.IncrementEnemyDefeatedCount();
    }

    public void ReplaceWhenWin(bool win, int xp, int money)
    {
        _boardRaycast.HandleBoardRaycast(true);

        if (G.GM != null && G.GM.CombatHandler != null)
            G.GM.CombatHandler.OnEndBattle -= ReplaceWhenWin;

        if (win)
        {
            _boardManager.ReplaceTileType(this, _postInteractionTile);
            _boardManager.IncrementEnemyDefeatedCount();
        }
    }

    private void OnDestroy()
    {
        if (G.GM != null && G.GM.CombatHandler != null)
            G.GM.OnEnterBoard -= Replace;
    }
}
