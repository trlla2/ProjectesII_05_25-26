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
    private QTEVisualController visualController;

    private List<Vector2Int> tracePoints;
    private int currentTargetIndex;
    private bool isInputPhase;
    private float timer;
    private bool isRevealing;
    private int revealedCount;
    private float revealTimer;
    private bool wasPressingLastFrame;
    private Vector2Int? lastTouchedPosition;

    public SequentialTracePhase(Grid grid, QTEVisualController visual, int points = 6, float revealTime = 3f, float inputTime = 4f)
    {
        this.grid = grid;
        this.visualController = visual;
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
        wasPressingLastFrame = false;
        lastTouchedPosition = null;

        if (visualController != null)
        {
            visualController.StartQTE();
        }
    }

    public void Update()
    {
        if (isRevealing)
        {
            UpdateReveal();
            return;
        }

        if (!isInputPhase) return;

        timer -= Time.unscaledDeltaTime; // Use unscaled time
        if (timer <= 0)
        {
            OnPhaseComplete?.Invoke(false);
            if (visualController != null)
                visualController.OnTraceComplete(false);
            return;
        }

        CheckInput();
    }

    public void Exit()
    {
        tracePoints?.Clear();
        if (visualController != null)
        {
            visualController.EndQTE();
        }
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
        revealTimer += Time.unscaledDeltaTime; // Use unscaled time
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
        if (visualController != null && index < tracePoints.Count)
        {
            visualController.RevealNumber(index + 1, tracePoints[index], 0f);
        }
    }

    private void CheckInput()
    {
        bool isPressing = Input.GetMouseButton(0);
        Vector2Int? gridPos = GetGridPositionFromInput();

        //Check if finger/mouse lifted, fail if already started
        if (wasPressingLastFrame && !isPressing && currentTargetIndex > 0)
        {
            OnPhaseComplete?.Invoke(false);
            if (visualController != null)
                visualController.OnTraceComplete(false);
            return;
        }

        //Update trail preview
        if (isPressing && visualController != null && gridPos.HasValue)
        {
            Vector3 worldPos = grid.VisualGrid.GridToWorldPosition(gridPos.Value);
            visualController.UpdateTrailToPosition(worldPos);
        }

        //Check for valid next number
        if (isPressing && gridPos.HasValue)
        {
            if (!lastTouchedPosition.HasValue || lastTouchedPosition.Value != gridPos.Value)
            {
                if (gridPos.Value == tracePoints[currentTargetIndex])
                {
                    //Correct number
                    if (visualController != null)
                    {
                        visualController.OnNumberTouched(currentTargetIndex + 1, gridPos.Value);
                    }

                    lastTouchedPosition = gridPos.Value;
                    currentTargetIndex++;

                    if (currentTargetIndex >= pointCount)
                    {
                        //Success!
                        OnPhaseComplete?.Invoke(true);
                        if (visualController != null)
                            visualController.OnTraceComplete(true);
                    }
                }
                else
                {
                    //Wrong number touched, fail if we've already started
                    if (currentTargetIndex > 0)
                    {
                        OnPhaseComplete?.Invoke(false);
                        if (visualController != null)
                            visualController.OnTraceComplete(false);
                    }
                }
            }
        }

        wasPressingLastFrame = isPressing;
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