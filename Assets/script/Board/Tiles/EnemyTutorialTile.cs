using UnityEngine;

public class EnemyTutorialTile : BoardTile
{
    [SerializeField] private BoardTileData _postInteractionTile;

    [SerializeField] private int lootRatePercentage;

    [SerializeField] private string enemyScene;

    private TileClickHandler _boardRaycast;

    private void Start()
    {
        _boardRaycast = Camera.main.GetComponent<TileClickHandler>();
    }

    public override void Interact()
    {
        Debug.Log("starting combat...");

        _boardRaycast.HandleBoardRaycast(false);
        SceneTransition.Instance.LoadScene(enemyScene, true);
        GameManager.Instance.EnterBattle();
        GameManager.Instance.CombatHandler.OnEndBattle += Replace;
    }

    public void Replace(bool win, int xp, int g, Spell drop)
    {
        _boardRaycast.HandleBoardRaycast(true);
        if (win)
            _boardManager.ReplaceTileType(this, _postInteractionTile);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Instance.CombatHandler != null)
            GameManager.Instance.CombatHandler.OnEndBattle -= Replace;
    }
}
