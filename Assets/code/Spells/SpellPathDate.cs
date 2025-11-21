using System.Collections.Generic;
using UnityEngine;


public enum SpellPathMode
{
    CurvedLinear,
    Loop
}

[System.Serializable]
public class PathSection
{
    public SpellPathMode mode = SpellPathMode.CurvedLinear;
    public AnimationCurve animCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float duration = 1.5f;

    public List<Transform> waypoints = new List<Transform>();
    public List<Transform> GetProcessedWaypoints()
    {
        List<Transform> processed = new List<Transform>(waypoints);

        if (mode == SpellPathMode.Loop && processed.Count >= 2)
        {
            //Si es Loop solo hay 1 waypoint
            processed.Add(processed[0]);
        }

        return processed;
    }
}

public class SpellPathDate : MonoBehaviour
{
    public PathSection[] pathSections;
}
