using UnityEngine;

public class EnemyTile : BoardTile
{
    [SerializeField] private BoardTileData _postInteractionTile;

    [SerializeField] private int lootRatePercentage;

    public override void Interact()
    {
        Debug.Log("starting combat...");

        
        BoardManager.instance.ReplaceTileType(this, _postInteractionTile);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

}
