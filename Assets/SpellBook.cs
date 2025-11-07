using UnityEngine;
using UnityEngine.EventSystems;

public class UIHoverMove : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Movement Settings")]
    public Vector2 startPosition;
    public Vector2 endPosition;
    public float duration = 0.5f;
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private RectTransform rectTransform;

    private float t;
    private Vector2 from;
    private Vector2 to;
    private bool isMoving;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = startPosition;
    }

    private void Update()
    {
        if (!isMoving) return;

        t += Time.deltaTime / duration;
        float curveT = moveCurve.Evaluate(Mathf.Clamp01(t));
        rectTransform.anchoredPosition = Vector2.Lerp(from, to, curveT);

        if (t >= 1f)
            isMoving = false;
    }

    //OnMouseEnter but for UI events
    public void OnPointerEnter(PointerEventData eventData)
    {
        StartMove(rectTransform.anchoredPosition, endPosition);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartMove(rectTransform.anchoredPosition, startPosition);
    }

    private void StartMove(Vector2 fromPos, Vector2 toPos)
    {
        from = fromPos;
        to = toPos;
        t = 0f;
        isMoving = true;
    }
}
