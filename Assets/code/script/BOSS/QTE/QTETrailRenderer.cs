using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QTETrailRenderer : MonoBehaviour
{
    [Header("Trail Settings")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float trailWidth = 0.15f;
    [SerializeField] public Color trailColor = new Color(0.6f, 0.3f, 1f, 1f);
    [SerializeField] private Material trailMaterial;
    [SerializeField] private GameObject trailParticlesPrefab;

    private List<Vector3> trailPoints = new List<Vector3>();
    private GameObject activeTrailParticles;

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
        trailPoints.Add(worldPosition);
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
        if (trailPoints.Count == 0) return;

        List<Vector3> previewPoints = new List<Vector3>(trailPoints);
        previewPoints.Add(worldPosition);

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = previewPoints.Count;
            for (int i = 0; i < previewPoints.Count; i++)
            {
                lineRenderer.SetPosition(i, previewPoints[i]);
            }
        }
    }

    private void UpdateRenderer()
    {
        if (lineRenderer == null || trailPoints.Count == 0) return;

        lineRenderer.positionCount = trailPoints.Count;
        for (int i = 0; i < trailPoints.Count; i++)
        {
            lineRenderer.SetPosition(i, trailPoints[i]);
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
        trailPoints.Clear();
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
        return trailPoints.Count;
    }

    public Color GetTrailColor()
    {
        return trailColor;
    }
}