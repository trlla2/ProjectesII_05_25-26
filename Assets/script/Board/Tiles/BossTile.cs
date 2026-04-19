using UnityEngine;

public class BossTile : EnemyTile
{
    protected override EnemyType ResolveEnemy()
    {

        if(GameManager.Instance.StageLevel == 1)
        {
            _combatSceneName = "BattlePhoneBossSTAGE1";
        }
        else if(GameManager.Instance.StageLevel == 2)
        {
            _combatSceneName = "BattlePhoneBossSTAGE2";
        }
        else if(GameManager.Instance.StageLevel == 3)
        {
            Debug.Log("Aun no");
        }
        if (_boardManager == null || _boardManager.EncountersManager == null)
        {
            Debug.LogError("EnemyTile: BoardManager o EncountersManager es null.");
            return null;
        }

        return _boardManager.EncountersManager.GetBossEncounter();
    }
}
