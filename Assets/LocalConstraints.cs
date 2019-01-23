using System.Collections.Generic;
using UnityEngine;

public class LocalConstraints : MonoBehaviour {

    SegmentFactory segFactory;

    private void Initialize()
    {
        segFactory = GetComponent<SegmentFactory>();
    }

    public bool Check(Road road, RoadMap qtree)
    {
        if (segFactory == null) Initialize();

        // check for intersecting roads
        List<Road> matches = qtree.QuadTree.Query(road.Bounds);
        foreach (Road other in matches)
        {
            bool found;
            Vector3 p = doRoadsIntersect(road, other, out found);
            float anglediff = Quaternion.Angle(road.transform.localRotation, other.transform.localRotation);
            anglediff = Mathf.Abs(anglediff);
            if (found && anglediff > CityConfig.MIN_INTERSECTION_ANGLE)
            {
                /*Junction j = segFactory.CreateJunction(p, Quaternion.identity);
                j.gameObject.GetComponent<MeshRenderer>().material.color = Color.magenta;*/
            }
        }

        return true;
    }

    Vector3 doRoadsIntersect(Road a, Road b, out bool found)
    {
        Vector2 p = GetIntersectionPointCoordinates(
            new Vector2(a.start.x, a.start.z),
            new Vector2(a.end.x, a.end.z),
            new Vector2(b.start.x, b.start.z),
            new Vector2(b.end.x, b.end.z),
            out found
            );
        return new Vector3(p.x, 0, p.y);
    }

    public Vector2 GetIntersectionPointCoordinates(Vector2 A1, Vector2 A2, Vector2 B1, Vector2 B2, out bool found)
    {
        float tmp = (B2.x - B1.x) * (A2.y - A1.y) - (B2.y - B1.y) * (A2.x - A1.x);

        if (tmp == 0)
        {
            // No solution!
            found = false;
            return Vector2.zero;
        }

        float mu = ((A1.x - B1.x) * (A2.y - A1.y) - (A1.y - B1.y) * (A2.x - A1.x)) / tmp;

        found = true;

        return new Vector2(
            B1.x + (B2.x - B1.x) * mu,
            B1.y + (B2.y - B1.y) * mu
        );
    }
}
