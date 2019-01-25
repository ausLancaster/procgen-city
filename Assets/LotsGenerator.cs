
using System;
using System.Collections.Generic;
using UnityEngine;

public class LotsGenerator : MonoBehaviour {

    [SerializeField]
    Lot lotPrefab;

    public List<Lot> Generate(List<Road> allRoads)
    {
        foreach (Road r in allRoads)
        {
            if (r.id == 5038)
            {
                List<Vector3> corners = new List<Vector3>();
                if (SearchForLot(r, r, r, true, corners, false))
                {
                    Lot lot = Instantiate(lotPrefab);
                    lot.Initialize(corners);
                }/*
                if (SearchForLot(r, r, r, false, corners, false))
                {
                    Lot lot = Instantiate(lotPrefab);
                    lot.Initialize(corners);
                }*/
            }
            if (r.id == 5221) r.SetColor(Color.green);

        }
        return new List<Lot>();
    }

    bool SearchForLot(Road original, Road current, Road prev, bool forward, List<Vector3> corners, bool convex)
    {
        print("---" + current.id + " " + forward);
        if (forward)
        {
            corners.Add(current.end);
        } else
        {
            corners.Add(current.start);
        }
        if (corners.Count >= 20) return false;

        // dead end: failed search
        if (current.next.Count == 0) return false;

        // choose the neighbour with the smallest angle
        List<Road.Neighbour> neighbours = forward ? current.next : current.prev;
        string str = forward ? "next" : "prev";
        str += ": ";
        float minAngle = Mathf.Infinity;
        Road.Neighbour next = null;
        foreach (Road.Neighbour neighbour in neighbours)
        {
            str += neighbour.r.id.ToString();
            str += neighbour.sameDirection? "same" : "notsame";
            str += ", ";
            float angle = GetAngle(
                current.transform.localRotation,
                neighbour.r.transform.localRotation,
                forward,
                neighbour.sameDirection
                );
            print("result:" + angle);
            if (angle < minAngle)
            {
                minAngle = angle;
                next = neighbour;
            }
        }
        print(str);
        /*if (next == null)
        {
            foreach (Road.Neighbour neighbour in current.neighbours)
                print(neighbour.r.id);
            current.SetColor(Color.green);
        }*/
        if (next.travelled) return false;

        // mark link as travelled for future searches
        next.travelled = true;

        // angle is convex: failed search (but continue)
        if (minAngle > 180) convex = true;

        // if next is same as original, we have successfully completed a loop
        if (next.r.id == original.id)
            return !convex;

        return SearchForLot(original, next.r, current, next.sameDirection == forward, corners, convex);
    }

    float GetAngle(Quaternion a, Quaternion b, bool forward, bool sameDirection)
    {
        //print("a:" + a.eulerAngles.y + " b:" + b.eulerAngles.y);
        if (sameDirection)
        {
            if (forward)
            {
                return Quaternion.Angle(Get180(a),  b);
            } else
            {
                //print(a.eulerAngles);
                //print(Get180(b).eulerAngles);
                return Quaternion.Angle(a, Get180(b));//
            }
        } else
        {
            if (forward)
            {
                return Quaternion.Angle(Get180(a), Get180(b));
            } else
            {
                return Quaternion.Angle(a, b);
            }
        }
    }

    Quaternion Get180(Quaternion q)
    {
        return q * Quaternion.Euler(180f, 180f, 0);
    }
}
