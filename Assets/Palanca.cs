using UnityEngine;
using System.Collections;

public class Palanca : MonoBehaviour
{
    [SerializeField]
    Grid grid;

    public void OnMouseDown()
    {
        grid.StartCoroutine(grid.PerformScroll(4));
        grid.ResetWaitTimer();
    }
}
