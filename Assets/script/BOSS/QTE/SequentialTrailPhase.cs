using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SequentialTracePhase : IQTEPhase
{
    public event Action<bool> OnPhaseComplete;

    private int pointCount;
    private float revealDuration;
    private float inputDuration;
    private Grid grid;
    private QTEVisualController visualController;
    private MonoBehaviour coroutineRunner;
    private Camera mainCamera;

    private List<Vector2Int> tracePoints;
    private int currentTargetIndex;
    private bool isInputPhase;
    private float timer;
    private bool isRevealing;
    private bool wasPressingLastFrame;
    private Vector2Int? lastTouchedPosition;

    //radius for a correct touch
    private const float HitRadius = 0.8f;

    public SequentialTracePhase(Grid grid, QTEVisualController visual, int points = 6, float revealTime = 3f, float inputTime = 4f, System.Action<bool> onComplete = null)
    {
        this.grid = grid;
        this.visualController = visual;
        this.coroutineRunner = visual; // QTEVisualController is a MonoBehaviour
        revealDuration = revealTime;
        pointCount = points;
        inputDuration = inputTime;

        OnPhaseComplete += onComplete;

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    public void Enter()
    {
        GenerateTracePoints();
        currentTargetIndex = 0;
        isInputPhase = false;
        isRevealing = true;
        wasPressingLastFrame = false;
        lastTouchedPosition = null;

        if (visualController != null)
        {
            visualController.StartQTE();
            StartSequentialReveal();
        }
    }

    public void update()
    {
        if (isRevealing) return;
        if (!isInputPhase) return;

        timer -= Time.unscaledDeltaTime;
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

        for (int x = 0; x < grid.GridSize; x++)
            for (int y = 0; y < grid.GridSize; y++)
                available.Add(new Vector2Int(x, y));

        for (int i = 0; i < pointCount && available.Count > 0; i++)
        {
            int idx = UnityEngine.Random.Range(0, available.Count);
            tracePoints.Add(available[idx]);
            available.RemoveAt(idx);
        }
    }

    private void StartSequentialReveal()
    {
        List<(int number, Vector2Int position)> numbersToReveal = new List<(int, Vector2Int)>();

        for (int i = 0; i < tracePoints.Count; i++)
        {
            numbersToReveal.Add((i + 1, tracePoints[i]));
        }

        float delayBetween = revealDuration / pointCount;

        visualController.RevealNumbersSequential(numbersToReveal, delayBetween);

        if (coroutineRunner != null)
        {
            coroutineRunner.StartCoroutine(WaitForRevealComplete());
        }
    }

    private IEnumerator WaitForRevealComplete()
    {
        yield return new WaitForSecondsRealtime(revealDuration);

        isRevealing = false;
        isInputPhase = true;
        timer = inputDuration;
    }

    private void CheckInput()
    {
        bool isPressing = Input.GetMouseButton(0) || Input.GetMouseButtonDown(0);

        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        // Trail preview
        if (isPressing && visualController != null)
        {
            visualController.UpdateTrailToPosition(mouseWorldPos);
        }
        else if (!isPressing && visualController != null)
        {
            visualController.ClearTrailPreview();
        }

        if (!isPressing)
        {
            wasPressingLastFrame = false;
            return;
        }

        if (currentTargetIndex < 0 || currentTargetIndex >= tracePoints.Count)
            return;

        Vector2Int targetGridPos = tracePoints[currentTargetIndex];
        Vector3 targetWorldPos = grid.VisualGrid.GridToWorldPosition(targetGridPos);
        targetWorldPos.z = 0;

        float distance = Vector2.Distance(
            new Vector2(mouseWorldPos.x, mouseWorldPos.y),
            new Vector2(targetWorldPos.x, targetWorldPos.y)
        );

        if (distance <= HitRadius)
        {
            //prevent repeated triggering while staying in the same spot
            if (!lastTouchedPosition.HasValue || lastTouchedPosition.Value != targetGridPos)
            {
                if (visualController != null)
                {
                    visualController.OnNumberTouched(currentTargetIndex + 1, targetGridPos);
                }

                lastTouchedPosition = targetGridPos;
                currentTargetIndex++;

                if (currentTargetIndex >= pointCount)
                {
                    OnPhaseComplete?.Invoke(true);
                    if (visualController != null)
                        visualController.OnTraceComplete(true);
                }
            }
        }

        wasPressingLastFrame = isPressing;
    }

    public List<Vector2Int> GetTracePoints() => tracePoints;
    public float GetRemainingTime() => timer;
    public int GetCurrentTarget() => currentTargetIndex;
}