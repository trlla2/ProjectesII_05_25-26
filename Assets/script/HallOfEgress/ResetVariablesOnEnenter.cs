using UnityEngine;

public class ResetVariablesOnEnenter : MonoBehaviour
{
    
    private void Awake()
    {
        if (!GameManager.Instance.PlayerStats.IsAlive())
        {
            Debug.Log("Player is dead, resetting stats and stage level.");
            GameManager.Instance.PlayerStats.ResetStatsOnDeath();
            GameManager.Instance.ResetStageLevel();
        }
    }

    
}
