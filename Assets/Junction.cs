
using System;
using UnityEngine;

public class Junction : MonoBehaviour, Segment {
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
