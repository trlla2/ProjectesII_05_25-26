using System.Collections;
using UnityEngine;

public class OnBoardingTrigger_UsableItems : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(CheckIfPlayerHasUsableItems());
    }

    private IEnumerator CheckIfPlayerHasUsableItems()
    {
        yield return new WaitForSeconds(1f);
        foreach (Item item in GameManager.Instance.PlayerStats.Inventory)
        {
            if (item.IsActivatable)
            {
                OnBoardingManager.Instance.TriggerIfNew(OnBoardingKey.USABLE_ITEMS);
                break;
            }
        }
    }
}
