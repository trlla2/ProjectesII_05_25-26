using System.Collections;
using UnityEngine;

public class OnBoardingTrigger_Enemies : MonoBehaviour
{
    [SerializeField] private EnemyBehaviour activeEnemy;
    private bool _isTriggered = false;
    private bool _isChecking = false;
    private bool _hasWaited = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(CheckIfEnemyHasToBeExplained());


        StartCoroutine(CheckIfPlayerHasUsableItems());
        
    }

    private IEnumerator CheckIfPlayerHasUsableItems()
    {
        if (_isChecking) yield return new WaitUntil(() => !_isChecking);
        _isChecking = true;
        if (_isTriggered) yield return new WaitUntil(() => !_isTriggered);
        yield return new WaitForSeconds(1f);
        _isTriggered = true;
        _isChecking = false;
        
        foreach (Item item in GameManager.Instance.PlayerStats.Inventory)
        {
            if (item.IsActivatable)
            {
                OnBoardingManager.Instance.TriggerIfNew(OnBoardingKey.USABLE_ITEMS);
                break;
            }
        }
        _isTriggered = false;
    }

    private IEnumerator CheckIfEnemyHasToBeExplained()
    {
        if (_isChecking) yield return new WaitUntil(() => !_isChecking);
        _isChecking = true;
        if (_isTriggered) yield return new WaitUntil(() => !_isTriggered);
        yield return new WaitForSeconds(1f);
        _isTriggered = true;
        _isChecking = false;
        
        if (activeEnemy != null)
        {
            switch (activeEnemy.GetEnemyType().SpellType)
            {
                case EnemySpellType.BombAttack:
                    OnBoardingManager.Instance.TriggerIfNew(OnBoardingKey.BOMB_ENEMY);
                    break;
                case EnemySpellType.HiddenDestroy:
                    OnBoardingManager.Instance.TriggerIfNew(OnBoardingKey.HIDDEN_ENEMY);
                    break;
                default:
                    break;
            }
        }
        _isTriggered = false;
    }

    public void TriggerLevelUpWithDelay()
    {
        OnBoardingManager.Instance.TriggerIfNewWithDelay(OnBoardingKey.LVL_UP_ELEMENTS);
    }

}
