using System.Collections;
using UnityEngine;

public class OnBoardingTrigger_BossStage1 : MonoBehaviour
{
    [SerializeField] private Orb _bossOrb1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TriggerStartOnBoardingWithDelay();
    }

    //private void Update()
    //{
    //    if (_bossOrb1.State == OrbState.Charged)
    //    {
    //        TriggerOrbChargedOnBoardingWithDelay();
    //    }
    //}

    private IEnumerator TriggerStartOnBoardingWithDelay()
    {
        yield return new WaitForSeconds(0.1f);
        OnBoardingManager.Instance.TriggerIfNew(OnBoardingKey.BOSS_STAGE1_START);
    }

    private IEnumerator TriggerOrbChargedOnBoardingWithDelay()
    {
        yield return new WaitForSeconds(2f);
        if (_bossOrb1.State == OrbState.Charged)
            OnBoardingManager.Instance.TriggerIfNew(OnBoardingKey.BOSS_STAGE1_ORBCHARGED);
    }
}
