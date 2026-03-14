using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private TutorialUIController uiController;
    [SerializeField] private Grid grid;
    [SerializeField] private EnemyBehaviour enemyBehaviour;

    [SerializeField] private TutorialSequence activeSequence;
    [SerializeField] private TutorialGridLayout activeLayout;
    private int currentStepIndex;
    private int refillIndex;
    private bool tutorialActive;
    private HashSet<Vector2Int> allowedPathSet;

    public event Action OnTutorialComplete;

    private void Start()
    {
        //Script executuion order nigthmare
        StartCoroutine(WaitFrameForStart());
    }

    IEnumerator WaitFrameForStart()
    {
        Debug.Log("Waiting for end of frame to start tutorial...");
        yield return new WaitForEndOfFrame();
        StartTutorial(activeSequence, activeLayout);
    }

    public bool IsTutorialActive() {  Debug.Log("IsTutorialActive: " + tutorialActive); return tutorialActive; }

    public bool IsPathAllowed(List<Vector2Int> path)
    {
        if (!tutorialActive || allowedPathSet == null)
            return true;

        if (allowedPathSet.Count == 0)
            return false;

        if (path.Count != allowedPathSet.Count)
            return false;

        foreach (var pos in path)
        {
            if (!allowedPathSet.Contains(pos))
                return false;
        }
        Debug.Log("Path is allowed: " + string.Join(", ", path));
        return true;
    }

    public bool IsPositionAllowed(Vector2Int pos)
    {
        if (!tutorialActive || allowedPathSet == null)
            return true;

        if (allowedPathSet.Count == 0)
            return false;
        Debug.Log($"Checking if position {pos} is allowed: {allowedPathSet.Contains(pos)}");
        return allowedPathSet.Contains(pos);
    }

    public void StartTutorial(TutorialSequence sequence, TutorialGridLayout layout)
    {
        activeSequence = sequence;
        activeLayout = layout;
        currentStepIndex = 0;
        refillIndex = 0;
        tutorialActive = true;

        ApplyGridLayout(layout);
        ExecuteStep(activeSequence.steps[0]);
    }

    public void StopTutorial()
    {
        tutorialActive = false;
        allowedPathSet = null;
        uiController.HideAll();
        OnTutorialComplete?.Invoke();
    }

    public void NotifySpellCast()
    {
        if (!tutorialActive) return;
        var step = activeSequence.steps[currentStepIndex];
        if (step.advanceCondition == TutorialAdvanceCondition.SpellCast)
            AdvanceStep();
    }

    public void NotifyEnemyDeath()
    {
        if (!tutorialActive) return;
        var step = activeSequence.steps[currentStepIndex];
        if (step.advanceCondition == TutorialAdvanceCondition.EnemyDeath)
            AdvanceStep();
    }

    public Element GetRefillElement(int x, int y)
    {
        if (activeLayout == null) return null;
        if (activeLayout.refillOverrides == null) return null;
        if (refillIndex >= activeLayout.refillOverrides.Count) return null;

        return activeLayout.refillOverrides[refillIndex].rows[y].elements[x];
    }

    public void ConsumeRefillOverride()
    {
        refillIndex++;
    }

    public bool HasRefillOverride()
    {
        if (activeLayout == null) return false;
        if (activeLayout.refillOverrides == null) return false;
        return refillIndex < activeLayout.refillOverrides.Count;
    }

    private void ApplyGridLayout(TutorialGridLayout layout)
    {
        for (int x = 0; x < Grid.GridSize; x++)
        {
            for (int y = 0; y < Grid.GridSize; y++)
            {
                Element element = layout.GetElement(x, y);
                if (element != null)
                {
                    grid.SetElementAt(new Vector2Int(x, y), element);
                    ElementVisual visual = grid.GetElementVisualAt(new Vector2Int(x, y));
                    if (visual != null)
                        visual.UpdateVisual(element);
                }
            }
        }
    }

    private void ExecuteStep(TutorialStep step)
    {
        allowedPathSet = step.allowedPath.ToHashSet();

        // If input is locked in this step, block all player pathing.
        if (step.lockInput)
        {
            allowedPathSet = new HashSet<Vector2Int>();
        };
        if (step.pauseEnemyTimers && enemyBehaviour != null)
            enemyBehaviour.PauseActiveTimedSpell();

        switch (step.type)
        {
            case TutorialStepType.Popup:
                uiController.ShowPopup(step.text, step.autoDismissTime, OnPopupDismissed);
                break;

            case TutorialStepType.Toast:
                uiController.ShowToast(step.text, step.autoDismissTime);
                if (step.advanceCondition == TutorialAdvanceCondition.Timer)
                    StartCoroutine(AutoAdvance(step.autoDismissTime));
                break;

            case TutorialStepType.HandDemo:
                uiController.ShowHandAnimation(step.handPath, grid, step.handDuration);
                ShowStepVisuals(step);
                StartCoroutine(AutoAdvance(step.handDuration));
                break;

            case TutorialStepType.WaitForSpellCast:
                ShowStepVisuals(step);
                break;

            case TutorialStepType.WaitForEnemyDeath:
                break;

            case TutorialStepType.FreePlay:
                allowedPathSet = null;
                ShowStepVisuals(step);
                if (step.advanceCondition == TutorialAdvanceCondition.Timer)
                    StartCoroutine(AutoAdvance(step.autoDismissTime));
                break;
        }
    }

    private void ShowStepVisuals(TutorialStep step)
    {
        if (step.highlightPositions != null && step.highlightPositions.Count > 0)
            uiController.ShowHighlights(step.highlightPositions, grid);

        if (step.showHand)
            uiController.ShowHandAnimation(step.handPath, grid, step.handDuration);
    }

    private void OnPopupDismissed()
    {
        if (!tutorialActive) return;
        TutorialStep step = activeSequence.steps[currentStepIndex];
        if (step.advanceCondition == TutorialAdvanceCondition.Tap)
            AdvanceStep();
    }

    private IEnumerator AutoAdvance(float delay)
    {
        yield return new WaitForSeconds(delay);
        AdvanceStep();
    }

    private void AdvanceStep()
    {
        uiController.HideAll();

        var currentStep = activeSequence.steps[currentStepIndex];
        if (currentStep.pauseEnemyTimers && enemyBehaviour != null)
            enemyBehaviour.ResumeActiveTimedSpell();

        currentStepIndex++;

        if (currentStepIndex >= activeSequence.steps.Count)
        {
            StopTutorial();
            return;
        }

        ExecuteStep(activeSequence.steps[currentStepIndex]);
    }
}