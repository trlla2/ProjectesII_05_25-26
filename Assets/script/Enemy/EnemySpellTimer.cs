using UnityEngine;

//Global timer running in the enemy object, handy if we want a timer that does not resset in grid state changes
public class EnemySpellTimer : MonoBehaviour
{
    private float intervalSeconds;
    private float totalElapsedTime;
    private float intervalElapsedTime;
    private bool isRunning;
    private bool isPaused;
    private int pendingSelections;

    private Grid grid;
    private System.Action onIntervalReached;

    public float TotalElapsedTime => totalElapsedTime;
    public float IntervalSeconds => intervalSeconds;
    public bool IsRunning => isRunning;
    public bool IsPaused => isPaused;

    public void Initialize(float interval, Grid gridRef, System.Action onInterval)
    {
        intervalSeconds = interval;
        grid = gridRef;
        onIntervalReached = onInterval;
        totalElapsedTime = 0f;
        intervalElapsedTime = 0f;
        pendingSelections = 0;
        isRunning = false;
        isPaused = false;
    }

    public void StartTimer()
    {
        isRunning = true;
        isPaused = false;
        totalElapsedTime = 0f;
        intervalElapsedTime = 0f;
        pendingSelections = 0;
    }

    public void StopTimer()
    {
        isRunning = false;
        pendingSelections = 0;
    }

    public void Pause()
    {
        if (!isRunning || isPaused) return;
        isPaused = true;
    }

    public void Resume()
    {
        if (!isRunning || !isPaused) return;
        isPaused = false;
    }

    private void Update()
    {
        if (!isRunning || isPaused) return;

        float dt = Time.deltaTime;
        totalElapsedTime += dt;
        intervalElapsedTime += dt;

        while (intervalElapsedTime >= intervalSeconds)
        {
            intervalElapsedTime -= intervalSeconds;
            pendingSelections++;
        }

        //Only flush pending selections when the grid is Idle
        if (pendingSelections > 0 && grid != null && grid.CurrentState == GridStateType.Idle)
        {
            int count = pendingSelections;
            pendingSelections = 0;
            for (int i = 0; i < count; i++)
            {
                onIntervalReached?.Invoke();
            }
        }
    }
}