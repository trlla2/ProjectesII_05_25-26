using UnityEngine;
using static Solo.MOST_IN_ONE.MOST_HapticFeedback;

public class PathVisualizer : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    public Transform cursorVisual;
    [SerializeField] private float cursorLerpSpeed = 25f;

    private Grid grid;
    private VisualGrid visualGrid;
    private PathData pathData;

    public void Initialize(Grid gridController, VisualGrid visual, PathData data)
    {
        grid = gridController;
        visualGrid = visual;
        pathData = data;

        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer != null)
            lineRenderer.positionCount = 0;
    }

    public void ShowCursor(bool show)
    {
        if (cursorVisual != null)
            cursorVisual.gameObject.SetActive(show);
    }

    public void UpdateLineRenderer()
    {
        if (lineRenderer == null || pathData.Count == 0)
        {
            if (lineRenderer != null)
                lineRenderer.positionCount = 0;
            return;
        }

        lineRenderer.positionCount = pathData.Count;
        Vector2Int[] positions = pathData.Positions;

        for (int i = 0; i < positions.Length; i++)
        {
            ElementVisual visual = grid.GetElementVisualAt(positions[i]);
            if (visual != null)
            {
                Vector3 worldPos = visual.GetWorldPosition();
                lineRenderer.SetPosition(i, worldPos);
            }
        }
    }

    public void UpdateCursorPosition(bool isPathing)
    {
        if (cursorVisual == null)
            return;

        if (!isPathing)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            cursorVisual.position = mousePos;
            return;
        }

        if (pathData.Count == 0)
            return;

        //Lerp to last position in path
        Vector2Int lastPos = pathData.GetLast();
        ElementVisual lastVisual = grid.GetElementVisualAt(lastPos);

        if (lastVisual != null)
        {
            Vector3 targetPos = lastVisual.GetWorldPosition();
            cursorVisual.position = Vector3.Lerp(
                cursorVisual.position,
                targetPos,
                cursorLerpSpeed * Time.fixedDeltaTime
            );
        }
    }

    public void HighlightElement(Vector2Int position, bool highlight)
    {
        ElementVisual visual = grid.GetElementVisualAt(position);
        if (visual != null)
        {
            HapticsManager.Instance.GenerateHapticFeedback(HapticsManager.HapticTypes.RigidImpact);
            visual.GetComponent<Highlighter>().SetHighlight(highlight);
            visual.GetComponent<ScaleAnimator>().PlayScale(0.5f, 0.15f);
        }
    }

    public void ClearVisuals()
    {
        if (lineRenderer != null)
            lineRenderer.positionCount = 0;

        ShowCursor(false);
    }
}