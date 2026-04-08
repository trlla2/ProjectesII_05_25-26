using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupFader : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float OriginAlpha = 1f;
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
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }

    private void Update()
    {
        if (!isPlaying || canvasGroup == null || Duration <= 0f) return;

        if (movingToTarget)
        {
            timer += Time.deltaTime;

            if (timer >= Duration)
            {
                timer = Duration;
                UpdateAlpha();

                if (Loop)
                {
                    movingToTarget = false;
                }
                else
                {
                    isPlaying = false;
                }

                onReachTarget?.Invoke();
                return;
            }
        }
        else
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                timer = 0f;
                UpdateAlpha();

                if (Loop)
                {
                    movingToTarget = true;
                }
                else
                {
                    isPlaying = false;
                }

                onReachOrigin?.Invoke();
                return;
            }
        }

        UpdateAlpha();
    }

    private void UpdateAlpha()
    {
        float normalizedTime = Mathf.Clamp01(timer / Duration);
        float curveValue = TransitionCurve.Evaluate(normalizedTime);
        canvasGroup.alpha = Mathf.LerpUnclamped(OriginAlpha, TargetAlpha, curveValue);
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
        if (timer >= Duration) timer = 0f;
        movingToTarget = true;
        isPlaying = true;
    }

    public void ToOrigin()
    {
        if (timer <= 0f) timer = Duration;
        movingToTarget = false;
        isPlaying = true;
    }

    public void ResetToOrigin()
    {
        timer = 0f;
        movingToTarget = true;
        isPlaying = false;
        canvasGroup.alpha = OriginAlpha;
    }
}