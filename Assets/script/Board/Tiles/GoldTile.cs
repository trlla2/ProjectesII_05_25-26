using UnityEngine;

public class GoldTile : BoardTile
{
    private int _minGoldAmount = 1;
    [SerializeField] private int _maxGoldAmount;
    public override void Interact()
    {
        GameManager.Instance.PlayerStats.AddMoney(Random.Range(_minGoldAmount, _maxGoldAmount));
        ReplaceWithPostInteraction();
    }

}
