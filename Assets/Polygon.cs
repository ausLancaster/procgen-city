using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Polygon
{

    // Use this for initialization
    public static MeshBuilder Mesh(List<Vector3> corners)
    {
        MeshBuilder mesh = new MeshBuilder();

        // Vertices

        mesh.vertices = corners;

        // Triangles
        for (int i = 1; i < corners.Count - 1; i++)
        {
            mesh.triangles.Add(0);
            mesh.triangles.Add(i);
            mesh.triangles.Add(i+1);
        }

        // Normals
        for (int i = 0; i < corners.Count; i++)
        {
            mesh.normals.Add(Vector3.up);
        }

        return mesh;
    }
}
