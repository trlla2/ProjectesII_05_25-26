using System.Collections.Generic;
using UnityEngine;

public class BombCableVisual : MonoBehaviour
{
    [SerializeField] private LineRenderer cableLine1;
    [SerializeField] private LineRenderer cableLine2;
    [SerializeField] private LineRenderer cableLine3;

    private List<LineRenderer> cableLines = new List<LineRenderer>();
    private List<Vector2Int> cardinalPositions = new List<Vector2Int>();
    private Vector2Int bombPosition;
    private Grid grid;


    private const float LineOffset = 0.18f;

    public void Initialize(Grid grid, Vector2Int bombPos, List<Vector2Int> cardinals)
    {
        this.grid = grid;
        this.bombPosition = bombPos;
        this.cardinalPositions = new List<Vector2Int>(cardinals);

        cableLines.Clear();
        if (cableLine1 != null) cableLines.Add(cableLine1);
        if (cableLine2 != null) cableLines.Add(cableLine2);
        if (cableLine3 != null) cableLines.Add(cableLine3);

        cableLine1.gameObject.SetActive(true);
        cableLine2.gameObject.SetActive(true);
        cableLine3.gameObject.SetActive(true);

        CreateCableLines();
    }

    private void CreateCableLines()
    {
        if (cardinalPositions.Count < 4 || cableLines.Count < 3) return;

        //ensure consistent order: Top, Right, Bottom, Left
        List<Vector2Int> sorted = SortCardinalPositions(cardinalPositions);

        for (int i = 0; i < cableLines.Count; i++)
        {
            SetupLineRendererLoop(cableLines[i], sorted, i);
        }

        foreach (LineRenderer line in cableLines)
        {
            if (line != null)
                line.enabled = true;
        }
    }

    private List<Vector2Int> SortCardinalPositions(List<Vector2Int> positions)
    {
        Vector2Int top = bombPosition, right = bombPosition, bottom = bombPosition, left = bombPosition;

        foreach (Vector2Int pos in positions)
        {
            Vector2Int diff = pos - bombPosition;
            if (diff.y > 0) top = pos;
            else if (diff.y < 0) bottom = pos;
            else if (diff.x > 0) right = pos;
            else if (diff.x < 0) left = pos;
        }

        return new List<Vector2Int> { top, right, bottom, left };
    }

    private void SetupLineRendererLoop(LineRenderer line, List<Vector2Int> sorted, int lineIndex)
    {
        if (line == null) return;


        line.positionCount = 5;
        line.loop = false;

        //Calculate offset perpendicular to the center
        float offset = (lineIndex - 1) * LineOffset;

        for (int i = 0; i < 5; i++)
        {
            int posIndex = i % 4; //Wrap around to close the loop
            Vector3 worldPos = grid.VisualGrid.GridToWorldPosition(sorted[posIndex]);
            
            Vector3 toBomb = grid.VisualGrid.GridToWorldPosition(bombPosition);
            Vector3 direction = (worldPos - toBomb).normalized;
            worldPos += direction * offset;

            line.SetPosition(i, worldPos);
        }
    }

    public void RemoveCable(Vector2Int clearedPosition)
    {
        if (!cardinalPositions.Contains(clearedPosition)) return;

        for (int i = 0; i < cableLines.Count; i++)
        {
            if (cableLines[i] != null && cableLines[i].enabled)
            {
                cableLines[i].enabled = false;
                cableLines.RemoveAt(i);
                return;
            }
        }
    }

    public void ClearAllLines()
    {
        foreach (LineRenderer line in cableLines)
        {
            if (line != null)
                line.enabled = false;
        }
        cableLines.Clear();
    }

    public int GetActiveCableCount()
    {
        return cableLines.Count;
    }

    private void OnDestroy()
    {
        ClearAllLines();
    }
}