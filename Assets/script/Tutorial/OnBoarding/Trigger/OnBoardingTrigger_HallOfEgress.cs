using UnityEngine;

public class OnBoardingTrigger_HallOfEgress : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // maybe intro del hall of egress?
    }

    public void TriggerOnBoardingSpellBook()
    {
        OnBoardingManager.Instance.TriggerIfNew(OnBoardingKey.SPELLBOOK);
    }
}
