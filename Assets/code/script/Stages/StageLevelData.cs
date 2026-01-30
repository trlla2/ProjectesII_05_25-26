using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStageLevel", menuName = "Game/Stage Level")]
public class StageLevelData : MonoBehaviour
{
    [SerializeField]
    private List<EnemyType> enemies;

    private List<EnemyType> avadibleEnemies;

    private void InitializeData()
    {
        avadibleEnemies = enemies;

    }

    public EnemyType GetEnemy(EnemyDifficulty enemyDifficulty)
    {
        if(avadibleEnemies  == null) InitializeData();

        EnemyType selectedEnemy;

        List<EnemyType> candidates = avadibleEnemies.Where(e => e.Difficulty == enemyDifficulty).ToList();

        if(candidates.Count() <= 0 )
        {
            Debug.LogError($"NO ENEMIES AVADIBLE OF DIFFICULTY LEVEL: {enemyDifficulty}");
            return null;
        }


        int i = Random.Range(0, candidates.Count);

        selectedEnemy = candidates[i];

        candidates.Remove(selectedEnemy);

        if(candidates.Count() <= 0) 
        {
            foreach(EnemyType e in enemies)
            {
                if(e.Difficulty == enemyDifficulty)
                    avadibleEnemies.Add(e);
            }
        }

        return selectedEnemy;
    }
}
