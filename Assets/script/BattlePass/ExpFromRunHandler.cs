using UnityEngine;

public class ExpFromRunHandler : MonoBehaviour
{
    [SerializeField]
    private BattlePassBarHandler battlePassBarHandler;

    private UIExplosionToTarget uiExplosionToTarget = null;

    public void Start()
    {
        uiExplosionToTarget = GetComponent<UIExplosionToTarget>();
        if (uiExplosionToTarget == null)
        {
            Debug.LogError("UIExplosionToTarget component not found on ExpFromRunHandler GameObject.");
            return;
        }

        
        if(GameManager.Instance.PlayerStats.ExperiencePoints > 0)
        {
            uiExplosionToTarget.TriggerEffect();
        }

    }

    public void AddEXPAmountToBattlePass()
    {
        
        battlePassBarHandler.UpdateBattlePass(GameManager.Instance.PlayerStats.ExperiencePoints);
        GameManager.Instance.PlayerStats.ResetLevelStats();
    }
}
