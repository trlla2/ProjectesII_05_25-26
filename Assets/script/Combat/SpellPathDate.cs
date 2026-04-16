using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpellPathDate : MonoBehaviour
{
    public PathSection[] pathSections;
}

[System.Serializable]
public class PathSection
{
    public SpellPathMode mode;
    public float duration = 1f;
    public AnimationCurve animCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public Transform[] waypoints;

    [Header("Random Position")]
    public bool isRandPos = false;
    public float randomRange = 1f;
    public Vector2 StartPos;

    [Header("Random Loop (requires IsRandPos + Loop mode)")]
    public float minRadius = 0.5f;
    public float maxRadius = 2f;

    [HideInInspector] public float finalRadius = 1.2f;
    [HideInInspector] public bool finalClockwise = false;

    public void ResolveRandomLoop()
    {
        finalRadius   = Random.Range(minRadius, maxRadius);
        finalClockwise = Random.value > 0.5f;
    }

    public List<Transform> GetProcessedWaypoints()
    {
        List<Transform> processed = new List<Transform>();

        if (waypoints != null)
        {
            foreach (Transform t in waypoints)
            {
                if (t == null) continue;

                if (isRandPos)
                {
                    Vector2 offset = Random.insideUnitCircle * randomRange;
                    t.position = StartPos + new Vector2(offset.x, offset.y);
                }

                processed.Add(t);
            }
        }

        return processed;
    }
}

public enum SpellPathMode
{
    CurvedLinear,
    Loop
}