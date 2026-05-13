using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

public class BattlePassBarHandler : MonoBehaviour
{
    [Header("SETUP")]
    [SerializeField] private Slider battlePassBar;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private Image rewardImage;
    [SerializeField] private GameObject rewardBG;
    [SerializeField] private Sprite spellBG;

    [Header("BattlePass")]
    [SerializeField] private BattlePassType battlePassType;

    [Header("Animation")]
    [SerializeField] private float interpolationDuration = 1.0f;
    [SerializeField] private AnimationCurve interpolationCurve;

    [SerializeField] private GameObject rewardClone;

    private float interpolationTime = 0.0f;
    private bool isInterpolating = false;

    private float startPercentage;
    private float targetPercentage;
    private int pendingExp;

    private float baseWidth;
    private float spellWidth = 124.0f;
    private void Start()
    {
        battlePassBar.minValue = 0f;
        battlePassBar.maxValue = 1f;

        baseWidth = rewardImage.rectTransform.sizeDelta.x;

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

        Image tmp = rewardClone.GetComponentInChildren<Image>();
        rewardImage.rectTransform.sizeDelta = new Vector2(baseWidth, rewardImage.rectTransform.sizeDelta.y);

        if (tmp != null && battlePassType.Rewards[level].RewardDrop.Icon != null)
        {
            tmp.sprite = battlePassType.Rewards[level].RewardDrop.Icon;

            foreach (Transform child in tmp.transform)
            {
                Destroy(child.gameObject);
            }
        }
        else if (tmp != null && battlePassType.Rewards[level].RewardDrop.sprites != null && battlePassType.Rewards[level].RewardDrop.sprites.Length > 0)
        {
            tmp.sprite = spellBG;

            var sprites = battlePassType.Rewards[level].RewardDrop.sprites;
            var colors = battlePassType.Rewards[level].RewardDrop.Colors;
            int count = Mathf.Min(sprites.Length, colors != null ? colors.Length : 0);

            for (int i = 0; i < count; i++)
            {
                GameObject tmpObj = Instantiate(new GameObject(), tmp.transform);
                Image img = tmpObj.AddComponent<Image>();
                img.sprite = sprites[i];
                img.color = colors[i];
                img.rectTransform.sizeDelta = new Vector2(25, 25);
                rewardImage.rectTransform.sizeDelta = new Vector2(spellWidth, rewardImage.rectTransform.sizeDelta.y);
            }
        }
        else
        {
            Debug.LogError("Reward has no valid sprite: " + battlePassType.Rewards[level].RewardDrop.RewardName);
            rewardImage.sprite = null;
        }

        TMP_Text tmpText = rewardClone.GetComponentInChildren<TMP_Text>();
        if (battlePassType.Rewards[level].quantity > 1)
        {
            tmpText.text = "x" + battlePassType.Rewards[level].quantity;
        }
        else
        {
            tmpText.text = "";
        }

        if (rewardClone.activeSelf)
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
        if(reward.quantity > 1)
        {
            rewardText.text = "x" + reward.quantity;
        }
        else
        {
            rewardText.text = "";
        }
        rewardImage.rectTransform.sizeDelta = new Vector2(baseWidth, rewardImage.rectTransform.sizeDelta.y);
        if (reward.RewardDrop.Icon != null)
        {
            rewardImage.sprite = reward.RewardDrop.Icon;

            foreach (Transform child in rewardImage.transform)
            {
                Destroy(child.gameObject);
            }
        }
        else if (reward.RewardDrop.sprites != null && reward.RewardDrop.sprites.Length > 0)
        {
            rewardImage.sprite = spellBG;

            var sprites = reward.RewardDrop.sprites;
            var colors = reward.RewardDrop.Colors;
            int count = Mathf.Min(sprites.Length, colors != null ? colors.Length : 0);

            for (int i = 0; i < count; i++)
            {
                GameObject tmp = Instantiate(new GameObject(), rewardImage.transform);
                Image img = tmp.AddComponent<Image>();
                img.sprite = sprites[i];
                img.color = colors[i];
                img.rectTransform.sizeDelta = new Vector2(25, 25);
                rewardImage.rectTransform.sizeDelta = new Vector2(spellWidth, rewardImage.rectTransform.sizeDelta.y);
            }
        }
        else
        {
            Debug.LogError("Reward has no valid sprite: " + reward.RewardDrop.RewardName);
            rewardImage.sprite = null;
        }


        rewardBG.SetActive(reward.RewardDrop.Type == RewardType.SKIN || reward.RewardDrop.Type == RewardType.SPELL);

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