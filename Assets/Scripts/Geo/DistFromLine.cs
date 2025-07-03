using UnityEngine;

public static partial class Geo
{
    public static float DistanceFromLine(Vector3 P, Vector3 L0, Vector3 L1)
    {
        // Calcualtes vectors between the start of the line and the other positions //
        Vector3 lineDirection = L1 - L0;
        Vector3 posDirection = P - L0;

        // Calculates how far along the line is the closest point in the bounds [0, 1] //
        float c1 = Vector3.Dot(lineDirection, posDirection);
        float c2 = Vector3.Dot(lineDirection, lineDirection);
        float t = Mathf.Clamp01(c1 / c2);

        // Calculates the distance of the shortest point //
        Vector3 closestPoint = L0 + t * lineDirection;
        return Vector3.Distance(P, closestPoint);
    }
}
