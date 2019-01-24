﻿using System.Collections.Generic;
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
                }

                // check for potential crossings within snap distance

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

                    // set up links between roads
                }
            }



        }

        return true;
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
}
