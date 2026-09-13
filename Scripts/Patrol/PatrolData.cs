using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PatrolData
{
    public List<Transform> waypoints = new List<Transform>();

    [Min(0.1f)] public float waypointCheckDistance = 0.6f;

    public bool pingPong;
}
