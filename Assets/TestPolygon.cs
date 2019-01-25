
using System.Collections.Generic;
using UnityEngine;

public class TestPolygon : MonoBehaviour {

    [SerializeField]
    SegmentFactory segFactory;

    private void Start()
    {
        List<Vector3> corners = new List<Vector3>();
        float radius = 100;
        float n = 5;
        for (int i=0; i<n; i++)
        {
            float angle = i*(2f * Mathf.PI)/n;
            print(angle);
            Vector3 c = radius * new Vector3(
                    Mathf.Cos(angle),
                    0,
                    Mathf.Sin(angle)
                );
            corners.Add(c);
            segFactory.CreateJunction(c, Quaternion.identity);
        }
        corners.Reverse();
        MeshBuilder mb = Polygon.Mesh(corners);
        GetComponent<MeshFilter>().mesh = mb.Generate();
    }

}
