using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

//https://nucleartide.com/linerenderer/ holly grail of linrenderer xdd
public class QTETrailRenderer : MonoBehaviour
{
    [Header("Trail Settings")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] public Color trailColor = new Color(0.6f, 0.3f, 1f, 1f);
    private Color initialColor;

    [SerializeField] private Material trailMaterial;
    [SerializeField] private GameObject trailParticlesPrefab;
    [SerializeField] private float minPointDistance = 1f;

    private List<Vector3> committedPoints = new List<Vector3>();

    // Local-space offsets set by PrepareForAnimation, used to rebuild world positions each frame
    private List<Vector3> localBasePoints = new List<Vector3>();
    private bool isAnimationMode = false;

    private GameObject activeTrailParticles;
    private Vector3 currentPreviewPosition;
    private bool hasPreview = false;

    private void Start()
    {
        initialColor = trailColor;
    }

    public void AddPoint(Vector3 worldPosition)
    {
        committedPoints.Add(worldPosition);
        UpdateRenderer();

        if (trailParticlesPrefab != null)
        {
            if (activeTrailParticles == null)
            {
                activeTrailParticles = Instantiate(trailParticlesPrefab, worldPosition, Quaternion.identity, transform);
            }
            activeTrailParticles.transform.position = worldPosition;
        }
    }

    public void PrepareForAnimation()
    {
        if (committedPoints.Count == 0) return;

        // Compute centroid in world space
        Vector3 centroid = Vector3.zero;
        foreach (Vector3 p in committedPoints)
            centroid += p;
        centroid /= committedPoints.Count;

        // Move transform to centroid, reset rotation and scale
        transform.position = centroid;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        // Store local offsets relative to centroid
        localBasePoints.Clear();
        foreach (Vector3 p in committedPoints)
            localBasePoints.Add(p - centroid);

        isAnimationMode = true;

        // Push initial world positions to the LineRenderer
        RefreshWorldPositions();
    }

    public void RefreshWorldPositions()
    {
        if (!isAnimationMode || lineRenderer == null) return;

        lineRenderer.positionCount = localBasePoints.Count;
        Matrix4x4 trs = transform.localToWorldMatrix;
        for (int i = 0; i < localBasePoints.Count; i++)
            lineRenderer.SetPosition(i, trs.MultiplyPoint3x4(localBasePoints[i]));
    }

    public void UpdatePreview(Vector3 worldPosition)
    {
        if (committedPoints.Count > 0)
        {
            Vector3 lastPoint = committedPoints[committedPoints.Count - 1];
            float distance = Vector3.Distance(lastPoint, worldPosition);

            if (distance < minPointDistance)
            {
                Vector3 direction = (worldPosition - lastPoint).normalized;
                worldPosition = lastPoint + direction * minPointDistance;
            }
        }

        currentPreviewPosition = worldPosition;
        hasPreview = true;
        UpdateRenderer();
    }

    public void ClearPreview()
    {
        hasPreview = false;
        UpdateRenderer();
    }

    private void UpdateRenderer()
    {
        if (lineRenderer == null) return;

        List<Vector3> pointsToRender = new List<Vector3>(committedPoints);

        if (committedPoints.Count > 0 && hasPreview)
            pointsToRender.Add(currentPreviewPosition);

        lineRenderer.positionCount = pointsToRender.Count;
        for (int i = 0; i < pointsToRender.Count; i++)
            lineRenderer.SetPosition(i, pointsToRender[i]);
    }

    public IEnumerator SolidifyTrail()
    {
        if (lineRenderer == null) yield break;

        float duration = 0.5f;
        float elapsed = 0f;
        Color startColor = trailColor;
        Color endColor = Color.white * 2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            Color currentColor = Color.Lerp(startColor, endColor, t);
            lineRenderer.startColor = currentColor;
            lineRenderer.endColor = currentColor;
            lineRenderer.widthMultiplier = Mathf.Lerp(1f, 1.5f, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);
    }

    public IEnumerator ShatterTrail()
    {
        if (lineRenderer == null) yield break;

        float duration = 0.3f;
        float elapsed = 0f;
        Color startColor = lineRenderer.startColor;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float alpha = Mathf.Lerp(startColor.a, 0f, t);
            lineRenderer.startColor = new Color(startColor.r, startColor.g, startColor.b, alpha);
            lineRenderer.endColor = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Clear();
    }

    public void Clear()
    {
        committedPoints.Clear();
        localBasePoints.Clear();
        isAnimationMode = false;
        hasPreview = false;

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.startColor = initialColor;
            lineRenderer.endColor = initialColor;
            lineRenderer.widthMultiplier = 1f;
        }

        if (activeTrailParticles != null)
        {
            Destroy(activeTrailParticles);
            activeTrailParticles = null;
        }

        transform.position = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;
    }

    public int GetPointCount() => committedPoints.Count;
    public Color GetTrailColor() => trailColor;

    internal void SetTrailColor(Color currentColor)
    {
        trailColor = currentColor;
        if (lineRenderer != null)
        {
            lineRenderer.startColor = currentColor;
            lineRenderer.endColor = currentColor;
        }
    }
}