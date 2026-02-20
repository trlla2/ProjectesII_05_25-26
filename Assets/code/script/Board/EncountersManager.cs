using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EncountersManager : MonoBehaviour
{
    [SerializeField] List<EnemyType> _enemyList;
    private List<EnemyType> _avaliableEnemies;
    [SerializeField] List<EnemyType> _bossList;

    private List<EnemyDifficulty> _encountersOrder;
    private int _lastEncounterIndex = 0;

    private void InitializeData()
    {
        _avaliableEnemies = _enemyList;
    }

    public EnemyType GetNextEncounter()
    {
        if (_enemyList == null) InitializeData();

        EnemyType selectedEnemy;

        List<EnemyType> candidates = _avaliableEnemies.Where(e => e.Difficulty == _encountersOrder[_lastEncounterIndex]).ToList();

        if (candidates.Count() <= 0)
        {
            Debug.LogError($"NO ENEMIES AVADIBLE OF DIFFICULTY LEVEL: {_encountersOrder[_lastEncounterIndex]}");
            return null;
        }


        int i = Random.Range(0, candidates.Count);

        selectedEnemy = candidates[i];

        candidates.Remove(selectedEnemy);

        if (candidates.Count() <= 0)
        {
            foreach (EnemyType e in _enemyList)
            {
                if (e.Difficulty == _encountersOrder[_lastEncounterIndex])
                    _avaliableEnemies.Add(e);
            }
        }

        _lastEncounterIndex++;
        return selectedEnemy;
    }

    public EnemyType GetEncounterByDifficulty(EnemyDifficulty enemyDifficulty)
    {
        if (_avaliableEnemies == null) InitializeData();

        EnemyType selectedEnemy;

        List<EnemyType> candidates = _avaliableEnemies.Where(e => e.Difficulty == enemyDifficulty).ToList();

        if (candidates.Count() <= 0)
        {
            Debug.LogError($"NO ENEMIES AVADIBLE OF DIFFICULTY LEVEL: {enemyDifficulty}");
            return null;
        }


        int i = Random.Range(0, candidates.Count);

        selectedEnemy = candidates[i];

        candidates.Remove(selectedEnemy);

        if (candidates.Count() <= 0)
        {
            foreach (EnemyType e in _enemyList)
            {
                if (e.Difficulty == enemyDifficulty)
                    _avaliableEnemies.Add(e);
            }
        }

        return selectedEnemy;
    }

    // =================


    //EnemyType GetRandomEnemyWithDifficulty(EnemyDifficulty difficulty)
    //{
    //    List<EnemyType> enemyList = GetEncontersInStage(difficulty, _enemyList);

    //    return enemyList.ElementAt(Random.Range(0, enemyList.Count));
    //}

    //EnemyType GetRandomBossFromLevel(EnemyDifficulty difficulty)
    //{
    //    List<EnemyType> enemyList = GetEncontersInStage(difficulty, _bossList);

    //    return enemyList.ElementAt(Random.Range(0, enemyList.Count));
    //}

    //List<EnemyType> GetEncontersInStage(EnemyDifficulty difficulty, List<EnemyType> encounterList)
    //{
    //    List<EnemyType> enemiesInStage = new List<EnemyType>();

    //    foreach (EnemyType e in encounterList)
    //    {
    //        if (e.Difficulty == difficulty)
    //            enemiesInStage.Add(e);
    //    }

    //    return enemiesInStage;
    //}

    public void LoadEnemyEncounters(List<EnemyType> encounterList)
    {
        //if (encounterList.Count <= 0) return;
        //if (_enemyList.Count > 0) return;
        _enemyList = new List<EnemyType>(encounterList);
        _avaliableEnemies = new List<EnemyType>(encounterList);
    }
    public void LoadEnemyEncountersOrder(List<EnemyDifficulty> encounters)
    {
        _encountersOrder = new List<EnemyDifficulty>(encounters);
    }

    public void LoadBossesEncounters(List<EnemyType> encounterList)
    {
        //if (encounterList.Count <= 0) return;
        //if (_bossList.Count > 0) return;
        _bossList = new List<EnemyType>(encounterList);
    }

}
