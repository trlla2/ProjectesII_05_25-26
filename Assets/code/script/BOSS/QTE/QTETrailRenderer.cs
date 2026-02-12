using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//https://nucleartide.com/linerenderer/ holly grail of linrenderer xdd
public class QTETrailRenderer : MonoBehaviour
{
    [Header("Trail Settings")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float trailWidth = 0.15f;
    [SerializeField] public Color trailColor = new Color(0.6f, 0.3f, 1f, 1f);
    [SerializeField] private Material trailMaterial;
    [SerializeField] private GameObject trailParticlesPrefab;
    [SerializeField] private float minPointDistance = 1f; // Minimum distance between points

    private List<Vector3> committedPoints = new List<Vector3>();
    private GameObject activeTrailParticles;
    private Vector3 currentPreviewPosition;
    private bool hasPreview = false;

    public void Initialize(LineRenderer renderer, float width, Color color, Material material, GameObject particlesPrefab)
    {
        lineRenderer = renderer;
        trailWidth = width;
        trailColor = color;
        trailMaterial = material;
        trailParticlesPrefab = particlesPrefab;

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.startWidth = trailWidth;
            lineRenderer.endWidth = trailWidth;
            lineRenderer.startColor = trailColor;
            lineRenderer.endColor = trailColor;
            if (trailMaterial != null)
                lineRenderer.material = trailMaterial;
        }
    }

    public void AddPoint(Vector3 worldPosition)
    {
        committedPoints.Add(worldPosition);
        UpdateRenderer();

        // Update trail particles
        if (trailParticlesPrefab != null)
        {
            if (activeTrailParticles == null)
            {
                activeTrailParticles = Instantiate(trailParticlesPrefab, worldPosition, Quaternion.identity, transform);
            }
            activeTrailParticles.transform.position = worldPosition;
        }
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
                //Move the preview at minPointDistance to avoid jaggered edges
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
        
        //Always add the preview position if we have committed points and a preview
        if (committedPoints.Count > 0 && hasPreview)
        {
            pointsToRender.Add(currentPreviewPosition);
        }

        lineRenderer.positionCount = pointsToRender.Count;
        for (int i = 0; i < pointsToRender.Count; i++)
        {
            lineRenderer.SetPosition(i, pointsToRender[i]);
        }
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
        hasPreview = false;
        
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
        }

        if (activeTrailParticles != null)
        {
            Destroy(activeTrailParticles);
            activeTrailParticles = null;
        }
    }

    public int GetPointCount()
    {
        return committedPoints.Count;
    }

    public Color GetTrailColor()
    {
        return trailColor;
    }

    internal void SetTrailColor(Color currentColor)
    {
        trailColor = currentColor;
        if (lineRenderer != null)
        {
            lineRenderer.startColor = currentColor;
            lineRenderer.endColor = currentColor;
        }
    }

    public IEnumerator FadeOut(float duration)
    {
        if (lineRenderer == null) yield break;

        float elapsed = 0f;
        Color startColor = lineRenderer.startColor;
        float startWidth = lineRenderer.widthMultiplier;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            
            float alpha = Mathf.Lerp(startColor.a, 0f, t);
            lineRenderer.startColor = new Color(startColor.r, startColor.g, startColor.b, alpha);
            lineRenderer.endColor = new Color(startColor.r, startColor.g, startColor.b, alpha);
            lineRenderer.widthMultiplier = Mathf.Lerp(startWidth, 0f, t);
            
            yield return null;
        }

        Clear();
    }
}