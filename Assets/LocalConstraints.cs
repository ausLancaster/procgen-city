using System.Collections.Generic;
using UnityEngine;

public class LocalConstraints : MonoBehaviour {

    SegmentFactory segFactory;

    private void Initialize()
    {
        segFactory = GetComponent<SegmentFactory>();
    }

    public bool Check(Road road, RoadMap roadMap)
    {
        if (segFactory == null) Initialize();

        // cut off road if outside map bounds
        bool intersects;
        Vector3 edgeIntersection = InterestsMapBoundary(road, out intersects);
        if (intersects)
        {
            road.MoveEnd(edgeIntersection);
            road.severed = true;
            Junction j = segFactory.CreateJunction(edgeIntersection, Quaternion.identity);
            j.SetColor(Color.white);
        }

        Rect searchBounds = GetSearchBounds(road);
        List<Segment> matches = roadMap.QuadTree.Query(road.Bounds);
        foreach (Segment other in matches)
        {
            if (other.GetType() == typeof(Road))
            {
                Road otherRoad = (Road) other;

                // check for intersecting roads

                bool found;
                Vector3 intersection = DoRoadsIntersect(road, otherRoad, out found);
                float anglediff = Quaternion.Angle(road.transform.localRotation, otherRoad.transform.localRotation);
                anglediff = Mathf.Abs(anglediff);

                if (!road.prev.Contains(otherRoad) && !road.prev.Contains(otherRoad.prev[0]))
                {
                    if (found)
                    {
                        if (anglediff > CityConfig.MIN_INTERSECTION_ANGLE)
                        {
                            Junction j = segFactory.CreateJunction(intersection, Quaternion.identity);
                            j.SetColor(Color.magenta);
                            roadMap.AddJunction(j);
                            road.attachedSegments.Add(j);
                            road.severed = true;
                            road.MoveEnd(intersection);

                            // split road that is being intersected
                            // set up links between roads
                            return true;
                        }
                        else
                        {
                            Junction j = segFactory.CreateJunction(intersection, Quaternion.identity);
                            j.SetColor(Color.red);
                            return false;
                        }

                    }
                    // check for potential crossings within snap distance

                    Vector3 nearestPoint = NearestPointOnLine(otherRoad.start, otherRoad.end - otherRoad.start, road.end);
                    if (Vector3.Distance(nearestPoint, road.end) < CityConfig.ROAD_SNAP_DISTANCE)
                    {
                        Junction j = segFactory.CreateJunction(nearestPoint, Quaternion.identity);
                        j.SetColor(Color.yellow);
                        roadMap.AddJunction(j);
                        road.attachedSegments.Add(j);
                        road.MoveEnd(nearestPoint);
                        road.severed = true;
                        return true;
                    }
                }

          
            }
            else if (other.GetType() == typeof(Junction))
            {
                Junction otherJunction = (Junction) other;

                // check for existing crossings within snap distance

                if (!road.attachedSegments.Contains(otherJunction) &&
                    Vector3.Distance(otherJunction.transform.localPosition, road.end) < CityConfig.ROAD_SNAP_DISTANCE
                    )
                {
                    otherJunction.SetColor(Color.blue);
                    road.MoveEnd(otherJunction.transform.localPosition);
                    road.severed = true;
                    return true;

                    // set up links between roads
                }
            }



        }

        return true;
    }

    Vector3 InterestsMapBoundary(Road r, out bool intersects)
    {
        Vector3 intersection = Vector3.zero;
        if (r.end.x < CityConfig.X_START)
        {
            intersection = GetIntersectionPointCoordinates(
                new Vector2(r.start.x, r.start.z),
                new Vector2(r.end.x, r.end.z),
                new Vector2(CityConfig.X_START, 0),
                new Vector2(CityConfig.X_START, 1),
                out intersects
                );
        } else if (r.end.z < CityConfig.Y_START)
        {
            intersection = GetIntersectionPointCoordinates(
                new Vector2(r.start.x, r.start.z),
                new Vector2(r.end.x, r.end.z),
                new Vector2(0, CityConfig.Y_START),
                new Vector2(1, CityConfig.Y_START),
                out intersects
                );
        } else if (r.end.x > CityConfig.X_START + CityConfig.WIDTH)
        {
            intersection = GetIntersectionPointCoordinates(
                new Vector2(r.start.x, r.start.z),
                new Vector2(r.end.x, r.end.z),
                new Vector2(CityConfig.X_START + CityConfig.WIDTH, 0),
                new Vector2(CityConfig.X_START + CityConfig.WIDTH, 1),
                out intersects
                );
        } else if (r.end.z > CityConfig.Y_START + CityConfig.HEIGHT)
        {
            intersection = GetIntersectionPointCoordinates(
                new Vector2(r.start.x, r.start.z),
                new Vector2(r.end.x, r.end.z),
                new Vector2(0, CityConfig.Y_START + CityConfig.HEIGHT),
                new Vector2(1, CityConfig.Y_START + CityConfig.HEIGHT),
                out intersects
                );
        } else
        {
            intersects = false;
        }
        return new Vector3(intersection.x, 0, intersection.y);
    }

    Rect GetSearchBounds(Road r)
    {
        float minX, minY, maxX, maxY;

        minX = Mathf.Min(r.start.x, r.end.x - CityConfig.ROAD_SNAP_DISTANCE);
        minY = Mathf.Min(r.start.y, r.end.y - CityConfig.ROAD_SNAP_DISTANCE);
        maxX = Mathf.Max(r.start.x, r.end.x + CityConfig.ROAD_SNAP_DISTANCE);
        maxY = Mathf.Max(r.start.y, r.end.y + CityConfig.ROAD_SNAP_DISTANCE);

        return new Rect(
            minX,
            minY,
            maxX - minX,
            maxY - minY
            );
    }

    Vector3 DoRoadsIntersect(Road a, Road b, out bool found)
    {
        Vector2 p = GetIntersectionPointCoordinates(
            new Vector2(a.start.x, a.start.z),
            new Vector2(a.end.x, a.end.z),
            new Vector2(b.start.x, b.start.z),
            new Vector2(b.end.x, b.end.z),
            out found
            );
        if (a.Bounds.Contains(p))
        {
            return new Vector3(p.x, 0, p.y);
        } else
        {
            found = false;
            return Vector3.zero;
        }
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

    public static Vector3 NearestPointOnLine(Vector3 linePnt, Vector3 lineDir, Vector3 pnt)
    {
        lineDir.Normalize();
        var v = pnt - linePnt;
        var d = Vector3.Dot(v, lineDir);
        return linePnt + lineDir * d;
    }
}
