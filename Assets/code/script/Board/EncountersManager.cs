using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EncountersManager : MonoBehaviour
{
    [SerializeField] List<EnemyType> _enemyList;
    [SerializeField] List<EnemyType> _bossList;


    EnemyType GetRandomEnemyWithDifficulty(EnemyDifficulty difficulty)
    {
        List<EnemyType> enemyList = GetEncontersInStage(difficulty, _enemyList);

        return enemyList.ElementAt(Random.Range(0, enemyList.Count));
    }

    EnemyType GetRandomBossFromLevel(EnemyDifficulty difficulty)
    {
        List<EnemyType> enemyList = GetEncontersInStage(difficulty, _bossList);

        return enemyList.ElementAt(Random.Range(0, enemyList.Count));
    }

    List<EnemyType> GetEncontersInStage(EnemyDifficulty difficulty, List<EnemyType> encounterList)
    {
        List<EnemyType> enemiesInStage = new List<EnemyType>();

        foreach (EnemyType e in encounterList)
        {
            if (e.Difficulty == difficulty)
                enemiesInStage.Add(e);
        }

        return enemiesInStage;
    }

    public void LoadEnemyEncounters(List<EnemyType> encounterList)
    {
        _enemyList = new List<EnemyType>(encounterList);
    }

}
