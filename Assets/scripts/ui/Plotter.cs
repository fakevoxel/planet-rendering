using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// literally ALL this does is plot points when given an array.
// good for the map view, thats about all right now
public class Plotter : MonoBehaviour
{
    public LineRenderer l;
    public float lineWidth;

    public void Plot(Vector3[] points)
    {
        //temp
        lineWidth = 1;
        l.startWidth = 0.1f;
        l.endWidth = 0.1f;

        l.positionCount = points.Length;
        Vector3[] scaledPoints = new Vector3[points.Length];
        for (int i = 0; i < scaledPoints.Length; i++)
        {
            scaledPoints[i] = points[i] * Sys.mapViewScalingFactor;
        }
        l.SetPositions(scaledPoints);
    }
}
