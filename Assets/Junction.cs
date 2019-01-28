
using System;
using System.Collections.Generic;
using UnityEngine;

public class Junction : MonoBehaviour, Segment {

    public List<Road> incoming { get; private set;}
    public List<Road> outgoing { get; private set; }
    public bool added { get; set; }

    private void Awake()
    {
        incoming = new List<Road>();
        outgoing = new List<Road>();
        added = false;
    }

    public Rect Bounds
    {
        get
        {
            return new Rect(
                transform.localPosition.x - 0.0005f,
                transform.localPosition.z - 0.0005f,
                0.001f,
                0.001f
                );
        }
    }

    public event EventHandler BoundsChanged;

    public void SetColor(Color col)
    {
        GetComponent<MeshRenderer>().material.color = col;
    }

    public void AddIncoming(Road r)
    {
        incoming.Add(r);
        r.intersections.Add(this);
    }


    public void AddOutgoing(Road r)
    {
        outgoing.Add(r);
        r.intersections.Add(this);
    }

    public void RemoveRoad(Road r)
    {
        incoming.Remove(r);
        outgoing.Remove(r);
    }
}
