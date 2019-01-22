
using UnityEngine;

public class Heatmap : MonoBehaviour {

    public static float Value(Vector3 v)
    {
        v += new Vector3(-CityConfig.X_START, 0, CityConfig.Y_START);
        v *= 0.7f * 0.001f;
        float v1 = Mathf.PerlinNoise(v.x, v.z);
        float result = v1;
        return result;
    }
}
