using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewBattlePass", menuName = "Game/Battle Pass")]
public class BattlePassType : ScriptableObject
{
    [System.Serializable]
    public class Reward
    {
        public int expObjective;
        public int quantity;
        [SerializeReference]
        public IDropeable reward;

        public void ClaimReward()
        {
            if (quantity > 1)
            {
                for (int i = 0; i < quantity; i++)
                {
                    reward.ClaimReward();
                }
            }
            else
            {
                reward.ClaimReward();
            }
        }
    }

    [SerializeField] private List<Reward> rewards;
    public List<Reward> Rewards => rewards;

    
}
