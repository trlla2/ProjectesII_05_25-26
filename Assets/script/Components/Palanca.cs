using UnityEngine;

public class Palanca : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Grid grid;
    [SerializeField] private AudioSource pullSound;

    [Header("Settings")]
    [SerializeField] private int cost = 4;
    [SerializeField] private int scrollTimes = 3;

    private bool isEnabled = true;

    private void Start()
    {
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void OnMouseDown()
    {
        TryPullLever();
    }

    private void TryPullLever()
    {
        if (!CanPullLever())
            return;

        if (!TryConsumeCost())
            return;

        ExecuteLeverPull();
    }

    private bool CanPullLever()
    {
        if (!isEnabled)
            return false;

        if (grid == null)
            return false;

        if (grid.CurrentState != GridStateType.Idle)
            return false;

        return true;
    }

    private bool TryConsumeCost()
    {
        return GameManager.Instance.PlayerStats.UseCurrency(CurrencyType.GOLD, cost);
    }

    private void ExecuteLeverPull()
    {
        if (pullSound != null)
        {
            pullSound.Play();
        }

        grid.Reroll(scrollTimes);
    }

    private void SubscribeToEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CombatHandler.OnEndBattle += HandleBattleEnd;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CombatHandler.OnEndBattle -= HandleBattleEnd;
        }
    }

    private void HandleBattleEnd(bool isWin, int exp, int g, Spell s)
    {
        SetEnabled(false);
    }

    public void SetEnabled(bool enabled)
    {
        isEnabled = enabled;
    }

    public void SetCost(int newCost)
    {
        cost = Mathf.Max(0, newCost);
    }

}