using UnityEngine;


public class Palanca : MonoBehaviour
{
    [SerializeField]
    Grid grid;

    [SerializeField]
    int times;

    [SerializeField]
    int cost = 4;

    private bool isEnabled = true;

    private void Start()
    {
        GameManager.Instance.OnEndBattle += DisablePalanca;
    }
    public void OnMouseDown()
    {
        if (!grid.isScrolling && isEnabled)
        {
            if(PlayerStats.Instance.UseMoney(cost))
            {
                grid.StartCoroutine(grid.PerformScroll(times));
                grid.ResetWaitTimer();
            }
        }
    }


    private void OnDestroy()
    {
        GameManager.Instance.OnEndBattle -= DisablePalanca;
    }

    public void DisablePalanca(bool isWin, int exp, int money)
    {
        isEnabled = false;
    }
}
