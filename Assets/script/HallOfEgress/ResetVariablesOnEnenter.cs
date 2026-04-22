using UnityEngine;

public class ResetVariablesOnEnenter : MonoBehaviour
{
    
    private void Start()
    {
        GameManager.Instance.PlayerStats.ResetStatsOnDeath();
        GameManager.Instance.ResetStageLevel();
    }

    
}
