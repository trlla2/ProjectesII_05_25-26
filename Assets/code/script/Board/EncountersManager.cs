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
        if (_enemyList == null)
            _enemyList = new List<EnemyType>();

        if (_avaliableEnemies == null)
            _avaliableEnemies = new List<EnemyType>(_enemyList);
    }

    public EnemyType GetNextEncounter()
    {
        /*if (_enemyList == null) */
        InitializeData();

        if (_encountersOrder == null || _encountersOrder.Count == 0)
        {
            Debug.LogError("EncountersManager: encountersOrder está vacío o es null.");
            return null;
        }

        if (_lastEncounterIndex >= _encountersOrder.Count)
        {
            Debug.LogWarning("EncountersManager: se han consumido todos los encuentros definidos. Repitiendo el último índice.");
            _lastEncounterIndex = _encountersOrder.Count - 1;
        }

        EnemyDifficulty targetDifficulty = _encountersOrder[_lastEncounterIndex];

        List<EnemyType> candidates = _avaliableEnemies
            .Where(e => e != null && e.Difficulty == targetDifficulty)
            .ToList();

        Debug.Log($"[Encounter] idx={_lastEncounterIndex}, diff={targetDifficulty}, candidates={candidates.Count}, names={string.Join(", ", candidates.Select(c => c.Name))}");

        if (candidates.Count == 0)
        {
            Debug.LogError($"EncountersManager: no hay enemigos con dificultad {targetDifficulty} en _enemyList del StageLevelData.");
            return null;
        }

        int i = Random.Range(0, candidates.Count);
        EnemyType selectedEnemy = candidates[i];

        // eliminar de _avaliableEnemies, no de la copia local
        _avaliableEnemies.Remove(selectedEnemy);

        // si ya no quedan de esa dificultad, repoblar desde _enemyList
        if (!_avaliableEnemies.Any(e => e != null && e.Difficulty == targetDifficulty))
        {
            foreach (EnemyType e in _enemyList)
            {
                if (e != null && e.Difficulty == targetDifficulty && !_avaliableEnemies.Contains(e))
                    _avaliableEnemies.Add(e);
            }
        }

        _lastEncounterIndex++;
        return selectedEnemy;
    }

    public EnemyType GetBossEncounter()
    {
        if (_bossList == null || _bossList.Count == 0)
        {
            Debug.LogError("EncountersManager: _bossList está vacío o es null.");
            return null;
        }
        int i = Random.Range(0, _bossList.Count);
        return _bossList[i];
    }

    public void LoadEnemyEncounters(List<EnemyType> encounterList)
    {
        //if (encounterList.Count <= 0) return;
        //if (_enemyList.Count > 0) return;
        _enemyList = new List<EnemyType>(encounterList);
        _avaliableEnemies = new List<EnemyType>(encounterList);
        _lastEncounterIndex = 0;
    }
    public void LoadEnemyEncountersOrder(List<EnemyDifficulty> encounters)
    {
        _encountersOrder = new List<EnemyDifficulty>(encounters);
        _lastEncounterIndex = 0;
    }

    public void LoadBossesEncounters(List<EnemyType> encounterList)
    {
        //if (encounterList.Count <= 0) return;
        //if (_bossList.Count > 0) return;
        _bossList = new List<EnemyType>(encounterList);
    }

}
