using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SpellPathAnimator : MonoBehaviour
{
    [Header("Spell Animation Settings")]
    [SerializeField] private float elementScale = 0.8f;
    [SerializeField] private float elementSpacing = 0.1f;
    [SerializeField] private Ease movementEase = Ease.OutCubic;

    [Header("References")]
    [SerializeField] private GameObject clonePrefab;
    [SerializeField] private SpellPathDate spellPathData;
    [SerializeField] private Grid grid;

    private CameraShake cameraShake;
    private List<GameObject> activeClones = new List<GameObject>();


    private void Start()
    {
        cameraShake = Camera.main != null ? Camera.main.GetComponent<CameraShake>() : null;

        if (grid == null)
        {
            grid = FindFirstObjectByType<Grid>();
            if (grid == null)
            {
                Debug.LogWarning("Grid reference is null. Assign it in the inspector or call Initialize(grid) before casting spells.");
            }
            else
            {
                Debug.Log("Grid auto-assigned from scene (FindObjectOfType<Grid>())");
            }
        }
    }

    private void OnDestroy()
    {
        CleanupAllAnimations();
    }

    public void Initialize(Grid gridController)
    {
        grid = gridController;
    }

    public void OnSpellCast(Vector2Int[] pathPositions, Spell spell, System.Action onComplete = null)
    {
        if (pathPositions == null || pathPositions.Length == 0)
        {
            onComplete?.Invoke();
            return;
        }

        if (spellPathData == null || spellPathData.pathSections.Length == 0)
        {
            onComplete?.Invoke();
            return;
        }

        if (grid == null)
        {
            Debug.LogError("[SpellPathAnimator] Cannot cast spell: Grid reference is null");
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(AnimateSpellPath(pathPositions, spell, onComplete));
    }

   
    public void CleanupAllAnimations()
    {
        StopAllCoroutines();

        foreach (GameObject clone in activeClones)
        {
            if (clone != null)
            {
                Destroy(clone);
            }
        }
        activeClones.Clear();
    }


    private IEnumerator AnimateSpellPath(Vector2Int[] pathPositions, Spell spell, System.Action onComplete)
    {
        List<GameObject> clones = CreateClones(pathPositions);

        if (clones.Count == 0)
        {
            onComplete?.Invoke();
            yield break;
        }

        for (int i = 0; i < clones.Count; i++)
        {
            StartCoroutine(AnimateSingleClone(clones[i], i * elementSpacing));
        }

        float totalDuration = CalculateTotalAnimationDuration();
        yield return new WaitForSeconds(totalDuration + (clones.Count * elementSpacing));

        foreach (GameObject clone in clones)
        {
            if (clone != null)
            {
                Destroy(clone);
            }
        }

        activeClones.RemoveAll(obj => obj == null);
        onComplete?.Invoke();
    }

    private IEnumerator AnimateSingleClone(GameObject clone, float startDelay)
    {
        yield return new WaitForSeconds(startDelay);

        Sequence animationSequence = DOTween.Sequence();

        foreach (PathSection section in spellPathData.pathSections)
        {
            List<Transform> waypoints = section.GetProcessedWaypoints();

            if (waypoints.Count == 0)
                continue;

            switch (section.mode)
            {
                case SpellPathMode.CurvedLinear:
                    AnimateCurvedPath(animationSequence, clone, waypoints, section);
                    break;

                case SpellPathMode.Loop:
                    AnimateLoopPath(animationSequence, clone, waypoints, section);
                    break;
            }
        }

        animationSequence.OnComplete(() => OnCloneAnimationComplete(clone));
        animationSequence.Play();
    }

    private void AnimateCurvedPath(Sequence sequence, GameObject clone, List<Transform> waypoints, PathSection section)
    {
        Vector3[] pathPositions = new Vector3[waypoints.Count];
        for (int i = 0; i < waypoints.Count; i++)
        {
            pathPositions[i] = waypoints[i].position;
        }

        sequence.Append(
            clone.transform.DOPath(
                pathPositions,
                section.duration,
                PathType.CatmullRom
            )
            .SetEase(section.animCurve)
        );
    }

    private void AnimateLoopPath(Sequence sequence, GameObject clone, List<Transform> waypoints, PathSection section)
    {
        if (waypoints.Count >= 2)
        {
            // Loop between two points
            Vector3 startPoint = waypoints[0].position;
            Vector3 endPoint = waypoints[1].position;
            Vector3 center = (startPoint + endPoint) / 2f;
            float radius = Vector3.Distance(startPoint, endPoint) / 2f;

            Vector3 toEnd = endPoint - center;
            float startAngle = Mathf.Atan2(toEnd.y, toEnd.x);

            Vector3[] circlePoints = CreateCircularPathBetweenPoints(center, radius, startAngle, 8);

            sequence.Append(
                clone.transform.DOPath(
                    circlePoints,
                    section.duration,
                    PathType.CatmullRom
                )
                .SetEase(section.animCurve)
            );
        }
        else if (waypoints.Count == 1)
        {
            // Loop around a single point
            Vector3 center = waypoints[0].position;
            Vector3[] circlePoints = CreateCircularPath(center, 1.2f, 8);

            sequence.Append(
                clone.transform.DOPath(
                    circlePoints,
                    section.duration,
                    PathType.CatmullRom
                )
                .SetEase(section.animCurve)
            );
        }
    }

    private void OnCloneAnimationComplete(GameObject clone)
    {
        // Shake camera
        if (cameraShake != null)
        {
            cameraShake.Shake(0.25f);
        }

        // Fade out
        SpriteRenderer sr = clone.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.DOFade(0f, 0.3f).OnComplete(() =>
            {
                if (clone != null)
                    Destroy(clone);
            });
        }
        else
        {
            if (clone != null)
                Destroy(clone);
        }
    }

    private List<GameObject> CreateClones(Vector2Int[] pathPositions)
    {
        List<GameObject> clones = new List<GameObject>();

        if (pathPositions == null || pathPositions.Length == 0)
            return clones;

        if (grid == null)
        {
            Debug.LogError("CreateClones called but Grid reference is null. No clones will be created.");
            return clones;
        }

        foreach (Vector2Int pos in pathPositions)
        {
            Element element = grid.GetElementAt(pos);
            if (element != null)
            {
                GameObject clone = CreateClone(element);
                if (clone != null)
                {
                    Vector3 worldPos = GetWorldPosition(pos);
                    clone.transform.position = worldPos;
                    clones.Add(clone);
                    activeClones.Add(clone);
                }
            }
        }

        return clones;
    }

    private GameObject CreateClone(Element element)
    {
        if (clonePrefab == null)
            return null;

        GameObject clone = Instantiate(clonePrefab);

        SpriteRenderer renderer = clone.GetComponent<SpriteRenderer>();
        if (renderer != null && element.Sprite != null)
        {
            renderer.sprite = element.Sprite;
            renderer.color = element.Color;
        }

        clone.transform.localScale = Vector3.one * elementScale;
        return clone;
    }

    private Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        if (grid == null)
            return Vector3.zero;

        ElementVisual visual = grid.GetElementVisualAt(gridPosition);
        if (visual != null)
        {
            return visual.GetWorldPosition();
        }
        return Vector3.zero;
    }

    private Vector3[] CreateCircularPath(Vector3 center, float radius, int segments)
    {
        Vector3[] points = new Vector3[segments + 1];

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * (360f / segments) * Mathf.Deg2Rad;
            float x = center.x + Mathf.Cos(angle) * radius;
            float y = center.y + Mathf.Sin(angle) * radius;
            points[i] = new Vector3(x, y, center.z);
        }

        return points;
    }

    private Vector3[] CreateCircularPathBetweenPoints(Vector3 center, float radius, float startAngle, int segments)
    {
        Vector3[] points = new Vector3[segments + 1];

        for (int i = 0; i <= segments; i++)
        {
            float angle = startAngle + (i * (360f / segments) * Mathf.Deg2Rad);
            float x = center.x + Mathf.Cos(angle) * radius;
            float y = center.y + Mathf.Sin(angle) * radius;
            points[i] = new Vector3(x, y, center.z);
        }

        return points;
    }

    private float CalculateTotalAnimationDuration()
    {
        float total = 0f;
        foreach (PathSection section in spellPathData.pathSections)
        {
            total += section.duration;
        }
        return total;
    }

}