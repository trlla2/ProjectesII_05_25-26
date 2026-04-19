using JetBrains.Annotations;
using UnityEngine;
using System.Collections;

public class OnBoardingTrigger_HallOfEgress : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(OnBoardingTriggerIntroIfFirstTime());
    }

    public void TriggerOnBoardingSpellBook()
    {
        OnBoardingManager.Instance.TriggerIfNew(OnBoardingKey.SPELLBOOK);
        StartCoroutine(DedicoSpellbook());
    }

    private IEnumerator OnBoardingTriggerIntroIfFirstTime()
    {
        OnBoardingManager.Instance.TriggerIfNewWithDelay(OnBoardingKey.HALL_OF_EGRESS_INTRO);
        yield return new WaitUntil(() => OnBoardingManager.Instance.IsPanelExplainedWithKey(OnBoardingKey.HALL_OF_EGRESS_INTRO));
        OnBoardingManager.Instance.TriggerIfNew(OnBoardingKey.HALL_OF_EGRESS_GOTOSPELLBOOK);
        yield return new WaitUntil(() => OnBoardingManager.Instance.IsPanelExplainedWithKey(OnBoardingKey.HALL_OF_EGRESS_GOTOSPELLBOOK));
        OnBoardingManager.Instance.TriggerIfNew(OnBoardingKey.HALL_OF_EGRESS_GOTOSPELLBOOK_DEDICO);
    }

    private IEnumerator DedicoSpellbook()
    {
        yield return new WaitUntil(() => OnBoardingManager.Instance.IsPanelExplainedWithKey(OnBoardingKey.SPELLBOOK));
        OnBoardingManager.Instance.TriggerIfNew(OnBoardingKey.SPELLBOOK_DEDICO);
    }
}
