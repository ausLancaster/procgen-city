
using UnityEngine;

public class PopMapVisualisation : MonoBehaviour {

    [SerializeField]
    Transform prefab;
    const int RESOLUTION = 32;
    Color minColor = Color.black;
    Color maxColor = Color.green * 0.7f;


    public void Generate()
    {
        float step = CityConfig.WIDTH / RESOLUTION;
        for (float i = CityConfig.X_START;
            i<CityConfig.X_START + CityConfig.WIDTH;
            i+=step)
        {
            for (float j = CityConfig.Y_START;
                j < CityConfig.Y_START + CityConfig.HEIGHT;
                j += step)
            {
                Transform heatSquare = Instantiate(prefab);
                heatSquare.localScale = new Vector3(0.1f * step, 1f, 0.1f * step);
                heatSquare.localPosition = new Vector3(i + step * 0.5f, -0.1f, j + step * 0.5f);

                float pop = Heatmap.Value(heatSquare.localPosition);
                Color color = Color.Lerp(minColor, maxColor, pop);
                var meshRenderer = heatSquare.GetComponent<MeshRenderer>();
                meshRenderer.material.color = color;
            }
        }
    }
}
