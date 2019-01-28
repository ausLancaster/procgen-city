﻿
using System;
using System.Collections.Generic;
using UnityEngine;

public class LotsGenerator : MonoBehaviour {

    [SerializeField]
    Lot lotPrefab;

    bool printDebug = false;


    public List<Lot> Generate(List<Road> allRoads)
    {
        foreach (Road r in allRoads)
        {
            if (r.id == 5087 || true)
            {
                if (r.id == 4251)
                {
                    //printDebug = true;
                } else
                {
                    printDebug = false;
                }
                if (printDebug) print("BEGINFORWARD");
                List<Vector3> corners = new List<Vector3>();
                if (SearchForLot(r, r, r, true, corners, false))
                {
                    Lot lot = Instantiate(lotPrefab);
                    lot.Initialize(corners);
                    if (printDebug) print("found");
                }
                corners = new List<Vector3>();
                if (printDebug) print("BEGINBACKWARD");
                if (SearchForLot(r, r, r, false, corners, false))
                {
                    Lot lot = Instantiate(lotPrefab);
                    lot.Initialize(corners);
                    if (printDebug) print("found");
                }
            }

        }
        return new List<Lot>();
    }

    bool SearchForLot(Road original, Road current, Road prev, bool forward, List<Vector3> corners, bool convex)
    {
        if (printDebug) print("---" + current.id + " " + forward);
        if (forward)
        {
            corners.Add(current.end);
        } else
        {
            corners.Add(current.start);
        }
        if (corners.Count >= 20) return false;

        // choose the neighbour with the smallest angle
        List<Road.Neighbour> neighbours = forward ? current.next : current.prev;

        // dead end: failed search
        if (neighbours.Count == 0)
        {
            if (printDebug) print("dead end");
            return false;
        }

        string str = forward ? "next" : "prev";
        str += ": ";
        float maxAngle = Mathf.NegativeInfinity;
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
            if (printDebug) print("result:" + angle);
            if (angle > maxAngle)
            {
                maxAngle = angle;
                next = neighbour;
            }
        }
        if (printDebug) print(str);
        str = forward ? "prev" : "next";
        str += ": ";
        neighbours = forward ? current.prev : current.next;
        foreach (Road.Neighbour neighbour in neighbours)
        {
            str += neighbour.r.id.ToString();
            str += neighbour.sameDirection ? "same" : "notsame";
            str += ", ";
        }
        if (printDebug) print(str);
        /*if (next == null)
        {
            foreach (Road.Neighbour neighbour in current.neighbours)
                print(neighbour.r.id);
            current.SetColor(Color.green);
        }*/
        if (next.travelled)
        {
            if (printDebug) print(next.r.id + "travelled");
            return false;
        }

        // delete this
        if (next.r.id == 12364 && current.id == 5087 && original.id != 5087)
        {
            print("Stolen by " + original.id);
        }
        // mark link as travelled for future searches
        next.travelled = true;

        // angle is convex: failed search (but continue)
        if (maxAngle < 179.9) convex = true;

        // if next is same as original, we have successfully completed a loop
        if (next.r.id == original.id)
            return !convex;

        return SearchForLot(original, next.r, current, next.sameDirection == forward, corners, convex);
    }

    public static float GetAngle(Quaternion a, Quaternion b, bool forward, bool sameDirection)
    {
        if (sameDirection)
        {
            if (forward)
            {
                float diff = Mathf.DeltaAngle(a.eulerAngles.y + 180, b.eulerAngles.y);
                if (diff < 0) diff += 360;
                return diff;
            } else
            {
                float diff = Mathf.DeltaAngle(a.eulerAngles.y, b.eulerAngles.y + 180);
                if (diff < 0) diff += 360;
                return diff;
            }
        } else
        {
            if (forward)
            {
                float diff = Mathf.DeltaAngle(a.eulerAngles.y + 180, b.eulerAngles.y + 180);
                if (diff < 0) diff += 360;
                return diff;
            } else
            {
                float diff = Mathf.DeltaAngle(a.eulerAngles.y, b.eulerAngles.y);
                if (diff < 0) diff += 360;
                return diff;
            }
        }
    }

    static Quaternion Get180(Quaternion q)
    {
        return q * Quaternion.Euler(0, 180f, 0);
    }
}
