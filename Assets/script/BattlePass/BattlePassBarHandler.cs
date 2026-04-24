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
        if (!GameManager.Instance.PlayerStats.CanUseCurrency(CurrencyType.EXP, newAmount))
            return;

        battlePassType.Rewards[GameManager.Instance.PlayerStats.BattlePassLevel].ClaimReward();

        GameManager.Instance.PlayerStats.AddBattlePassLevel();

        UpdateVisuals();
    }
    
    private void UpdateVisuals()
    {
        rewardImage.sprite = battlePassType.Rewards[GameManager.Instance.PlayerStats.BattlePassLevel].reward.Icon;
        
        if(battlePassType.Rewards[GameManager.Instance.PlayerStats.BattlePassLevel].quantity > 1)
            rewardText.text = "x" + battlePassType.Rewards[GameManager.Instance.PlayerStats.BattlePassLevel].quantity;
        else
            rewardText.text = "";

        if(battlePassType.Rewards[GameManager.Instance.PlayerStats.BattlePassLevel].reward.Type == RewardType.SKIN ||
            battlePassType.Rewards[GameManager.Instance.PlayerStats.BattlePassLevel].reward.Type == RewardType.SPELL)
            rewardBG.SetActive(true);

        UpdateBar();
    }

    private void UpdateBar()
    {
        interpolationTime = 0.0f;
        isInterpolating = true;

    }

    private void HandleInterpolation()
    {
        if (isInterpolating)
        {
            interpolationTime += Time.deltaTime;
            float t = Mathf.Clamp01(interpolationTime / interpolationDuration);
            t = interpolationCurve.Evaluate(t);

            battlePassBar.value = Mathf.Lerp(battlePassBar.value, GameManager.Instance.PlayerStats.GetCurrency(CurrencyType.EXP), t);
            if (t >= 1.0f)
            {
                interpolationTime = 0.0f;
                isInterpolating = false;
                battlePassBar.value = GameManager.Instance.PlayerStats.GetCurrency(CurrencyType.EXP);
                ClaimRewards();
            }
        }
    }

    private void Update()
    {
        HandleInterpolation();
    }
}