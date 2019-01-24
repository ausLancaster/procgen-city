using CSharpQuadTree;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Road : MonoBehaviour, IComparable<Road>, Segment
{

    public Vector3 start { get; set; }
    public Vector3 end { get; set; }
    public float length;
    public event EventHandler BoundsChanged;
    public float t { get; set; }
    public List<Junction> attachedSegments { get; set; }
    public RoadType type { get; set; }
    Rect bounds;
    public List<Road> prev { get; private set; }
    public List<Road> next { get; private set; }
    public bool severed { get; set; }

    private void Awake()
    {
        prev = new List<Road>();
        next = new List<Road>();
        attachedSegments = new List<Junction>();
        severed = false;
    }

    public Rect Bounds
    {
        get
        {
            bounds = UpdateBounds();
            return bounds;
        }
    }
    public int CompareTo(Road other)
    {
        if (this.t < other.t) return -1;
        else if (this.t > other.t) return 1;
        else return 0;
    }

    private void OnDestroy()
    {
        foreach (Junction s in attachedSegments)
        {
            if (s != null) Destroy(s.gameObject);
        }
    }

    public void SetColor(Color col)
    {
        GetComponent<MeshRenderer>().material.color = col;
    }

    public void MoveEnd(Vector3 intersection)
    {
        end = intersection;
        Vector3 diff = end - start;
        length = diff.magnitude;
        transform.localScale = new Vector3(length, transform.localScale.y, transform.localScale.z);
        transform.localPosition = (start + end) / 2f;
        if (length > 50) print(length);
    }

    public Rect UpdateBounds()
    {
        float minX, maxX, minY, maxY;
        if (start.x < end.x)
        {
            minX = start.x;
            maxX = end.x;
        }
        else
        {
            minX = end.x;
            maxX = start.x;
        }
        if (start.z < end.z)
        {
            minY = start.z;
            maxY = end.z;
        }
        else
        {
            minY = end.z;
            maxY = start.z;
        }
        bounds = new Rect(
            minX,
            minY,
            Mathf.Max(maxX - minX, 0.001f),
            Mathf.Max(maxY - minY, 0.001f)
            );
        return bounds;
    }
}

public enum RoadType { Street, Highway };