using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class QTECircleMarker : MonoBehaviour
{
    [Header("Circle Settings")]
    [SerializeField] private GameObject circleUIPrefab;
    [SerializeField] private float circlePulseSpeed = 2f;
    [SerializeField] private float circleMinScale = 0.9f;
    [SerializeField] private float circleMaxScale = 1.1f;
    [SerializeField] private float scaleInDuration = 0.3f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("References")]
    [SerializeField] private Canvas qteCanvas;
    [SerializeField] private Camera mainCamera;

    private List<GameObject> activeCircles = new List<GameObject>();
    private List<Coroutine> pulseCoroutines = new List<Coroutine>();
    private bool isActive = false;

    public void Initialize(Canvas canvas, Camera camera, GameObject prefab)
    {
        if (canvas != null)
        {
            qteCanvas = canvas;
        }

        if (camera != null)
        {
            mainCamera = camera;
        }
        else if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        if (prefab != null)
        {
            circleUIPrefab = prefab;
        }
    }

    public void SetActive(bool active)
    {
        isActive = active;
    }

    public GameObject CreateCircle(Vector3 worldPosition, Color color)
    {
        if (circleUIPrefab == null || qteCanvas == null || mainCamera == null)
        {
            Debug.LogError("NUll references!!");
            return null;
        }

        // Instantiate as child of canvas
        GameObject circle = Instantiate(circleUIPrefab, qteCanvas.transform);

        // Convert world position to canvas position
        Vector2 canvasPosition = WorldToCanvasPosition(worldPosition);
        RectTransform rt = circle.GetComponent<RectTransform>();
        if (rt == null)
        {
            rt = circle.AddComponent<RectTransform>();
        }
        
        rt.anchoredPosition = canvasPosition;

        // Setup Image component color
        Image img = circle.GetComponent<Image>();
        if (img != null)
        {
            img.color = new Color(color.r, color.g, color.b, 0.5f);
        }

        // Enter Scale animation
        circle.transform.localScale = Vector3.zero;
        circle.transform.DOScale(Vector3.one, scaleInDuration)
            .SetEase(scaleCurve)
            .SetUpdate(true); // Use unscaled time for paused game

        //Start continuous pulse animation
        Coroutine pulseCoroutine = StartCoroutine(PulseCircle(circle));
        
        activeCircles.Add(circle);
        pulseCoroutines.Add(pulseCoroutine);

        return circle;
    }

    private Vector2 WorldToCanvasPosition(Vector3 worldPosition)
    {
        RectTransform canvasRect = qteCanvas.GetComponent<RectTransform>();
        Vector2 viewportPosition = mainCamera.WorldToViewportPoint(worldPosition);

        //Convert viewport position to canvas position
        Vector2 canvasPosition = new Vector2(
            (viewportPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f),
            (viewportPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f)
        );

        return canvasPosition;
    }

    private IEnumerator PulseCircle(GameObject circle)
    {
        while (circle != null && isActive)
        {
            float t = Mathf.Abs(Mathf.Sin(Time.unscaledTime * circlePulseSpeed));
            float scale = Mathf.Lerp(circleMinScale, circleMaxScale, t);
            circle.transform.localScale = Vector3.one * scale;
            yield return null;
        }
    }

    public void RemoveCircle(int index)
    {
        if (index < 0 || index >= activeCircles.Count)
        {
            return;
        }

        GameObject circle = activeCircles[index];
        if (circle != null)
        {
            Image img = circle.GetComponent<Image>();
            if (img != null)
            {
                img.DOFade(0f, 0.2f).SetUpdate(true);
            }

            circle.transform.DOScale(Vector3.zero, 0.2f)
                .SetUpdate(true)
                .OnComplete(() => Destroy(circle));
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

    private void OnDestroy()
    {
        ClearAll();
    }
}