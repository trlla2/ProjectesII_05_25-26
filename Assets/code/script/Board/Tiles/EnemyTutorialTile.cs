using UnityEngine;

public class EnemyTutorialTile : BoardTile
{
    [SerializeField] private BoardTileData _postInteractionTile;

    [SerializeField] private int lootRatePercentage;

    [SerializeField] private string enemyScene;
    [SerializeField] private EnemyType specificEnemyType;
    private TileClickHandler _boardRaycast;

    private void Start()
    {
        _boardRaycast = Camera.main.GetComponent<TileClickHandler>();
    }

    public override void Interact()
    {
        Debug.Log("starting combat...");
        GameManager.Instance.SetForcedEnemy(specificEnemyType);
        _boardRaycast.HandleBoardRaycast(false);
        SceneTransition.Instance.LoadScene(enemyScene, true);
        GameManager.Instance.EnterBattle();
        GameManager.Instance.OnEndBattle += Replace;
    }

    public void Replace(bool win, int xp, int money)
    {
        _boardRaycast.HandleBoardRaycast(true);
        if (win)
            BoardManager.instance.ReplaceTileType(this, _postInteractionTile);
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnEndBattle -= Replace;
    }
}
