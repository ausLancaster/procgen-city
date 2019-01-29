using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PolygonPrism
{
    // Use this for initialization
    public static MeshBuilder Mesh(List<Vector3> corners, float height)
    {
        MeshBuilder mesh = new MeshBuilder();
        int n = corners.Count;


        // Polygon on top

        foreach (Vector3 c in corners)
        {
            mesh.vertices.Add(new Vector3(c.x, c.y + height, c.z));
            mesh.normals.Add(Vector3.up);
        }

        for (int i = 1; i < n - 1; i++)
        {
            mesh.triangles.Add(0);
            mesh.triangles.Add(i);
            mesh.triangles.Add(i + 1);
        }

        // Rectangular sides

        for (int i=0; i < n; i++)
        {
            int next = i + 1;
            if (next == n) next = 0;

            int v1 = mesh.vertices.Count;
            mesh.vertices.Add(corners[i]);
            int v2 = mesh.vertices.Count;
            mesh.vertices.Add(corners[i] + height * Vector3.up);
            int v3 = mesh.vertices.Count;
            mesh.vertices.Add(corners[next]);
            int v4 = mesh.vertices.Count;
            mesh.vertices.Add(corners[next] + height * Vector3.up);

            mesh.triangles.Add(v1);
            mesh.triangles.Add(v4);
            mesh.triangles.Add(v2);
            mesh.triangles.Add(v1);
            mesh.triangles.Add(v3);
            mesh.triangles.Add(v4);

            // side normals
            Vector3 u = corners[i] + height * Vector3.up - corners[i];
            Vector3 v = corners[next] - corners[i];

            Vector3 normal = new Vector3(
                u.y * v.z - u.z * v.y,
                u.z * v.x - u.x * v.z,
                u.x * v.y - u.y * v.x
                );

            for (int j=0; j<4; j++)
            {
                mesh.normals.Add(normal);
            }
        }
        return mesh;
    }
}
