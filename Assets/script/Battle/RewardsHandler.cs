using TMPro;
using UnityEngine;

public class RewardsHandler : MonoBehaviour
{

    [Header("With spell stuff")]
    [SerializeField]
    private GameObject rewardsWith;

    [SerializeField]
    private GameObject expWith;
    [SerializeField]
    private TMP_Text expTextWith;
    [SerializeField]
    private GameObject goldWith;
    [SerializeField]
    private TMP_Text goldTextWith;

    [SerializeField]
    private UISpell_Behaviour spellBehaviour;

    [Header("Without spell stuff")]
    [SerializeField]
    private GameObject rewardsWithout;

    [SerializeField]
    private GameObject expWithout;
    [SerializeField]
    private TMP_Text expTextWithout;
    [SerializeField]
    private GameObject goldWithout;
    [SerializeField]
    private TMP_Text goldTextWithout;

    private void Start()
    {
        GameManager.Instance.CombatHandler.OnEndBattle += SetWinLoseReward;
    }

    private void SetWinLoseReward(bool isWin, int exp, int gold, Spell drop)
    {
        if (drop != null)
        {
            if (exp > 0)
                expTextWith.text = exp.ToString();
            else
                expWith.SetActive(false);

            if (gold > 0)
                goldTextWith.text = gold.ToString();
            else
                goldWith.SetActive(false);

            spellBehaviour.SetupSpellUI(drop);


            rewardsWith.SetActive(true);
            rewardsWithout.SetActive(false);
        }
        else
        {
            if(exp > 0)
                expTextWithout.text = exp.ToString();
            else
                expWithout.SetActive(false);

            if (gold > 0)
                goldTextWithout.text = gold.ToString();
            else
                goldWithout.SetActive(false);


            rewardsWith.SetActive(false);
            rewardsWithout.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        GameManager.Instance.CombatHandler.OnEndBattle -= SetWinLoseReward;
    }
}
