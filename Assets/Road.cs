
using System;
using System.Collections.Generic;
using UnityEngine;

public class Road : MonoBehaviour, IComparable<Road> {

    public Vector3 start { get; set; }
    public Vector3 end { get; set; }
    public float length;
    public float t { get; set; }
    public RoadType type { get; set;  }


    public int CompareTo(Road other)
    {
        if (this.t < other.t) return -1;
        else if (this.t > other.t) return 1;
        else return 0;
    }
}

public enum RoadType { Road, Highway };