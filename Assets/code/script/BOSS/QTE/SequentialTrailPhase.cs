using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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

    public SequentialTracePhase(Grid grid, QTEVisualController visual, int points = 6, float revealTime = 3f, float inputTime = 4f)
    {
        this.grid = grid;
        this.visualController = visual;
        this.coroutineRunner = visual; // QTEVisualController is a MonoBehaviour, we need to run coroutines in behabour obj not playn classes
        pointCount = points;
        revealDuration = revealTime;
        inputDuration = inputTime;

        // Get references
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
        if (isRevealing)
        {
            // Just wait for the sequential reveal to finish
            return;
        }

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

    private void StartSequentialReveal()
    {
        // Build list of (number, position) tuples
        List<(int number, Vector2Int position)> numbersToReveal = new List<(int, Vector2Int)>();
        
        for (int i = 0; i < tracePoints.Count; i++)
        {
            numbersToReveal.Add((i + 1, tracePoints[i]));
        }

        // Calculate delay between each reveal
        float delayBetween = revealDuration / pointCount;

        // Start the sequential reveal coroutine
        visualController.RevealNumbersSequential(numbersToReveal, delayBetween);

        // Schedule the transition to input phase using the coroutineRunner
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
        bool isPressing = Input.GetMouseButton(0);
        Vector2Int? gridPos = GetGridPositionFromUIInput();

        //Update trail preview
        if (isPressing && visualController != null && gridPos.HasValue)
        {
            Vector3 worldPos = grid.VisualGrid.GridToWorldPosition(gridPos.Value);
            visualController.UpdateTrailToPosition(worldPos);
        }

        //Check for valid next number - only collect if it's the correct one in sequence
        if (isPressing && gridPos.HasValue)
        {
            // Only process if this is a new position (not the last one we touched)
            if (!lastTouchedPosition.HasValue || lastTouchedPosition.Value != gridPos.Value)
            {
                // Only collect if it's the correct next number in the sequence
                if (gridPos.Value == tracePoints[currentTargetIndex])
                {
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
            }
        }

        wasPressingLastFrame = isPressing;
    }

    private Vector2Int? GetGridPositionFromUIInput()
    {
        // Use UI raycasting since we're working with UI elements
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        //Also check world space positions for grid cells
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0;

        //Check against each trace point to see if we're close enough
        float hitRadius = 1f;
        
        for (int i = 0; i < tracePoints.Count; i++)
        {
            Vector2Int gridPos = tracePoints[i];
            Vector3 cellWorldPos = grid.VisualGrid.GridToWorldPosition(gridPos);
            cellWorldPos.z = 0;

            float distance = Vector2.Distance(new Vector2(worldPos.x, worldPos.y), 
                                             new Vector2(cellWorldPos.x, cellWorldPos.y));

            if (distance < hitRadius)
            {
                return gridPos;
            }
        }

        return null;
    }

    public List<Vector2Int> GetTracePoints() => tracePoints;
    public float GetRemainingTime() => timer;
    public int GetCurrentTarget() => currentTargetIndex;
}