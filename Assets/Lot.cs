
using System.Collections.Generic;
using UnityEngine;

public class Lot : MonoBehaviour {

    static int colorPropertyId = Shader.PropertyToID("_Color");
    static MaterialPropertyBlock sharedPropertyBlock;

    public List<Vector3> corners { get; set; }

    MeshRenderer meshRenderer;

    public void Initialize(List<Vector3> corners, float height, Material material)
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = material;

        Color color = Random.value * new Color(1f, 1f, 1f);

        if (sharedPropertyBlock == null)
        {
            sharedPropertyBlock = new MaterialPropertyBlock();
        }
        sharedPropertyBlock.SetColor(colorPropertyId, color);
        meshRenderer.SetPropertyBlock(sharedPropertyBlock);

        this.corners = corners;

        // find centre by averaging all corners
        Vector3 centre = Vector3.zero;
        foreach (Vector3 c in corners)
        {
            centre += c;
        }
        centre *= (1f / corners.Count);

        // shift corners toward centre to make way for road
        for (int i=0; i<corners.Count; i++)
        {
            Vector3 direction = centre - corners[i];
            direction = direction.normalized;
            corners[i] += CityConfig.CORNER_TO_BUILDING_GAP * direction;
        }

        MeshBuilder mb = PolygonPrism.Mesh(corners, height);
        GetComponent<MeshFilter>().mesh = mb.Generate();
    }
}
