using UnityEngine;

[System.Serializable]
public class Reward
{
    public int expCost;
    public int quantity;
    [SerializeField]
    private ScriptableObject rewardDrop;

    public IDropeable RewardDrop
    {
        get
        {
            if (rewardDrop is IDropeable dropable)
            {
                return dropable;
            }
            else
            {
                Debug.LogError("Reward does not implement IDropeable interface.");
                return null;
            }
        }
    }

    public void ClaimReward()
    {

        if (rewardDrop is not IDropeable dropable)
        {
            Debug.LogError("Reward does not implement IDropeable interface.");
            return;
        }
        IDropeable drop = (IDropeable)rewardDrop;

        if (quantity > 1)
        {
            for (int i = 0; i < quantity; i++)
            {
                drop.ClaimReward();
            }
        }
        else
        {
            drop.ClaimReward();
        }
    }
}
