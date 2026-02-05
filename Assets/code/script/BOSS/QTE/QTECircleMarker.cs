using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class QTECircleMarker : MonoBehaviour
{
    [Header("Circle Settings")]
    [SerializeField] private GameObject circlePrefab;
    [SerializeField] private float circlePulseSpeed = 2f;
    [SerializeField] private float circleMinScale = 0.9f;
    [SerializeField] private float circleMaxScale = 1.1f;
    [SerializeField] private float scaleInDuration = 0.3f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private List<GameObject> activeCircles = new List<GameObject>();
    private List<Coroutine> pulseCoroutines = new List<Coroutine>();
    private bool isActive = false;

    public void Initialize(GameObject prefab, float pulseSpeed, float minScale, float maxScale, float scaleIn, AnimationCurve curve)
    {
        circlePrefab = prefab;
        circlePulseSpeed = pulseSpeed;
        circleMinScale = minScale;
        circleMaxScale = maxScale;
        scaleInDuration = scaleIn;
        scaleCurve = curve;
    }

    public void SetActive(bool active)
    {
        isActive = active;
    }

    public GameObject CreateCircle(Vector3 worldPosition, Color color)
    {
        GameObject circle = circlePrefab != null 
            ? Instantiate(circlePrefab, worldPosition, Quaternion.identity, transform)
            : new GameObject("Circle");

        circle.transform.position = worldPosition;
        circle.transform.localScale = Vector3.zero;

        SpriteRenderer sr = circle.GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = circle.AddComponent<SpriteRenderer>();

        sr.color = new Color(color.r, color.g, color.b, 0.5f);

        // Scale in animation
        circle.transform.DOScale(Vector3.one, scaleInDuration).SetEase(scaleCurve);

        // Start continuous pulse animation
        Coroutine pulseCoroutine = StartCoroutine(PulseCircle(circle));
        
        activeCircles.Add(circle);
        pulseCoroutines.Add(pulseCoroutine);

        return circle;
    }

    private IEnumerator PulseCircle(GameObject circle)
    {
        while (circle != null && isActive)
        {
            float t = Mathf.PingPong(Time.time * circlePulseSpeed, 1f);
            float scale = Mathf.Lerp(circleMinScale, circleMaxScale, t);
            circle.transform.localScale = Vector3.one * scale;
            yield return null;
        }
    }

    public void RemoveCircle(int index)
    {
        if (index < 0 || index >= activeCircles.Count) return;

        GameObject circle = activeCircles[index];
        if (circle != null)
        {
            circle.transform.DOScale(Vector3.zero, 0.2f).OnComplete(() => Destroy(circle));
        }

        if (index < pulseCoroutines.Count && pulseCoroutines[index] != null)
        {
            StopCoroutine(pulseCoroutines[index]);
        }
    }

    public void ClearAll()
    {
        foreach (var coroutine in pulseCoroutines)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }
        pulseCoroutines.Clear();

        foreach (var circle in activeCircles)
        {
            if (circle != null)
                Destroy(circle);
        }
        activeCircles.Clear();
    }

    public int GetCircleCount()
    {
        return activeCircles.Count;
    }
}