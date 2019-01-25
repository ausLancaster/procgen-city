
using System.Collections.Generic;
using UnityEngine;

public class LotsGenerator : MonoBehaviour {

    [SerializeField]
    Lot lotPrefab;

    public List<Lot> Generate(List<Road> allRoads)
    {
        foreach (Road r in allRoads)
        {
            print("begin");
            List<Vector3> corners = new List<Vector3>();
            if (SearchForLot(r, r, r, corners, false)) {
                print("found");
            }
        }
        return new List<Lot>();
    }

    bool SearchForLot(Road original, Road current, Road prev, List<Vector3> corners, bool convex)
    {
        print(current.id);
        corners.Add(current.transform.localPosition);
        if (corners.Count >= 20) return false;

        // dead end: failed search
        if (current.neighbours.Count == 0) return false;

        // choose the neighbour with the smallest angle
        float minAngle = Mathf.Infinity;
        Road.Neighbour next = null;
        foreach (Road.Neighbour neighbour in current.neighbours)
        {
            //print("cmp: " + neighbour.r.id + " " + current.id);
            if (!(neighbour.r.id == prev.id))
            {
                float angle = current.transform.localRotation.eulerAngles.y - neighbour.r.transform.localRotation.eulerAngles.y;
                if (angle < minAngle)
                {
                    minAngle = angle;
                    next = neighbour;
                }
            }
        }

        if (next.travelled) return false;

        // mark link as travelled for future searches
        next.travelled = true;

        // search fails if angle is convex
        if (minAngle > 180) convex = true;

        // if next is same as original, we have successfully completed a loop
        if (next.r.id == original.id) return convex;

        return SearchForLot(original, next.r, current, corners, convex);
    }
}
