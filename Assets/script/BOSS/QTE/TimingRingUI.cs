using UnityEngine;
using UnityEngine.UI;

public class TimingRingUI : MonoBehaviour
{
    [Header("Ring Visuals")]
    [SerializeField] private Image ringImage;

    [Header("Scale Settings")]
    [SerializeField] private float outerScale = 2.5f;
    [SerializeField] private float innerScale = 0.4f;

    private Canvas qteCanvas;
    private Camera mainCamera;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        Hide();
    }

    public void Initialize(Canvas canvas, Camera cam)
    {
        qteCanvas = canvas;
        mainCamera = cam != null ? cam : Camera.main;
    }

    public void ShowAt(Vector3 worldPos)
    {
        if (qteCanvas == null || mainCamera == null)
        {
            gameObject.SetActive(true);
            return;
        }

        RectTransform canvasRect = qteCanvas.GetComponent<RectTransform>();
        Vector2 viewportPos = mainCamera.WorldToViewportPoint(worldPos);
        Vector2 canvasPos = new Vector2(
            (viewportPos.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f),
            (viewportPos.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f)
        );

        rectTransform.anchoredPosition = canvasPos;
        rectTransform.localScale = Vector3.one * outerScale;

        if (ringImage != null)
        {
            Color c = ringImage.color;
            ringImage.color = new Color(c.r, c.g, c.b, 1f);
        }

        gameObject.SetActive(true);
    }

    public void SetProgress(float progress)
    {
        float scale = Mathf.Lerp(outerScale, innerScale, progress);
        rectTransform.localScale = Vector3.one * scale;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}