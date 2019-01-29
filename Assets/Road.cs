using System;
using System.Collections.Generic;
using UnityEngine;

public class Road : MonoBehaviour, IComparable<Road>, Segment
{
    //public int id { get; private set; }
    public int id;
    public Vector3 start { get; private set; }
    public Vector3 end { get; private set; }
    public float length { get; private set; }
    public float t { get; private set; }
    public RoadType type { get; private set; }

    public event EventHandler BoundsChanged;
    public List<Junction> attachedSegments { get; private set; }
    public List<Junction> intersections { get; set; }
    public List<Neighbour> prev { get; private set; }
    public List<Neighbour> next { get; private set; }
    Rect bounds;
    Road parent;
    public bool severed { get; set; }

    public class Neighbour
    {
        public Road r { get; private set; }
        public bool sameDirection { get; private set; }
        public bool travelled { get; set; }

        public Neighbour(Road r, bool sameDirection)
        {
            this.r = r;
            this.sameDirection = sameDirection;
            this.travelled = false;
        }
    }

    public void Initialize(int id, Vector3 start, Vector3 end, float length, float t, RoadType type)
    {
        this.id = id;
        this.start = start;
        this.end = end;
        this.length = length;
        this.t = t;
        this.type = type;
    }

    public Road Parent
    {
        get
        {
            return parent;
        }
        set
        {
            parent = value;
            prev.Add(new Neighbour(value, true));
        }
    }

    private void Awake()
    {
        prev = new List<Neighbour>();
        next = new List<Neighbour>();
        attachedSegments = new List<Junction>();
        intersections = new List<Junction>();
        severed = false;
    }

    public Rect Bounds
    {
        get
        {
            if (bounds == null) UpdateBounds();
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
        if (prev != null)
        {
            foreach (Neighbour n in prev)
            {
                n.r.RemoveNeighbour(this);
            }
        }
        if (next != null)
        {
            foreach (Neighbour n in next)
            {
                n.r.RemoveNeighbour(this);
            }
        }
        foreach (Junction s in attachedSegments)
        {
            if (s != null) Destroy(s.gameObject);
        }
        foreach (Junction j in intersections)
        {
            if (j != null)
            {
                j.RemoveRoad(this);
            }
        }
    }

    public void RemoveNeighbour(Road r)
    {

        foreach (Neighbour n in next)
        {
            if (n.r.id == r.id)
            {
                next.Remove(n);
                return;
            }
        }
        foreach (Neighbour n in prev)
        {
            if (n.r.id == r.id)
            {
                prev.Remove(n);
                return;
            }
        }
    }

    public void ReplaceNeighbour(Road existing, Road replacement)
    {
        foreach (Neighbour n in next)
        {
            if (n.r.id == existing.id)
            {
                bool sameDirection = n.sameDirection;
                next.Remove(n);
                next.Add(new Neighbour(replacement, sameDirection));
                return;
            }
        }
        foreach (Neighbour n in prev)
        {
            if (n.r.id == existing.id)
            {
                bool sameDirection = n.sameDirection;
                prev.Remove(n);
                prev.Add(new Neighbour(replacement, sameDirection));
                return;
            }
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
        Quaternion dir = Quaternion.FromToRotation(Vector3.right, diff) * Quaternion.Euler(90f, 0, 0);
        transform.localRotation = dir;
        UpdateBounds();
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

        EventHandler handler = BoundsChanged;
        if (handler != null)
            handler(this, new EventArgs());

        return bounds;
    }
}

public enum RoadType { Street, Highway };