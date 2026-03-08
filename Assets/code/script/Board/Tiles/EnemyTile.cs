using UnityEngine;

public class EnemyTile : BoardTile
{
    [SerializeField] private string _combatSceneName;
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

        if (!_boardManager.EncountersManager.IsForced())
        {
            EnemyType enemyToFight = ResolveEnemy();
            if (enemyToFight == null)
            {
                Debug.LogError("EnemyTile: ResolveEnemy() null, no se inicia combate.");
                return;
            }

            GameManager.Instance.SetPendingEnemy(enemyToFight);
        }

        _boardRaycast.HandleBoardRaycast(false);
        SceneTransition.Instance.LoadScene(_combatSceneName, true);
        GameManager.Instance.EnterBattle();

        GameManager.Instance.OnEnterBoard += Replace;
        GameManager.Instance.OnEnterBoard += TryToRevealGeneratedBranchIfHasOne;
        //if (GameManager.Instance.CombatHandler != null)
        //    GameManager.Instance.CombatHandler.OnEndBattle += Replace;
        //TryToRevealGeneratedBranchIfHasOne();

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

        //if (GameManager.Instance. != null &&GameManager.Instance.CombatHandler != null)
        //    GameManager.Instance.CombatHandler.OnEndBattle -= Replace;

        _boardManager.ReplaceTileType(this, _postInteractionTile);
        _boardManager.IncrementEnemyDefeatedCount();
    }

    public void ReplaceWhenWin(bool win, int xp, int money)
    {
        _boardRaycast.HandleBoardRaycast(true);

        if (GameManager.Instance != null && GameManager.Instance.CombatHandler != null)
            GameManager.Instance.CombatHandler.OnEndBattle -= ReplaceWhenWin;

        if (win)
        {
            _boardManager.ReplaceTileType(this, _postInteractionTile);
            _boardManager.IncrementEnemyDefeatedCount();
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Instance.CombatHandler != null)
            GameManager.Instance.OnEnterBoard -= Replace;
    }
}
