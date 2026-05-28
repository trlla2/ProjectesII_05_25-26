using System;
using System.Collections.Generic;
using UnityEngine;

public class Orb2QTE : MonoBehaviour, IQTE
{
    [Header("Sequential Phase Settings")]
    [SerializeField] private int sequentialPointCount = 6;
    [SerializeField] private float sequentialRevealTime = 3f;
    [SerializeField] private float sequentialInputTime = 4f;

    [Header("Timing Phase Settings")]
    [SerializeField] private int timingPointCount = 5;
    [SerializeField] private float timingBeatDuration = 1.2f;
    [SerializeField] private float timingWindowStart = 0.6f;
    [SerializeField] private float timingWindowEnd = 0.95f;

    [Header("References")]
    [SerializeField] private Grid grid;
    [SerializeField] private QTEVisualController visualController;
    [SerializeField] private QTEVisualEffects visualEffects;
    [SerializeField] private QTETimerUI timerUI;
    [SerializeField] private Transform orbTransform;

    [Header("Damage Settings")]
    [SerializeField] private float successDamagePercent = 0.5f;

    private IQTEPhase currentPhase;
    private bool isActive = false;
    private bool isWaitingForSuccessAnimation = false;

    public List<IQTEPhase> Phases { get; private set; }
    public bool IsActive => isActive;

    public event Action<bool> OnQTEComplete;

    public static event Action OnQTEStarted;
    public static event Action OnQTEEnded;

    private void Awake()
    {
        Phases = new List<IQTEPhase>();
        visualController.Initialize(grid);
    }

    private void Update()
    {
        if (isActive)
        {
            update();
        }
    }

    public void StartQTE()
    {
        if (isActive)
        {
            return;
        }

        if (!ValidateReferences())
        {
            Debug.LogError("Cannot start QTE, missing references!");
            return;
        }

        start();
    }

    private bool ValidateReferences()
    {
        return grid != null && visualController != null;
    }

    public void start()
    {
        isActive = true;
        isWaitingForSuccessAnimation = false;

        OnQTEStarted?.Invoke();

        if (visualEffects != null)
        {
            visualEffects.ShowOverlay();
        }
        if (timerUI != null)
        {
            timerUI.StartTimer(sequentialInputTime);
        }

        Time.timeScale = 0f;

        SequentialTracePhase sequentialPhase = new SequentialTracePhase(
            grid,
            visualController,
            sequentialPointCount,
            sequentialRevealTime,
            sequentialInputTime
        );

        sequentialPhase.OnPhaseComplete += HandleSequentialPhaseComplete;
        Phases.Add(sequentialPhase);

        currentPhase = sequentialPhase;
        currentPhase.Enter();
    }

    public void ResetVisuals()
    {
        if (visualController != null)
        {
            visualController.ClearAll();
        }
    }

    public void update()
    {
        if (!isActive || currentPhase == null || isWaitingForSuccessAnimation) return;

        currentPhase.update();

        if (currentPhase is SequentialTracePhase sequentialPhase && timerUI != null)
        {
            timerUI.UpdateTimer(sequentialPhase.GetRemainingTime(), sequentialInputTime);
        }
        else if (currentPhase is TimingPhase timingPhase && timerUI != null)
        {
            float remaining = (1f - timingPhase.GetBeatProgress()) * timingBeatDuration;
            timerUI.UpdateTimer(remaining, timingBeatDuration);
        }
    }

    public void Cancel()
    {
        if (!isActive) return;
        HandleFinalComplete(false);
    }

    private void HandleSequentialPhaseComplete(bool success)
    {
        if (currentPhase != null)
        {
            currentPhase.OnPhaseComplete -= HandleSequentialPhaseComplete;
            currentPhase.Exit();
        }

        if (timerUI != null)
        {
            timerUI.HideTimer();
        }

        if (success)
        {
            StartTimingPhase();
        }
        else
        {
            HandleFinalComplete(false);
        }
    }

    private void StartTimingPhase()
    {
        TimingPhase timingPhase = new TimingPhase(
            grid,
            visualController,
            visualController.GetTimingRingUI(),
            timingPointCount,
            timingBeatDuration,
            timingWindowStart,
            timingWindowEnd
        );

        timingPhase.OnPhaseComplete += HandleTimingPhaseComplete;
        Phases.Add(timingPhase);

        currentPhase = timingPhase;
        currentPhase.Enter();
    }

    private void HandleTimingPhaseComplete(bool success)
    {
        List<Vector2Int> snapshotPoints = null;

        if (currentPhase != null)
        {
            currentPhase.OnPhaseComplete -= HandleTimingPhaseComplete;

            if (success && currentPhase is TimingPhase completedTimingPhase)
            {
                // Snapshot before Exit() clears the list in-place
                snapshotPoints = new List<Vector2Int>(completedTimingPhase.GetTimingPoints());
            }

            currentPhase.Exit();
        }

        // Build trail after Exit() so EndQTE's isActive=false doesn't interfere
        if (snapshotPoints != null)
        {
            visualController.BuildTrailFromPoints(snapshotPoints);
        }

        if (timerUI != null)
        {
            timerUI.HideTimer();
        }

        if (success)
        {
            StartSuccessAnimationPhase();
        }
        else
        {
            HandleFinalComplete(false);
        }
    }

    private void StartSuccessAnimationPhase()
    {
        isWaitingForSuccessAnimation = true;

        // Re-activate visual controller without clearing the trail we just built
        visualController.ResumeQTE();

        visualEffects.PlayChargeAttackEffect();

        Vector3 targetOrbPosition = Vector3.zero;
        if (orbTransform != null)
        {
            targetOrbPosition = orbTransform.position;
        }
        else
        {
            OrbVisual orbVisual = GameManager.Instance.CombatHandler.bossManager.GetCurrentOrbVisual();
            if (orbVisual != null)
            {
                targetOrbPosition = orbVisual.GetPosition();
            }
        }

        TrailSuccessAnimationPhase animPhase = new TrailSuccessAnimationPhase(
            visualController,
            targetOrbPosition,
            this
        );

        animPhase.OnPhaseComplete += HandleSuccessAnimationComplete;
        Phases.Add(animPhase);

        currentPhase = animPhase;
        currentPhase.Enter();
    }

    private void HandleSuccessAnimationComplete(bool success)
    {
        if (currentPhase != null)
        {
            currentPhase.OnPhaseComplete -= HandleSuccessAnimationComplete;
            currentPhase.Exit();
            currentPhase = null;
        }

        isWaitingForSuccessAnimation = false;
        HandleFinalComplete(true);
    }

    private void HandleFinalComplete(bool success)
    {
        isActive = false;
        Phases.Clear();
        currentPhase = null;

        Time.timeScale = 1f;

        OrbVisual currentOrbVisual = GameManager.Instance.CombatHandler.bossManager.GetCurrentOrbVisual();
        if (currentOrbVisual != null)
        {
            currentOrbVisual.fullChargeParticles.Stop();
            OrbAnimationEffects effects = currentOrbVisual.GetComponent<OrbAnimationEffects>();
            if (effects != null)
            {
                effects.KillAllTweens();
            }
        }

        if (visualEffects != null)
        {
            if (success)
            {
                visualEffects.PlaySuccessEffect();
            }
            else
            {
                visualEffects.PlayFailureEffect();
            }

            visualEffects.HideOverlay();
        }

        if (success)
        {
            ApplyDamageToBoss();
        }

        visualController.ClearAll();

        OnQTEEnded?.Invoke();

        OnQTEComplete?.Invoke(success);
    }

    private void ApplyDamageToBoss()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CombatHandler.AttackBoss(successDamagePercent);
        }
    }

    private void OnDestroy()
    {
        if (currentPhase != null)
        {
            currentPhase.OnPhaseComplete -= HandleSequentialPhaseComplete;
            currentPhase.OnPhaseComplete -= HandleTimingPhaseComplete;
            currentPhase.OnPhaseComplete -= HandleSuccessAnimationComplete;
        }

        if (isActive)
        {
            Time.timeScale = 1f;
            OnQTEEnded?.Invoke();
        }
    }
}