using System.Collections;
using UnityEngine;

public class SpellTimer
{
    public float delaySeconds;
    public float countdownSeconds;

    private Coroutine activeCoroutine;
    private Grid grid;
    private System.Action cachedOnDelayComplete;
    private System.Action cachedOnCountdownExpired;

    private float delayElapsed;
    private float countdownElapsed;
    private bool delayDone;
    private bool isPaused;

    public float CountdownSeconds => countdownSeconds;
    public bool IsPaused => isPaused;

    public SpellTimer(float delay, float countdown)
    {
        delaySeconds = delay;
        countdownSeconds = countdown;
    }

    public void Start(Grid grid, System.Action onDelayComplete, System.Action onCountdownExpired)
    {
        if (grid == null) return;

        Stop(grid);

        this.grid = grid;
        cachedOnDelayComplete = onDelayComplete;
        cachedOnCountdownExpired = onCountdownExpired;
        delayElapsed = 0f;
        countdownElapsed = 0f;
        delayDone = false;
        isPaused = false;

        activeCoroutine = grid.StartCoroutine(TimerRoutine());
    }

    public void Stop(Grid grid)
    {
        if (activeCoroutine != null && grid != null)
        {
            grid.StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }
    }

    public void Pause(Grid grid)
    {
        if (isPaused) return;
        isPaused = true;

        if (activeCoroutine != null && grid != null)
        {
            grid.StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }
    }

    public void Resume(Grid grid)
    {
        if (!isPaused) return;
        isPaused = false;

        if (grid == null) return;
        activeCoroutine = grid.StartCoroutine(TimerRoutine());
    }

    private IEnumerator TimerRoutine()
    {
        // Delay phase — continue from delayElapsed
        while (delayElapsed < delaySeconds)
        {
            if (grid == null) yield break;
            delayElapsed += Time.deltaTime;
            yield return null;
        }

        if (grid == null) yield break;

        if (!delayDone)
        {
            delayDone = true;
            cachedOnDelayComplete?.Invoke();
        }

        // Countdown phase — continue from countdownElapsed
        while (countdownElapsed < countdownSeconds)
        {
            if (grid == null) yield break;
            countdownElapsed += Time.deltaTime;
            yield return null;
        }

        activeCoroutine = null;
        cachedOnCountdownExpired?.Invoke();
    }
}