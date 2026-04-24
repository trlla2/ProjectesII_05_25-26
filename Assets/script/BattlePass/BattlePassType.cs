using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewBattlePass", menuName = "Game/Battle Pass")]
public class BattlePassType : ScriptableObject
{
    [SerializeField] private List<Reward> rewards;
    public List<Reward> Rewards => rewards;
}
