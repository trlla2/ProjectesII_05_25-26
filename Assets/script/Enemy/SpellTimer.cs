using DG.Tweening;
using UnityEngine;

public class SpellTimer
{
    private float totalSeconds;
    private float remainingSeconds;
    private Tween countdownTween; // base the timer on a tween for easy pause/resume and time manipulation, aslo easy integration with visuals
    private bool isPaused;

    public float TotalSeconds => totalSeconds;
    public float RemainingSeconds => remainingSeconds;
    public bool IsPaused => isPaused;
    public bool IsExpired => remainingSeconds <= 0f;

    public event System.Action OnTimerExpired;
    public event System.Action<float> OnTimeAdded; 

    public SpellTimer(float countdown)
    {
        totalSeconds = countdown;
        remainingSeconds = countdown;
    }

    public void Start()
    {
        Stop();
        remainingSeconds = totalSeconds;
        isPaused = false;
        StartTween();
    }

    public void Stop()
    {
        countdownTween?.Kill();
        countdownTween = null;
    }

    public void Pause()
    {
        if (isPaused || countdownTween == null) return;
        isPaused = true;
        countdownTween.Pause();
    }

    public void Resume()
    {
        if (!isPaused || countdownTween == null) return;
        isPaused = false;
        countdownTween.Play();
    }

    public void AddTime(float seconds)
    {
        if (IsExpired) return;

        bool wasPaused = isPaused;
        countdownTween?.Kill();

        remainingSeconds += seconds;
        remainingSeconds = Mathf.Min(remainingSeconds, totalSeconds);

        StartTween();

        if (wasPaused)
            countdownTween.Pause();

        OnTimeAdded?.Invoke(remainingSeconds);
    }

    private void StartTween()
    {
        countdownTween = DOTween.To(
            () => remainingSeconds,
            x => remainingSeconds = x,
            0f,
            remainingSeconds
        )
        .SetEase(Ease.Linear)
        .OnComplete(() =>
        {
            remainingSeconds = 0f;
            OnTimerExpired?.Invoke();
        });
    }

    public float GetNormalizedRemaining() => totalSeconds > 0f ? remainingSeconds / totalSeconds : 0f;
}