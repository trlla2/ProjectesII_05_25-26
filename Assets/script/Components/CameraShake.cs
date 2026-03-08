using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    public float shakeDuration = 0.2f;
    public AnimationCurve shakeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private Vector3 _originalPosition;
    void Awake()
    {
        _originalPosition = transform.localPosition;
    }



    public void Shake(float force)
    {
        Shake(force, shakeDuration);
    }

    public void Shake(float force, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine(force, duration));
    }

    private IEnumerator ShakeRoutine(float force, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float strength = shakeCurve.Evaluate(elapsed / duration) * force;
            transform.localPosition = _originalPosition + (Vector3)Random.insideUnitCircle * strength;
            yield return null;
        }
        transform.localPosition = _originalPosition;
    }

}
