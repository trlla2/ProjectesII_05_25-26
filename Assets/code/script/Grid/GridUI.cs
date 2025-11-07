using UnityEngine;
using UnityEngine.UI;

public class GridUI : MonoBehaviour
{
    [Header("GRID Script")]
    [SerializeField]
    private Grid grid;

    [Header("Gird Timer")]
    [SerializeField]
    private Image timeCircle;
    [SerializeField] private AnimationCurve timeCurve;

    private void Update()
    {
        timeCircle.fillAmount = timeCurve.Evaluate(1 -(grid.GetCurrentScrollTimer() / grid.GetScrollTime()));
    }
}
