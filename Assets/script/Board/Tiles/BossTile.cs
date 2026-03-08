using UnityEngine;

public class BossTile : EnemyTile
{
    protected override EnemyType ResolveEnemy()
    {
        if (_boardManager == null || _boardManager.EncountersManager == null)
        {
            Debug.LogError("EnemyTile: BoardManager o EncountersManager es null.");
            return null;
        }

        return _boardManager.EncountersManager.GetBossEncounter();
    }
}
