using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStageLevelData", menuName = "Game/Stage Level Data")]
public class StageLevelData : ScriptableObject
{
    [SerializeField]
    private List<EnemyType> posibleEncounters;

    private List<EnemyType> avadibleEncounter;

    [SerializeField]
    private List<EnemyDifficulty> encounters;

    private int lastEncounterIndex = 0;

    [SerializeField]
    private List<BoardTile> interactableTiles;

    private void InitializeData()
    {
        avadibleEncounter = posibleEncounters;
    }

    public EnemyType GetNextEncounter()
    {
        if (avadibleEncounter == null) InitializeData();

        EnemyType selectedEnemy;

        List<EnemyType> candidates = avadibleEncounter.Where(e => e.Difficulty == encounters[lastEncounterIndex]).ToList();

        if (candidates.Count() <= 0)
        {
            Debug.LogError($"NO ENEMIES AVADIBLE OF DIFFICULTY LEVEL: {encounters[lastEncounterIndex]}");
            return null;
        }


        int i = Random.Range(0, candidates.Count);

        selectedEnemy = candidates[i];

        candidates.Remove(selectedEnemy);

        if (candidates.Count() <= 0)
        {
            foreach (EnemyType e in posibleEncounters)
            {
                if (e.Difficulty == encounters[lastEncounterIndex])
                    avadibleEncounter.Add(e);
            }
        }

        lastEncounterIndex ++;
        return selectedEnemy;
    }

    public EnemyType GetEncounterByDifficulty(EnemyDifficulty enemyDifficulty)
    {
        if(avadibleEncounter  == null) InitializeData();

        EnemyType selectedEnemy;

        List<EnemyType> candidates = avadibleEncounter.Where(e => e.Difficulty == enemyDifficulty).ToList();
        
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
            foreach(EnemyType e in posibleEncounters)
            {
                if(e.Difficulty == enemyDifficulty)
                    avadibleEncounter.Add(e);
            }
        }

        return selectedEnemy;
    }

    public int GetNumEncounters() { return encounters.Count; }

    public List<BoardTile> GetinteractableTiles() {  return interactableTiles; }
}
