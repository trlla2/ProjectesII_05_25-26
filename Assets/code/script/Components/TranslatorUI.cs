using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TranslatorUI : MonoBehaviour
{
    private RectTransform rectTransform;

    [Header("Time")]
    [SerializeField] private float animationDuration = 1.0f;
    [SerializeField] private float currentTime = 0f;


    [SerializeField] private AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
    [Header("Translations")]
    [SerializeField] private Vector3 displacement = Vector3.zero;
    private Vector3 originPosition;
    [SerializeField] private Vector3 scale = Vector3.zero;
    private Vector3 originScale;
    [SerializeField] private Vector3 rotation = Vector3.zero;
    private Vector3 originRotation;

    private IEnumerator currentAnimation;

    [Header("Animation Triggers")]
    public UnityEvent OnTargetReach;
    public UnityEvent OnOriginReach;
    public UnityEvent<float> OnChange;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        originPosition = rectTransform.localPosition;
        originScale = rectTransform.localScale;
        originRotation = rectTransform.localEulerAngles;
    }


    public void ToOrigin()
    {
        ChangeAnimation(ToOriginAnimation());
    }

    public void ToTarget()
    {
        ChangeAnimation(ToTargetAnimation());
    }

    public void ResetPosition()
    {
        currentTime = 0f;

        SetPositionForCurrentTime();

        currentAnimation = null;
    }

    private void ChangeAnimation(IEnumerator newAnimation)
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        currentAnimation = newAnimation;
        StartCoroutine(currentAnimation);
    }

    private IEnumerator ToTargetAnimation()
    {
        Debug.Log("a");
        while (currentTime < animationDuration)
        {
            currentTime += Time.deltaTime;

            SetPositionForCurrentTime();

            yield return new WaitForEndOfFrame();
        }

        currentTime = animationDuration;

        SetPositionForCurrentTime();

        currentAnimation = null;

        OnTargetReach.Invoke();
    }

    private IEnumerator ToOriginAnimation()
    {
        while (currentTime > 0)
        {
            currentTime -= Time.deltaTime;

            SetPositionForCurrentTime();

            yield return new WaitForEndOfFrame();
        }

        currentTime = 0f;

        SetPositionForCurrentTime();

        currentAnimation = null;

        OnOriginReach.Invoke();
    }

    private void SetPositionForCurrentTime()
    {
        float interpolatedValue = currentTime / animationDuration;

        interpolatedValue = curve.Evaluate(interpolatedValue);
        rectTransform.localPosition = originPosition + (displacement * interpolatedValue); // interpolate position
        rectTransform.localScale = originScale + (scale * interpolatedValue); // interpolate scale
        rectTransform.localEulerAngles = originRotation + (rotation * interpolatedValue); // interpolate rotation
        OnChange.Invoke(interpolatedValue);
    }
}