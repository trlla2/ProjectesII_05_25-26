using UnityEngine;

public class EnemyTile : BoardTile
{
    [SerializeField] private BoardTileData _postInteractionTile;
    [SerializeField] private int lootRatePercentage;

    [Header("Force Enemy Settings")]
    [SerializeField] private bool forceSpecificEnemy = false;
    [SerializeField] private EnemyType specificEnemyType;

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

        if (G.GM.CombatHandler != null)
            G.GM.CombatHandler.OnEndBattle += Replace;
    }

    private EnemyType ResolveEnemy()
    {
        if (forceSpecificEnemy && specificEnemyType != null)
            return specificEnemyType;

        if (_boardManager == null || _boardManager.EncountersManager == null)
        {
            Debug.LogError("EnemyTile: BoardManager o EncountersManager es null.");
            return null;
        }

        return _boardManager.EncountersManager.GetNextEncounter();
    }

    public void Replace(bool win, int xp, int money)
    {
        _boardRaycast.HandleBoardRaycast(true);

        if (G.GM != null && G.GM.CombatHandler != null)
            G.GM.CombatHandler.OnEndBattle -= Replace;

        if (win)
        {
            _boardManager.ReplaceTileType(this, _postInteractionTile);
            _boardManager.IncrementEnemyDefeatedCount();
        }
    }

    private void OnDestroy()
    {
        if (G.GM != null && G.GM.CombatHandler != null)
            G.GM.CombatHandler.OnEndBattle -= Replace;
    }
}
