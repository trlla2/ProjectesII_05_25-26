using UnityEngine;

public class ResetVariablesOnEnenter : MonoBehaviour
{
    private void Awake()
    {
        PlayerStats stats = GameManager.Instance.PlayerStats;

        stats.SetCurrency(CurrencyType.GOLD, 50);

        if (!stats.IsAlive())
        {
            stats.ResetStatsOnDeath();
            GameManager.Instance.ResetStageLevel();
        }
        else
        {
            stats.ResetHpToMax();
        }
    }
}
