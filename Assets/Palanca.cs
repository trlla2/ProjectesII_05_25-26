using UnityEngine;
using System.Collections;

public class Palanca : MonoBehaviour
{
    [SerializeField]
    Grid grid;

    [SerializeField]
    int times;

    public void OnMouseDown()
    {
        if (!grid.isScrolling)
        {
            grid.StartCoroutine(grid.PerformScroll(times));
            grid.ResetWaitTimer();
        }
    }
}
