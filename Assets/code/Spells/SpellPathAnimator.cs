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
                //For loops create a circular motion that starts and ends at a start and end waypoint, so will always have 2 waypoints
                if (waypoints.Count >= 2)
                {
                    
                    Vector3 center = waypoints[0].position;
                    Vector3 radiusPoint = waypoints[1].position;
                    float radius = Vector3.Distance(center, radiusPoint);

                    Vector3[] circlePoints = CreateCircularPath(center, radius, 8);

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
        Vector3[] points = new Vector3[segments + 1]; //+1 to close the loop

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * (360f / segments) * Mathf.Deg2Rad;
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