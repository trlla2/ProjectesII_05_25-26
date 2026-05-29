using UnityEngine;

public class ResetVariablesOnEnenter : MonoBehaviour
{
    private void Awake()
    {
        PlayerStats stats = GameManager.Instance.PlayerStats;

        // Always save elemental progress, reset hp and gold on HallOfEgress entry
        stats.SaveElementalProgress();
        stats.SetCurrency(CurrencyType.GOLD, 50);

        if (!stats.IsAlive())
        {
            // ResetStatsOnDeath resets hp to base max and restores saved elemental progress
            stats.ResetStatsOnDeath();
            GameManager.Instance.ResetStageLevel();
        }
        else
        {
            stats.ResetHpToMax();
        }
    }
}
