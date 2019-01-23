using CSharpQuadTree;
using System;
using UnityEngine;

public class Road : MonoBehaviour, IComparable<Road>, IQuadObject
{

    public Vector3 start { get; set; }
    public Vector3 end { get; set; }
    public float length;
    public event EventHandler BoundsChanged;
    public float t { get; set; }
    public Junction junction { get; set; }
    public RoadType type { get; set;  }
    Rect bounds;

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
        if (junction != null) Destroy(junction.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        print("collide");
    }

    private void OnTriggerEnter(Collider collision)
    {
        print("trigger");
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
        if (start.y < end.y)
        {
            minY = start.y;
            maxY = end.y;
        }
        else
        {
            minY = end.y;
            maxY = start.y;
        }
        bounds = new Rect(
            minX,
            minY,
            maxX - minX + 0.01f,
            maxY - minY + 0.01f
            );
        return bounds;
    }
}

public enum RoadType { Street, Highway };