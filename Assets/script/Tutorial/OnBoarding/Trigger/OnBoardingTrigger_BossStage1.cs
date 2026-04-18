using System.Collections;
using UnityEngine;

public class OnBoardingTrigger_BossStage1 : MonoBehaviour
{
    [SerializeField] private Orb _bossOrb1;

    private bool hasTriggeredOrbChargedOnBoarding = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(TriggerStartOnBoardingWithDelay());
    }

    private void Update()
    {
        if (!hasTriggeredOrbChargedOnBoarding && _bossOrb1.State == OrbState.Charged)
        {
            hasTriggeredOrbChargedOnBoarding = true;
            StartCoroutine(TriggerOrbChargedOnBoardingWithDelay());
        }
    }

    private IEnumerator TriggerStartOnBoardingWithDelay()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        OnBoardingManager.Instance.TriggerIfNew(OnBoardingKey.BOSS_STAGE1_START);
    }

    private IEnumerator TriggerOrbChargedOnBoardingWithDelay()
    {
        yield return new WaitForSecondsRealtime(10f);
        if (_bossOrb1.State == OrbState.Charging || _bossOrb1.State == OrbState.Activated || _bossOrb1.State == OrbState.Inactive)
            yield break;
        
        OnBoardingManager.Instance.TriggerIfNew(OnBoardingKey.BOSS_STAGE1_ORBCHARGED);
    }
}
