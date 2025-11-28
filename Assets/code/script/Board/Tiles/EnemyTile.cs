using System.Collections;
using UnityEngine;

public class EnemyTile : BoardTile
{
    [SerializeField] private BoardTileData _postInteractionTile;

    [SerializeField] private int lootRatePercentage;

    public override void Interact()
    {
        Debug.Log("starting combat...");

        
        SceneTransition.Instance.LoadScene("BattlePhone", true);
        GameManager.Instance.OnEndBattle += Replace;
    }

    public void Replace(bool win, int xp, int money) // FORCED CHANGE!!!!!!
    {
        if (win)
            BoardManager.instance.ReplaceTileType(this, _postInteractionTile);
        else
        {
            //MUERTE
        }
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnEndBattle -= Replace;
    }
}
