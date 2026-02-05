using System;
using System.Collections.Generic;
using UnityEngine;

public class SequentialTracePhase : IQTEPhase
{
    public event Action<bool> OnPhaseComplete;

    private int pointCount;
    private float revealDuration;
    private float inputDuration;
    private Grid grid;

    private List<Vector2Int> tracePoints;
    private int currentTargetIndex;
    private bool isInputPhase;
    private float timer;
    private bool isRevealing;
    private int revealedCount;
    private float revealTimer;

    public SequentialTracePhase(Grid grid, int points = 6, float revealTime = 3f, float inputTime = 4f)
    {
        this.grid = grid;
        pointCount = points;
        revealDuration = revealTime;
        inputDuration = inputTime;
    }

    public void Enter()
    {
        GenerateTracePoints();
        currentTargetIndex = 0;
        isInputPhase = false;
        isRevealing = true;
        revealedCount = 0;
        revealTimer = 0f;
    }

    public void Update()
    {
        if (isRevealing)
        {
            UpdateReveal();
            return;
        }

        if (!isInputPhase) return;

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            OnPhaseComplete?.Invoke(false);
            return;
        }

        CheckInput();
    }

    public void Exit()
    {
        tracePoints?.Clear();
    }

    private void GenerateTracePoints()
    {
        tracePoints = new List<Vector2Int>();
        var available = new List<Vector2Int>();

        for (int x = 0; x < Grid.GridSize; x++)
            for (int y = 0; y < Grid.GridSize; y++)
                available.Add(new Vector2Int(x, y));

        for (int i = 0; i < pointCount && available.Count > 0; i++)
        {
            int idx = UnityEngine.Random.Range(0, available.Count);
            tracePoints.Add(available[idx]);
            available.RemoveAt(idx);
        }
    }

    private void UpdateReveal()
    {
        revealTimer += Time.deltaTime;
        float interval = revealDuration / pointCount;

        while (revealedCount < pointCount && revealTimer >= interval * (revealedCount + 1))
        {
            RevealPoint(revealedCount);
            revealedCount++;
        }

        if (revealedCount >= pointCount)
        {
            isRevealing = false;
            isInputPhase = true;
            timer = inputDuration;
        }
    }

    private void RevealPoint(int index)
    {
        // Visual feedback handled by QTEVisualController
    }

    private void CheckInput()
    {
        if (!Input.GetMouseButton(0)) return;

        Vector2Int? gridPos = GetGridPositionFromInput();
        if (!gridPos.HasValue) return;

        if (gridPos.Value == tracePoints[currentTargetIndex])
        {
            currentTargetIndex++;
            if (currentTargetIndex >= pointCount)
            {
                OnPhaseComplete?.Invoke(true);
            }
        }
        else if (currentTargetIndex > 0)
        {
            OnPhaseComplete?.Invoke(false);
        }
    }

    private Vector2Int? GetGridPositionFromInput()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        for (int x = 0; x < Grid.GridSize; x++)
        {
            for (int y = 0; y < Grid.GridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Vector3 cellWorld = grid.VisualGrid.GridToWorldPosition(pos);

                if (Vector2.Distance(worldPos, cellWorld) < 0.5f)
                    return pos;
            }
        }
        return null;
    }

    public List<Vector2Int> GetTracePoints() => tracePoints;
    public float GetRemainingTime() => timer;
    public int GetCurrentTarget() => currentTargetIndex;
}