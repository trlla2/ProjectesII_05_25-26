using UnityEngine;


public enum RewardType { CURRENCY, OBJECT, SKIN, SPELL }
public interface IDropeable 
{

    string RewardName { get; }
    Sprite Icon { get; }
    Sprite[] sprites { get; }
    Color[] Colors { get; }

    RewardType Type { get; }

    void ClaimReward();
}
