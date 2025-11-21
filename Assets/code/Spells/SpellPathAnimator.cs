using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SpellPathAnimator : MonoBehaviour
{
    [Header("Spell Animation Settings")]
    public float elementScale = 0.8f;
    public float elementSpacing = 0.1f;
    public Ease movementEase = Ease.OutCubic;

    [Header("References")]
    public GameObject clonePrefab;

    private CameraShake shake;

    public SpellPathDate spellPathData;
    private List<GameObject> activeClones = new List<GameObject>();

    void Start()
    {
        shake = Camera.main.GetComponent<CameraShake>();
    }

    public void OnSpellCast(List<Slot> pathSlots, Spell spell)
    {
        if (spellPathData == null || spellPathData.pathSections.Length == 0) return;

        StartCoroutine(AnimateSpellPath(pathSlots, spell));
    }

    IEnumerator AnimateSpellPath(List<Slot> pathSlots, Spell spell)
    {
        if (pathSlots.Count == 0) yield break;

        List<GameObject> clones = new List<GameObject>();

        foreach (Slot slot in pathSlots)
        {
            if (slot.CurrentElement != null)
            {
                GameObject clone = CreateClone(slot.CurrentElement);
                clone.transform.position = GetSlotWorldPosition(slot);
                clones.Add(clone);
                activeClones.Add(clone);
            }
        }

        if (clones.Count == 0) yield break;

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
    }

    IEnumerator AnimateSingleClone(GameObject clone, float startDelay)
    {
        yield return new WaitForSeconds(startDelay);

        Sequence animationSequence = DOTween.Sequence();

        foreach (PathSection section in spellPathData.pathSections)
        {
            List<Transform> waypoints = section.GetProcessedWaypoints();

            if (waypoints.Count == 0) continue;

            if (section.mode == SpellPathMode.CurvedLinear)
            {
                Vector3[] pathPositions = new Vector3[waypoints.Count];
                for (int i = 0; i < waypoints.Count; i++)
                {
                    pathPositions[i] = waypoints[i].position;
                }

                animationSequence.Append(
                    clone.transform.DOPath(
                        pathPositions,
                        section.duration,
                        PathType.CatmullRom
                    )
                    .SetEase(section.animCurve)
                );
            }
            else if (section.mode == SpellPathMode.Loop)
            {
                if (waypoints.Count >= 2)
                {
                    Vector3 startPoint = waypoints[0].position;
                    Vector3 endPoint = waypoints[1].position;

                    // Calculate center point between start and end
                    Vector3 center = (startPoint + endPoint) / 2f;

                    // Calculate radius as half the distance between points
                    float radius = Vector3.Distance(startPoint, endPoint) / 2f;

                    // Calculate the angle of the line connecting the two points
                    Vector3 toEnd = endPoint - center;
                    float startAngle = Mathf.Atan2(toEnd.y, toEnd.x);

                    // Create circular path starting from waypoint[0] and ending at waypoint[1]
                    Vector3[] circlePoints = CreateCircularPathBetweenPoints(center, radius, startAngle, 8);

                    animationSequence.Append(
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
                    Vector3 center = waypoints[0].position;
                    Vector3[] circlePoints = CreateCircularPath(center, 1.2f, 8);

                    animationSequence.Append(
                        clone.transform.DOPath(
                            circlePoints,
                            section.duration,
                            PathType.CatmullRom
                        )
                        .SetEase(section.animCurve)
                    );
                }
            }
        }

        animationSequence.OnComplete(() =>
        {
            shake.Shake(0.25f);

            SpriteRenderer sr = clone.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.DOFade(0f, 0.3f).OnComplete(() => {
                    if (clone != null) Destroy(clone);
                });
            }
            else
            {
                if (clone != null) Destroy(clone);
            }
        });

        animationSequence.Play();
    }

    Vector3[] CreateCircularPath(Vector3 center, float radius, int segments)
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

    Vector3[] CreateCircularPathBetweenPoints(Vector3 center, float radius, float startAngle, int segments)
    {
        Vector3[] points = new Vector3[segments + 1];

        // Create a full loop starting from startAngle
        for (int i = 0; i <= segments; i++)
        {
            // Go full 360 degrees (2pi radians) to complete the loop
            float angle = startAngle + (i * (360f / segments) * Mathf.Deg2Rad);
            float x = center.x + Mathf.Cos(angle) * radius;
            float y = center.y + Mathf.Sin(angle) * radius;
            points[i] = new Vector3(x, y, center.z);
        }

        return points;
    }

    GameObject CreateClone(Element element)
    {
        GameObject clone;

        if (clonePrefab == null) return null;


        clone = Instantiate(clonePrefab);

        SpriteRenderer renderer = clone.GetComponent<SpriteRenderer>();
        if (renderer != null && element.Sprite != null)
        {
            renderer.sprite = element.Sprite;
            renderer.color = element.Color;
        }

        clone.transform.localScale = Vector3.one * elementScale;
        return clone;
    }

    Vector3 GetSlotWorldPosition(Slot slot)
    {
        if (slot.Holder != null)
        {
            return slot.Holder.transform.position;
        }
        return Vector3.zero;
    }

    float CalculateTotalAnimationDuration()
    {
        float total = 0f;
        foreach (PathSection section in spellPathData.pathSections)
        {
            total += section.duration;
        }
        return total;
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

    void OnDestroy()
    {
        CleanupAllAnimations();
    }
}