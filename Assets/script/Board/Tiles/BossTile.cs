using UnityEngine;

public class BossTile : EnemyTile
{
    protected override EnemyType ResolveEnemy()
    {
        int bossIndex = GameManager.Instance.GetBossSceneIndex();
        _combatSceneName = $"BattlePhoneBossSTAGE{bossIndex}";

        if (_boardManager == null || _boardManager.EncountersManager == null)
        {
            Debug.LogError("EnemyTile: BoardManager o EncountersManager es null.");
            return null;
        }

        return _boardManager.EncountersManager.GetBossEncounter();
    }
}
