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

    public List<Transform> GetProcessedWaypoints()
    {
        List<Transform> processed = new List<Transform>();

        if (waypoints != null)
        {
            foreach (Transform t in waypoints)
            {
                if (t != null)
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