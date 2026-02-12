using System;
using System.Collections.Generic;
using UnityEngine;

public class Orb1QTE : MonoBehaviour, IQTE
{
    [Header("Phase Settings")]
    [SerializeField] private int sequentialPointCount = 6;
    [SerializeField] private float sequentialRevealTime = 3f;
    [SerializeField] private float sequentialInputTime = 4f;

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
    
    //Global events for QTE state changes (player pathing needs it)
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
            Debug.LogError("Cannot start QTE,,missing  references!");
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

        //Update timer UI only for sequential phase
        if (currentPhase is SequentialTracePhase sequentialPhase && timerUI != null)
        {
            timerUI.UpdateTimer(sequentialPhase.GetRemainingTime(), sequentialInputTime);
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

        Vector3 targetOrbPosition = Vector3.zero;
        if (orbTransform != null)
        {
            targetOrbPosition = orbTransform.position;
        }
        else
        {
            OrbVisual orbVisual = GameManager.Instance.bossManager.GetCurrentOrbVisual();
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

        OnQTEEnded?.Invoke();

        OnQTEComplete?.Invoke(success);
    }

    private void ApplyDamageToBoss()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AttackBoss(successDamagePercent);
        }
    }

    private void OnDestroy()
    {
        if (currentPhase != null)
        {
            currentPhase.OnPhaseComplete -= HandleSequentialPhaseComplete;
            currentPhase.OnPhaseComplete -= HandleSuccessAnimationComplete;
        }

        if (isActive)
        {
            Time.timeScale = 1f;
            OnQTEEnded?.Invoke();
        }
    }
}