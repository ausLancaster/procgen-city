
using System;
using System.Collections.Generic;
using UnityEngine;

public class Junction : MonoBehaviour, Segment {

    public List<Road> incoming {get; set;}
    public List<Road> outgoing { get; set; }
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
}
