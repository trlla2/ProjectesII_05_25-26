using UnityEngine;

public class EnemyTile : BoardTile
{
    [SerializeField] protected string _combatSceneName;
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
                return;
            }

            GameManager.Instance.SetPendingEnemy(enemyToFight);
        }

        _boardRaycast.HandleBoardRaycast(false);
        SceneTransition.Instance.LoadScene(_combatSceneName, true);
        GameManager.Instance.EnterBattle();

        GameManager.Instance.OnEnterBoard += Replace;
        GameManager.Instance.OnEnterBoard += TryToRevealGeneratedBranchIfHasOne;
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

        ReplaceWithPostInteraction();
        _boardManager.IncrementEnemyDefeatedCount();
    }

    public void ReplaceWhenWin(bool win, int xp, int g, Spell drop)
    {
        _boardRaycast.HandleBoardRaycast(true);

        if (GameManager.Instance != null && GameManager.Instance.CombatHandler != null)
            GameManager.Instance.CombatHandler.OnEndBattle -= ReplaceWhenWin;

        if (win)
        {
            ReplaceWithPostInteraction();
            _boardManager.IncrementEnemyDefeatedCount();
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Instance.CombatHandler != null)
            GameManager.Instance.OnEnterBoard -= Replace;
    }
}
