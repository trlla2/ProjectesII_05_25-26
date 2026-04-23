using UnityEngine;

public class GoldTile : BoardTile
{
    private int _minGoldAmount = 1;
    [SerializeField] private int _maxGoldAmount;
    [SerializeField] private Transform _coinTransform;
    [SerializeField] private GameObject _coinClone;
    public override void Interact()
    {
        GameManager.Instance.PlayerStats.AddCurrency(CurrencyType.GOLD, Random.Range(_minGoldAmount, _maxGoldAmount));
        ReplaceWithPostInteraction();

        Instantiate(_coinClone, _coinTransform.position, _coinTransform.rotation, null);
    }

}
