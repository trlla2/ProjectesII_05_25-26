using System.Collections.Generic;
using UnityEngine;

public class BombCableVisual : MonoBehaviour
{
    [SerializeField] private Material cableMaterial1;
    [SerializeField] private Material cableMaterial2;
    [SerializeField] private Material cableMaterial3;

    private Dictionary<Vector2Int, List<LineRenderer>> cablesByOffset = new Dictionary<Vector2Int, List<LineRenderer>>();
    private Dictionary<int, List<LineRenderer>> cablesByColorIndex = new Dictionary<int, List<LineRenderer>>();
    private List<Vector2Int> activeOffsets = new List<Vector2Int>();
    private Vector2Int bombPosition;
    private Grid grid;

    int destroyIndex = 0;

    [SerializeField] private float LineOffset = 0.08f;
    [SerializeField] private int LinesPerCardinal = 3;
    [SerializeField] private float LineWidth = 0.05f;

    public void Initialize(Grid grid, Vector2Int bombPos, List<Vector2Int> offsets)
    {
        this.grid = grid;
        this.bombPosition = bombPos;
        this.activeOffsets = new List<Vector2Int>(offsets);

        ClearAllLines();
        CreateCableLines();
    }

    private void CreateCableLines()
    {
        if (activeOffsets.Count < 4) return;

        // Initialize color tracking
        cablesByColorIndex[0] = new List<LineRenderer>();
        cablesByColorIndex[1] = new List<LineRenderer>();
        cablesByColorIndex[2] = new List<LineRenderer>();

        foreach (Vector2Int offset in activeOffsets)
        {
            List<LineRenderer> lines = new List<LineRenderer>();

            for (int i = 0; i < LinesPerCardinal; i++)
            {
                Material material = GetMaterialForIndex(i);
                LineRenderer line = CreateLineRenderer(material);
                SetupLineRenderer(line, offset, i);
                lines.Add(line);

                // Track by color
                cablesByColorIndex[i].Add(line);
            }

            cablesByOffset[offset] = lines;
        }
    }

    private LineRenderer CreateLineRenderer(Material material)
    {
        GameObject lineObj = new GameObject("CableLine");
        lineObj.transform.SetParent(transform);

        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        line.material = material;
        line.startWidth = LineWidth;
        line.endWidth = LineWidth;
        line.useWorldSpace = false;
        line.sortingOrder = -3;

        return line;
    }

    private Material GetMaterialForIndex(int index)
    {
        switch (index)
        {
            case 0: return cableMaterial1;
            case 1: return cableMaterial2;
            case 2: return cableMaterial3;
            default: return cableMaterial1;
        }
    }

    private void SetupLineRenderer(LineRenderer line, Vector2Int offset, int lineIndex)
    {
        if (line == null) return;

        line.positionCount = 2;
        line.loop = false;

        Vector3 bombWorldPos = grid.VisualGrid.GridToWorldPosition(bombPosition);
        Vector3 cardinalWorldPos = grid.VisualGrid.GridToWorldPosition(bombPosition + offset);

        Vector3 direction = (cardinalWorldPos - bombWorldPos).normalized;
        Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f);
        float indexOffset = (lineIndex - 1) * LineOffset; // -1, 0, 1 for 3 lines

        Vector3 offsetVector = perpendicular * indexOffset;

        line.SetPosition(0, bombWorldPos + offsetVector);
        line.SetPosition(1, cardinalWorldPos + offsetVector);
        line.enabled = true;
    }

    public void RemoveCable(Vector2Int clearedOffset)
    {
        if (!cablesByOffset.ContainsKey(clearedOffset)) return;

        List<LineRenderer> linesToRemove = cablesByOffset[clearedOffset];

        foreach (LineRenderer line in linesToRemove)
        {
            if (line != null)
            {
                for (int i = 0; i < LinesPerCardinal; i++)
                {
                    if (cablesByColorIndex.ContainsKey(i))
                    {
                        cablesByColorIndex[i].Remove(line);
                    }
                }
                
                Destroy(line.gameObject);
            }
        }

        cablesByOffset.Remove(clearedOffset);
        activeOffsets.Remove(clearedOffset);

        DestroyAllLinesOfColor(destroyIndex);
        destroyIndex++;
    }

    private void DestroyAllLinesOfColor(int colorIndex)
    {
        if (!cablesByColorIndex.ContainsKey(colorIndex)) return;

        List<LineRenderer> linesToDestroy = new List<LineRenderer>(cablesByColorIndex[colorIndex]);

        foreach (LineRenderer line in linesToDestroy)
        {
            if (line != null)
            {
                line.enabled = false;
                Destroy(line.gameObject);

                // Remove from offset tracking
                foreach (var kvp in cablesByOffset)
                {
                    kvp.Value.Remove(line);
                }
            }
        }

        cablesByColorIndex[colorIndex].Clear();
    }

    public void ClearAllLines()
    {
        destroyIndex = 0;
        foreach (var kvp in cablesByOffset)
        {
            foreach (LineRenderer line in kvp.Value)
            {
                if (line != null)
                {
                    line.enabled = false;
                    Destroy(line.gameObject);
                }
            }
        }
        cablesByOffset.Clear();
        cablesByColorIndex.Clear();
    }

    public int GetActiveCableCount()
    {
        return cablesByOffset.Count;
    }

    private void OnDestroy()
    {
        ClearAllLines();
    }
}