using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class ScaleAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    public AnimationCurve scaleCurve;

    [Header("Loop Settings")]
    public bool alwaysPlay;
    public float loopStrength = 0.1f;
    public float loopCurveDuration = 1f;
    public float loopSinFrequency = 2f;

    [Header("Target")]
    [SerializeField] private SpriteRenderer targetRenderer;

    private Vector3 baseScale;
    private Coroutine currentRoutine;

    bool Blinking = false;

    private void OnEnable()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<SpriteRenderer>();

        if (targetRenderer != null)
            baseScale = targetRenderer.transform.localScale;

        if (alwaysPlay)
            PlayAlways();
    }

    private void OnDisable()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        if (targetRenderer != null)
            targetRenderer.transform.localScale = baseScale;
    }

    public void PlayScale(float strength, float duration)
    {
        if (alwaysPlay)
        {
            PlayAlways();
            return;
        }

        if(Blinking)
        {
            return;
        }   

        // Restart if an animation is already playing
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(AnimateScale(strength, duration));
    }

    public void PlayAlways()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(AnimateAlways());
    }

    public void PlayDelayBlink(float strength, float frequency, float delayDuration, System.Action onComplete = null)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        Blinking = true;
        currentRoutine = StartCoroutine(AnimateDelayBlink(strength, frequency, delayDuration, onComplete));
    }

    private IEnumerator AnimateScale(float strength, float duration)
    {
        if (targetRenderer == null)
            yield break;

        float time = 0f;
        while (time < duration)
        {
            float normalizedTime = time / duration;
            float curveValue = scaleCurve.Evaluate(normalizedTime);

            targetRenderer.transform.localScale = baseScale * (1f + curveValue * strength);

            time += Time.deltaTime;
            yield return null;
        }

        targetRenderer.transform.localScale = baseScale;
        currentRoutine = null;
    }

    private IEnumerator AnimateAlways()
    {
        if (targetRenderer == null)
        {
            float time = 0f;
            while (true)
            {
                float sinValue = Mathf.Sin(time * Mathf.PI * 2f * loopSinFrequency);

                transform.localScale = baseScale * (1f + sinValue * loopStrength);

                time += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            float time = 0f;

            while (true)
            {
                float sinValue = Mathf.Sin(time * Mathf.PI * 2f * loopSinFrequency);

                targetRenderer.transform.localScale = baseScale * (1f + sinValue * loopStrength);

                time += Time.deltaTime;
                yield return null;
            }
        }
    }

    private IEnumerator AnimateDelayBlink(float strength, float frequency, float delayDuration, System.Action onComplete)
    {
        if (targetRenderer == null)
            yield break;

        float time = 0f;
        float startFrequency = frequency;
        float endFrequency = frequency * 3f;
        float startStrength = strength;
        float endStrength = strength * 2f; 

        while (time < delayDuration)
        {
            float normalizedTime = time / delayDuration;

            float currentFrequency = Mathf.Lerp(startFrequency, endFrequency, normalizedTime * normalizedTime);
            
            float currentStrength = Mathf.Lerp(startStrength, endStrength, normalizedTime * normalizedTime);
            
            float sinValue = Mathf.Sin(time * Mathf.PI * 2f * currentFrequency);
            targetRenderer.transform.localScale = baseScale * (1f + sinValue * currentStrength);

            time += Time.deltaTime;
            yield return null;
        }

        targetRenderer.transform.localScale = baseScale;
        currentRoutine = null;
        Blinking = false;
        onComplete?.Invoke();
    }

    private void Reset()
    {
        targetRenderer = GetComponentInChildren<SpriteRenderer>();

        if (targetRenderer != null)
            baseScale = targetRenderer.transform.localScale;

        if (scaleCurve == null || scaleCurve.length == 0)
            scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        loopStrength = 0.1f;
        loopCurveDuration = 1f;
        loopSinFrequency = 2f;
    }
}