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

    [Header("Damage Settings")]
    [SerializeField] private float successDamagePercent = 0.5f;

    private SequentialTracePhase currentPhase;
    private bool isActive = false;

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

        currentPhase = new SequentialTracePhase(
            grid, 
            visualController, 
            sequentialPointCount, 
            sequentialRevealTime, 
            sequentialInputTime
        );

        currentPhase.OnPhaseComplete += HandlePhaseComplete;
        Phases.Add(currentPhase);

        currentPhase.Enter();
    }

    public void update()
    {
        if (!isActive || currentPhase == null) return;

        currentPhase.update();

        //Update timer UI
        if (timerUI != null)
        {
            timerUI.UpdateTimer(currentPhase.GetRemainingTime(), sequentialInputTime);
        }
    }

    public void Cancel()
    {
        if (!isActive) return;

        HandlePhaseComplete(false);
    }

    private void HandlePhaseComplete(bool success)
    {
        if (currentPhase != null)
        {
            currentPhase.OnPhaseComplete -= HandlePhaseComplete;
            currentPhase.Exit();
            currentPhase = null;
        }

        isActive = false;
        Phases.Clear();

        if (timerUI != null)
        {
            timerUI.HideTimer();
        }

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
            currentPhase.OnPhaseComplete -= HandlePhaseComplete;
        }

        if (isActive)
        {
            Time.timeScale = 1f;
            OnQTEEnded?.Invoke();
        }
    }

    //public float GetRemainingTime()
    //{
    //    return currentPhase?.GetRemainingTime() ?? 0f;
    //}

    //public int GetCurrentProgress()
    //{
    //    return currentPhase?.GetCurrentTarget() ?? 0;
    //}
}