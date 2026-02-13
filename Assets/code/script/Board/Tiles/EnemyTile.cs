using System.Collections;
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

        if (forceSpecificEnemy && specificEnemyType != null)
        {
            GameManager.Instance.SetForcedEnemy(specificEnemyType);
        }

        _boardRaycast.HandleBoardRaycast(false);
        SceneTransition.Instance.LoadScene("BattlePhone", true);
        GameManager.Instance.EnterBattle();
        GameManager.Instance.OnEndBattle += Replace;
    }

    public void Replace(bool win, int xp, int money) 
    {
        _boardRaycast.HandleBoardRaycast(true);
        if (win)
            _boardManager.ReplaceTileType(this, _postInteractionTile);
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnEndBattle -= Replace;
    }
}
