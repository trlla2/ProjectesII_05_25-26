using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattlePassBarHandler : MonoBehaviour
{
    [Header("SETUP")]
    [SerializeField]
    private Slider battlePassBar;
    [SerializeField]
    private TMP_Text rewardText;
    [SerializeField]
    private Image rewardImage;
    [SerializeField]
    private GameObject rewardBG;

    [Header("BattlePas")]
    [SerializeField]
    private BattlePassType battlePassType;

    [Header("Animation")]
    [SerializeField]
    private float interpolationDuration = 1.0f;
    [SerializeField]
    private AnimationCurve interpolationCurve;

    private float interpolationTime = 0.0f;
    private bool isInterpolating = false;

    private float startValue;
    private float targetValue;
    private int newAmount;

    private void Start()
    {
        UpdateVisuals();

        UpdateBattlePass(100);
    }

    public void UpdateBattlePass(int amount)
    {
        newAmount = amount;
        UpdateBar();
    }

    private void ClaimRewards()
    {
        var playerStats = GameManager.Instance.PlayerStats;
        int level = playerStats.BattlePassLevel;
        var cost = battlePassType.Rewards[level].expCost;

        if (!playerStats.UseCurrency(CurrencyType.EXP, cost)) return;

        battlePassType.Rewards[level].ClaimReward();
        playerStats.AddBattlePassLevel();

        UpdateVisuals();
    }
    
    private void UpdateVisuals()
    {
        var playerStats = GameManager.Instance.PlayerStats;
        int level = playerStats.BattlePassLevel;
        var reward = battlePassType.Rewards[level];

        rewardImage.sprite = reward.RewardDrop.Icon;
        rewardText.text = reward.quantity > 1 ? "x" + reward.quantity : "";

        if (reward.RewardDrop.Type == RewardType.SKIN || reward.RewardDrop.Type == RewardType.SPELL)
        {
            rewardBG.SetActive(true);
        }

        UpdateBar();
    }

    private void UpdateBar()
    {
        startValue = battlePassBar.value;
        targetValue = GameManager.Instance.PlayerStats.GetCurrency(CurrencyType.EXP) + newAmount;

        interpolationTime = 0.0f;
        isInterpolating = true;
    }

    private void HandleInterpolation()
    {
        if (!isInterpolating) return;

        interpolationTime += Time.deltaTime;
        
        float curveValue = interpolationCurve.Evaluate(interpolationTime / interpolationDuration);

        battlePassBar.value = Mathf.Lerp(startValue, targetValue, curveValue);

        if (interpolationTime >= interpolationDuration)
        {
            isInterpolating = false;

            if (newAmount > 0)
            {
                GameManager.Instance.PlayerStats.AddCurrency(CurrencyType.EXP, newAmount);
                newAmount = 0;
            }

            battlePassBar.value = GameManager.Instance.PlayerStats.GetCurrency(CurrencyType.EXP);
            ClaimRewards();
        }
    }

    private void Update()
    {
        HandleInterpolation();
    }
}