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
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine(force));
    }

    private IEnumerator ShakeRoutine(float force)
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float strength = shakeCurve.Evaluate(elapsed / shakeDuration) * force;
            transform.localPosition = _originalPosition + (Vector3)Random.insideUnitCircle * strength;
            yield return null;
        }
        transform.localPosition = _originalPosition;
    }

}
