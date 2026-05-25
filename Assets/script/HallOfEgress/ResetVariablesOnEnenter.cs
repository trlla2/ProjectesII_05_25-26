using UnityEngine;

public class ResetVariablesOnEnenter : MonoBehaviour
{
    
    private void Awake()
    {
        if (!GameManager.Instance.PlayerStats.IsAlive())
        {
            GameManager.Instance.PlayerStats.ResetStatsOnDeath();
            GameManager.Instance.ResetStageLevel();
        }
    }

    
}
