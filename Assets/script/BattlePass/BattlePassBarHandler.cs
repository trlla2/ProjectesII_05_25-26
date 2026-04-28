using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattlePassBarHandler : MonoBehaviour
{
    [Header("SETUP")]
    [SerializeField] private Slider battlePassBar;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private Image rewardImage;
    [SerializeField] private GameObject rewardBG;

    [Header("BattlePass")]
    [SerializeField] private BattlePassType battlePassType;

    [Header("Animation")]
    [SerializeField] private float interpolationDuration = 1.0f;
    [SerializeField] private AnimationCurve interpolationCurve;

    [SerializeField] private Image rewardClone;

    private float interpolationTime = 0.0f;
    private bool isInterpolating = false;

    private float startPercentage;
    private float targetPercentage;
    private int pendingExp;

    private void Start()
    {
        battlePassBar.minValue = 0f;
        battlePassBar.maxValue = 1f;

        UpdateVisuals();
    }

    public void UpdateBattlePass(int amount)
    {
        pendingExp += amount;

        if (!isInterpolating)
        {
            StartNextInterpolationCycle();
        }
    }

    private void StartNextInterpolationCycle()
    {
        if (pendingExp <= 0) return;

        var playerStats = GameManager.Instance.PlayerStats;
        int level = playerStats.BattlePassLevel;

        if (level >= battlePassType.Rewards.Count) return;

        int expCost = battlePassType.Rewards[level].expCost;
        int currentExp = playerStats.GetCurrency(CurrencyType.EXP);

        startPercentage = (float)currentExp / expCost;

        if (currentExp + pendingExp >= expCost)
        {
            targetPercentage = 1.0f;
        }
        else
        {
            targetPercentage = (float)(currentExp + pendingExp) / expCost;
        }

        interpolationTime = 0.0f;
        isInterpolating = true;
    }

    private void HandleInterpolation()
    {
        if (!isInterpolating) return;

        interpolationTime += Time.deltaTime;

        float t = Mathf.Clamp01(interpolationTime / interpolationDuration);
        float curveValue = interpolationCurve.Evaluate(t);

        battlePassBar.value = Mathf.Lerp(startPercentage, targetPercentage, curveValue);

        if (t >= 1f)
        {
            isInterpolating = false;

            var playerStats = GameManager.Instance.PlayerStats;
            int level = playerStats.BattlePassLevel;
            int expCost = battlePassType.Rewards[level].expCost;
            int currentExp = playerStats.GetCurrency(CurrencyType.EXP);

            int expNeededToLevelUp = expCost - currentExp;

            if (pendingExp >= expNeededToLevelUp)
            {
                playerStats.AddCurrency(CurrencyType.EXP, expNeededToLevelUp);
                pendingExp -= expNeededToLevelUp;
                ClaimRewards();

                if (pendingExp > 0)
                {
                    StartNextInterpolationCycle();
                }
            }
            else
            {
                playerStats.AddCurrency(CurrencyType.EXP, pendingExp);
                pendingExp = 0;
            }
        }
    }

    private void ClaimRewards()
    {
        var playerStats = GameManager.Instance.PlayerStats;
        int level = playerStats.BattlePassLevel;
        var cost = battlePassType.Rewards[level].expCost;

        if (!playerStats.UseCurrency(CurrencyType.EXP, cost)) return;
        rewardClone.sprite = battlePassType.Rewards[level].RewardDrop.Icon;

        if (rewardClone.IsActive())
        {
            rewardClone.GetComponent<TranslatorUI>().ResetTranslator();
            rewardClone.GetComponent<CanvasGroupFader>().ResetToOrigin();

            rewardClone.GetComponent<TranslatorUI>().ToTarget();
        }
        else
        {
            rewardClone.gameObject.SetActive(true);
        }
            
        battlePassType.Rewards[level].ClaimReward();
        playerStats.AddBattlePassLevel();

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        var playerStats = GameManager.Instance.PlayerStats;
        int level = playerStats.BattlePassLevel;

        if (level >= battlePassType.Rewards.Count) return;

        var reward = battlePassType.Rewards[level];

        rewardImage.sprite = reward.RewardDrop.Icon;
        rewardText.text = reward.quantity > 1 ? "x" + reward.quantity : "";

        if (reward.RewardDrop.Type == RewardType.SKIN || reward.RewardDrop.Type == RewardType.SPELL)
        {
            rewardBG.SetActive(true);
        }

        if (!isInterpolating)
        {
            int expCost = reward.expCost;
            int currentExp = playerStats.GetCurrency(CurrencyType.EXP);

            battlePassBar.value = Mathf.Clamp01(currentExp / expCost);
        }
    }

    private void Update()
    {
        HandleInterpolation();
    }
}