using System.Collections.Generic;
using UnityEngine;

public class LocalConstraints : MonoBehaviour {

    SegmentFactory segFactory;

    int[] nums = new int[] {};  // delete this
    bool stop = false; // delete this

    private void Initialize()
    {
        segFactory = GetComponent<SegmentFactory>();
    }

    public bool Check(Road road, RoadMap roadMap)
    {
        if (stop) return false; // delete this
        foreach (int i in nums)
        {
            if (road.id == i)
            {
                print(road.id);
            }
        }

        if (segFactory == null) Initialize();

        int actionPriority = 0;
        Vector3 finalCrossingIntersection = Vector3.zero;
        Vector3 finalEdgeIntersection = Vector3.zero;
        Vector3 finalNewIntersection = Vector3.zero;
        Road finalOtherRoad = null;
        Junction finalOtherJunction = null;

        // cut off road if outside map bounds

        bool intersects;
        Vector3 edgeIntersection = InterestsMapBoundary(road, out intersects);
        if (intersects)
        {
            actionPriority = 3;
            if (finalEdgeIntersection == Vector3.zero ||
                Vector3.Distance(road.start, edgeIntersection) < Vector3.Distance(road.start, finalEdgeIntersection))
            {
                finalEdgeIntersection = edgeIntersection;
            }
        }

        Rect searchBounds = GetSearchBounds(road);
        List<Segment> matches = roadMap.QuadTree.Query(road.Bounds);

        foreach (Segment other in matches)
        {
            if (other.GetType() == typeof(Road))
            {

                // check for intersecting roads

                Road otherRoad = (Road)other;

                if (actionPriority <= 4)
                {

                    bool found;
                    Vector3 crossingIntersection = DoRoadsIntersect(road, otherRoad, out found);
                    if (!(road.Parent.id == otherRoad.id) && !(road.Parent.id == otherRoad.Parent.id))
                    {
                        if (found)
                        {
                            actionPriority = 4;

                            if (ExceedsMinimumIntersectionAngle(road, otherRoad))
                            {
                                if (finalCrossingIntersection == Vector3.zero ||
                                    Vector3.Distance(road.start, crossingIntersection) < Vector3.Distance(road.start, finalCrossingIntersection))
                                {
                                    finalCrossingIntersection = crossingIntersection;
                                    finalOtherRoad = otherRoad;
                                }
                            }
                            else
                            {
                                if (CityConfig.SHOW_FAILED_JUNCTIONS)
                                {
                                    Junction j = segFactory.CreateJunction(road.start, Quaternion.identity);
                                    j.SetColor(Color.red);
                                }

                                return false;
                            }
                        }
                    }
                }

                if (actionPriority <= 1)
                {

                    // check for potential crossings within snap distance

                    Vector3 newIntersection = NearestPointOnLine(otherRoad.start, otherRoad.end - otherRoad.start, road.end);
                    if (Vector3.Distance(newIntersection, road.end) < CityConfig.ROAD_SNAP_DISTANCE)
                    {
                        actionPriority = 1;

                        if (ExceedsMinimumIntersectionAngle(road, otherRoad))
                        {
                            if (finalNewIntersection == Vector3.zero ||
                                Vector3.Distance(road.start, newIntersection) < Vector3.Distance(road.start, finalNewIntersection))
                            {
                                finalNewIntersection = newIntersection;
                                finalOtherRoad = otherRoad;
                            }
                        }
                        else
                        {
                            if (CityConfig.SHOW_FAILED_JUNCTIONS)
                            {
                                Junction j = segFactory.CreateJunction(road.start, Quaternion.identity);
                                j.SetColor(Color.red);
                            }

                            return false;
                        }
                    }
                }
            }
            else if (actionPriority <= 2 && other.GetType() == typeof(Junction))
            {
                Junction otherJunction = (Junction) other;

                // check for existing crossings within snap distance

                if (!road.attachedSegments.Contains(otherJunction) &&
                    Vector3.Distance(otherJunction.transform.localPosition, road.end) < CityConfig.ROAD_SNAP_DISTANCE
                    )
                {
                    actionPriority = 2;
                    if (finalOtherJunction == null ||

                        Vector3.Distance(road.start, otherJunction.transform.position) < Vector3.Distance(road.start, finalOtherJunction.transform.position))
                    {
                        finalOtherJunction = otherJunction;
                    }
                }
            }



        }

        if (actionPriority == 4)
        {
            MakeRoadIntersection(road, finalOtherRoad, finalCrossingIntersection, roadMap);

        } else if (actionPriority == 3)
        {
            MakeEdgeIntersection(road, finalEdgeIntersection);

        } else if (actionPriority == 2)
        {
            SnapToExistingJunction(road, finalOtherJunction);

        } else if (actionPriority == 1)
        {
            SnapToNewJunction(road, finalOtherRoad, finalNewIntersection, roadMap);

        }

        return true;
    }

    void MakeRoadIntersection(Road road, Road otherRoad, Vector3 intersection, RoadMap roadMap)
    {
        Junction j = segFactory.CreateJunction(intersection, Quaternion.identity);
        j.SetColor(Color.magenta);
        road.attachedSegments.Add(j);
        road.severed = true;
        otherRoad.severed = true;
        road.MoveEnd(intersection);

        SetUpNewIntersection(road, otherRoad, intersection, j, roadMap);
    }

    void MakeEdgeIntersection(Road road, Vector3 intersection)
    {
        road.MoveEnd(intersection);
        road.severed = true;
        Junction j = segFactory.CreateJunction(intersection, Quaternion.identity);
        j.SetColor(Color.white);
    }

    void SnapToExistingJunction(Road road, Junction otherJunction)
    {
        otherJunction.SetColor(Color.blue);
        road.MoveEnd(otherJunction.transform.localPosition);
        road.severed = true;

        // set up links between roads
        foreach (Road r in otherJunction.outgoing)
        {
            r.prev.Add(new Road.Neighbour(road, true));
            road.next.Add(new Road.Neighbour(r, true));
        }
        foreach (Road r in otherJunction.incoming)
        {
            r.next.Add(new Road.Neighbour(road, false));
            road.next.Add(new Road.Neighbour(r, false));
        }
        otherJunction.AddIncoming(road);
    }

    void SnapToNewJunction(Road road, Road otherRoad, Vector3 intersection, RoadMap roadMap)
    {
        Junction j = segFactory.CreateJunction(intersection, Quaternion.identity);
        j.SetColor(Color.yellow);
        road.attachedSegments.Add(j);
        road.MoveEnd(intersection);
        road.severed = true;
        otherRoad.severed = true;

        SetUpNewIntersection(road, otherRoad, intersection, j, roadMap);
    }

    bool ExceedsMinimumIntersectionAngle(Road a, Road b)
    {
        float angleDiff = Mathf.Abs(Mathf.DeltaAngle(a.transform.localRotation.eulerAngles.y, b.transform.localRotation.eulerAngles.y));
        float reverseAngleDiff = Mathf.Abs(Mathf.DeltaAngle(a.transform.localRotation.eulerAngles.y + 180, b.transform.localRotation.eulerAngles.y));
        return angleDiff > CityConfig.MIN_INTERSECTION_ANGLE && reverseAngleDiff > CityConfig.MIN_INTERSECTION_ANGLE;
    }

    void SetUpNewIntersection(Road road, Road otherRoad, Vector3 intersection, Junction j, RoadMap map)
    {


        // split road that is being intersected
        Road newRoad = segFactory.CreateRoad(intersection, otherRoad.end, otherRoad.t, otherRoad.type);
        otherRoad.MoveEnd(intersection);
        /*if (newRoad.id == 12364)
        {
            print("road: " + road.id);
            print("otherroad: " + otherRoad.id);
            foreach (Road.Neighbour n in otherRoad.next) {
                print("othernext: " + n.r.id);
                foreach (Road.Neighbour np in n.r.prev)
                {
                    print("othernextprev: " + np.r.id);
                }
                foreach (Road.Neighbour np in n.r.next)
                {
                    print("othernextnext: " + np.r.id);
                }
            }
        }*/
        // set up links between roads
        newRoad.next.AddRange(otherRoad.next);
        newRoad.Parent = otherRoad;
        newRoad.prev.Add(new Road.Neighbour(road, true));
        road.next.Add(new Road.Neighbour(newRoad, true));
        road.next.Add(new Road.Neighbour(otherRoad, false));
        foreach (Road.Neighbour n in otherRoad.next)
        {
            n.r.ReplaceNeighbour(otherRoad, newRoad);
            //n.r.RemoveNeighbour(otherRoad);
            //n.r.prev.Add(new Road.Neighbour(newRoad, true));
        }

        /*if (newRoad.id == 12364)
        {
            print("road: " + road.id);
            print("otherroad: " + otherRoad.id);
            foreach (Road.Neighbour n in otherRoad.next)
            {
                print("othernext: " + n.r.id);
                foreach (Road.Neighbour np in n.r.prev)
                {
                    print("othernextprev: " + np.r.id);
                }
                foreach (Road.Neighbour np in n.r.next)
                {
                    print("othernextnext: " + np.r.id);
                }
            }
        }*/

        otherRoad.next.Clear();
        otherRoad.next.Add(new Road.Neighbour(newRoad, true));
        otherRoad.next.Add(new Road.Neighbour(road, false));
        j.AddIncoming(road);
        j.AddOutgoing(newRoad);
        j.AddIncoming(otherRoad);
        map.AddRoad(newRoad);


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
        if (found && a.Bounds.Contains(p) && b.Bounds.Contains(p))
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
