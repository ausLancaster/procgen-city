
using System.Collections.Generic;
using UnityEngine;

public class Lot : MonoBehaviour {

    public List<Vector3> corners { get; set; }

    public void Initialize(List<Vector3> corners)
    {
        this.corners = corners;
        MeshBuilder mb = Polygon.Mesh(corners);
        GetComponent<MeshFilter>().mesh = mb.Generate();
    }
}
