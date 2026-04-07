using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupFader : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private float originalAlpha = 1f;
    public float TargetAlpha = 0f;
    public AnimationCurve TransitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public float Duration = 1f;
    public bool Loop = false;

    [Header("Events")]
    public UnityEvent onReachOrigin;
    public UnityEvent onReachTarget;

    private float timer = 0f;
    private bool movingToTarget = true;
    private bool isPlaying = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        if (canvasGroup != null)
        {
            originalAlpha = canvasGroup.alpha;
        }
    }

    private void Update()
    {
        if (!isPlaying || canvasGroup == null || Duration <= 0f) return;

        // Update time
        if (movingToTarget)
        {
            timer += Time.deltaTime;
        }
        else
        {
            timer -= Time.deltaTime;
        }

        // Update alpha
        float normalizedTime = Mathf.Clamp01(timer / Duration);
        float curveValue = TransitionCurve.Evaluate(normalizedTime);

        canvasGroup.alpha = Mathf.LerpUnclamped(originalAlpha, TargetAlpha, curveValue);

        // Check if reach
        if (movingToTarget && timer >= Duration)
        {
            timer = Duration;
            onReachTarget?.Invoke();

            if (Loop)
            {
                movingToTarget = false;
            }
            else
            {
                isPlaying = false;
            }
        }
        else if (!movingToTarget && timer <= 0f)
        {
            timer = 0f;
            onReachOrigin?.Invoke();

            if (Loop)
            {
                movingToTarget = true;
            }
            else
            {
                isPlaying = false;
            }
        }
    }

    public void Play()
    {
        isPlaying = true;
    }

    public void Stop()
    {
        isPlaying = false;
    }

    public void ToTarget()
    {
        movingToTarget = true;
        isPlaying = true;
    }

    public void ToOrigin()
    {
        movingToTarget = false;
        isPlaying = true;
    }

    public void ResetToOrigin()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = originalAlpha;
        }

        timer = 0f;
        movingToTarget = true;
        isPlaying = false;
    }
}