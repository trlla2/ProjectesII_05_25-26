using System;
using System.Collections.Generic;
using UnityEngine;

public class TimingPhase : IQTEPhase
{
    public event Action<bool> OnPhaseComplete;

    private int pointCount;
    private float beatDuration;
    private float successWindowStart;
    private float successWindowEnd;

    private Grid grid;
    private QTEVisualController visualController;
    private Camera mainCamera;

    private List<Vector2Int> timingPoints;
    private int currentIndex;
    private float beatTimer;
    private bool isActive;
    private bool inputConsumed;

    private TimingRingUI timingRing;

    public TimingPhase(
        Grid grid,
        QTEVisualController visual,
        TimingRingUI timingRing = null,
        int points = 5,
        float beatDuration = 1.2f,
        float successWindowStart = 0.6f,
        float successWindowEnd = 0.95f,
        Action<bool> onComplete = null)
    {
        this.grid = grid;
        this.visualController = visual;
        this.timingRing = timingRing;
        this.pointCount = points;
        this.beatDuration = beatDuration;
        this.successWindowStart = successWindowStart;
        this.successWindowEnd = successWindowEnd;

        OnPhaseComplete += onComplete;

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    public void Enter()
    {
        GeneratePoints();
        currentIndex = 0;
        isActive = true;

        if (visualController != null)
            visualController.StartQTE();

        ShowCurrentBeat();
    }

    public void update()
    {
        if (!isActive) return;

        beatTimer += Time.unscaledDeltaTime;
        float progress = Mathf.Clamp01(beatTimer / beatDuration);

        if (timingRing != null)
            timingRing.SetProgress(progress);

        if (beatTimer >= beatDuration)
        {
            HandleMiss();
            return;
        }

        if (Input.GetMouseButtonDown(0) && !inputConsumed)
        {
            inputConsumed = true;
            EvaluateInput(progress);
        }
    }

    public void Exit()
    {
        isActive = false;
        timingPoints?.Clear();

        if (timingRing != null)
            timingRing.Hide();

        if (visualController != null)
            visualController.EndQTE();
    }

    // --- Private ---

    private void GeneratePoints()
    {
        timingPoints = new List<Vector2Int>();
        var available = new List<Vector2Int>();

        for (int x = 0; x < grid.GridSize; x++)
            for (int y = 0; y < grid.GridSize; y++)
                available.Add(new Vector2Int(x, y));

        for (int i = 0; i < pointCount && available.Count > 0; i++)
        {
            int idx = UnityEngine.Random.Range(0, available.Count);
            timingPoints.Add(available[idx]);
            available.RemoveAt(idx);
        }
    }

    private void ShowCurrentBeat()
    {
        if (currentIndex >= timingPoints.Count) return;

        beatTimer = 0f;
        inputConsumed = false;

        Vector2Int gridPos = timingPoints[currentIndex];
        Vector3 worldPos = grid.VisualGrid.GridToWorldPosition(gridPos);
        worldPos.z = 0;

        if (visualController != null)
            visualController.ShowTimingBeat(currentIndex + 1, worldPos);

        if (timingRing != null)
            timingRing.ShowAt(worldPos);
    }

    private void EvaluateInput(float progress)
    {
        bool inWindow = progress >= successWindowStart && progress <= successWindowEnd;

        if (inWindow)
            HandleHit(progress);
        else
            HandleMiss();
    }

    private void HandleHit(float progress)
    {
        bool isPerfect = progress >= 0.75f && progress <= 0.9f;

        if (visualController != null)
            visualController.OnTimingBeatHit(currentIndex + 1, timingPoints[currentIndex], isPerfect);

        currentIndex++;

        if (currentIndex >= pointCount)
        {
            isActive = false;
            OnPhaseComplete?.Invoke(true);
            if (visualController != null)
                visualController.OnTraceComplete(true);
        }
        else
        {
            ShowCurrentBeat();
        }
    }

    private void HandleMiss()
    {
        isActive = false;

        if (visualController != null)
            visualController.OnTimingBeatMissed(currentIndex + 1);

        OnPhaseComplete?.Invoke(false);

        if (visualController != null)
            visualController.OnTraceComplete(false);
    }
    public float GetBeatProgress() => Mathf.Clamp01(beatTimer / beatDuration);
    public int GetCurrentTarget() => currentIndex;
    public int GetTotalPoints() => pointCount;
    public List<Vector2Int> GetTimingPoints() => timingPoints;
}