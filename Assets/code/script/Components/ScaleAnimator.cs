using UnityEngine;
using System.Collections;

public class ScaleAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    public AnimationCurve scaleCurve;

    [Header("Loop Settings")]
    public bool alwaysPlay;
    public float loopStrength = 0.1f;
    public float loopCurveDuration = 1f;
    public float loopSinFrequency = 2f;

    private Vector3 baseScale;

    private Coroutine currentRoutine;

    private void OnEnable()
    {
        baseScale = transform.localScale;

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

        transform.localScale = baseScale;
    }

    public void PlayScale(float strength, float duration)
    {
        if (alwaysPlay)
        {
            PlayAlways();
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

    private IEnumerator AnimateScale(float strength, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            float normalizedTime = time / duration;
            float curveValue = scaleCurve.Evaluate(normalizedTime);

            transform.localScale = baseScale * (1f + curveValue * strength);

            time += Time.deltaTime;
            yield return null;
        }

        transform.localScale = baseScale;
        currentRoutine = null;
    }

    private IEnumerator AnimateAlways()
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

    private void Reset()
    {
        baseScale = transform.localScale;

        if (scaleCurve == null || scaleCurve.length == 0)
            scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        loopStrength = 0.1f;
        loopCurveDuration = 1f;
        loopSinFrequency = 2f;
    }
}