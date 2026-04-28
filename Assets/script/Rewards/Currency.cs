using UnityEngine;

public enum CurrencyType { GOLD, RUBY, EXP }

[CreateAssetMenu(fileName = "NewCurrencyType", menuName = "Game/Currency")]
public class Currency : ScriptableObject, IDropeable
{
    [Header("Data")]
    [SerializeField] private string currencyName;
    [SerializeField] private Sprite icon;
    [SerializeField] private CurrencyType currencyType;
    [SerializeField] private Color color;


    public string RewardName => currencyName;
    public Sprite Icon => icon;
    public RewardType Type => RewardType.CURRENCY;
    public CurrencyType CurrencyType => currencyType;   
    public Color Color => color;

    public void ClaimReward()
    {
        PlayerStats playerStats = GameManager.Instance.PlayerStats;
        playerStats.AddCurrency(currencyType, 1);
    }
}
