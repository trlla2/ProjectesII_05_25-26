using UnityEngine;


public class Palanca : MonoBehaviour
{
    [SerializeField]
    Grid grid;

    [SerializeField]
    int times;

    [SerializeField]
    int cost = 4;

    [SerializeField]
    private AudioSource pullSound;

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
                pullSound.Play();
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
