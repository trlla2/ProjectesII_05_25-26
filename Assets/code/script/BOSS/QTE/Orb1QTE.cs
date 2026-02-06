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
    [SerializeField] private float successDamagePercent = 0.5f; // 50% boss HP

    private SequentialTracePhase currentPhase;
    private bool isActive = false;

    public List<IQTEPhase> Phases { get; private set; }
    public bool IsActive => isActive;

    public event Action<bool> OnQTEComplete;

    private void Awake()
    {
        Phases = new List<IQTEPhase>();
        
        if (visualController != null)
        {
            visualController.Initialize(grid);
        }
    }

    public void StartQTE()
    {
        if (isActive)
        {
            Debug.LogWarning("QTE already active!");
            return;
        }

        if (grid == null || visualController == null)
        {
            Debug.LogError("Missing required references for QTE!");
            return;
        }

        Start();
    }

    public void Start()
    {
        isActive = true;

        // Show visual effects
        if (visualEffects != null)
        {
            visualEffects.ShowOverlay();
        }

        // Show timer UI
        if (timerUI != null)
        {
            timerUI.StartTimer(sequentialInputTime);
        }

        // Pause game
        Time.timeScale = 0f;

        // Create and start the sequential trace phase
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

    public void Update()
    {
        if (!isActive) return;

        currentPhase?.Update();

        // Update timer UI
        if (timerUI != null && currentPhase != null)
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

        // Hide timer UI
        if (timerUI != null)
        {
            timerUI.HideTimer();
        }

        // Resume game
        Time.timeScale = 1f;

        // Play visual effects
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

        // Apply damage or handle failure
        if (success)
        {
            ApplyDamageToBoss();
        }

        // Notify listeners
        OnQTEComplete?.Invoke(success);
    }

    private void ApplyDamageToBoss()
    {
        GameManager.Instance.AttackBoss(successDamagePercent);
        Debug.Log($"Applying {successDamagePercent * 100}% damage to boss!");
    }

    private void OnDestroy()
    {
        if (currentPhase != null)
        {
            currentPhase.OnPhaseComplete -= HandlePhaseComplete;
        }
    }

    // Public method for external systems to check if QTE is in progress
    public float GetRemainingTime()
    {
        return currentPhase.GetRemainingTime();
    }

    public int GetCurrentProgress()
    {
        return currentPhase.GetCurrentTarget();
    }
}