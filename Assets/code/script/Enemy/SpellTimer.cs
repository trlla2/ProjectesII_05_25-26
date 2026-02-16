using System.Collections;
using UnityEngine;

public class SpellTimer
{
    private float delaySeconds;
    private float countdownSeconds;
    private Coroutine activeCoroutine;

    public float CountdownSeconds => countdownSeconds;

    public SpellTimer(float delay, float countdown)
    {
        delaySeconds = delay;
        countdownSeconds = countdown;
    }

    public void Start(Grid grid, System.Action onDelayComplete, System.Action onCountdownExpired)
    {
        if (grid == null) return;
        Stop(grid);
        activeCoroutine = grid.StartCoroutine(TimerRoutine(grid, onDelayComplete, onCountdownExpired));
    }

    public void Stop(Grid grid)
    {
        if (activeCoroutine != null && grid != null)
        {
            grid.StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }
    }

    private IEnumerator TimerRoutine(Grid grid, System.Action onDelayComplete, System.Action onCountdownExpired)
    {
        //Initial delay
        float elapsed = 0f;
        while (elapsed < delaySeconds)
        {
            if (grid == null) yield break;
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (grid == null) yield break;

        //DONE
        onDelayComplete?.Invoke();

        //Countdown phase
        elapsed = 0f;
        while (elapsed < countdownSeconds)
        {
            if (grid == null) yield break;
            elapsed += Time.deltaTime;
            yield return null;
        }

        activeCoroutine = null;

        // Timer fully expired
        onCountdownExpired?.Invoke();
    }
}