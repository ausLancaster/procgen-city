
using UnityEngine;

public class Heatmap {

    static int seed = 0;

    public static void Seed(int seed)
    {
        Heatmap.seed = seed;
    }

    public static float Value(Vector3 v)
    {
        v += new Vector3(-CityConfig.X_START, 0, CityConfig.Y_START);
        v += new Vector3(seed * CityConfig.WIDTH, 0, 0);
        v *= 0.7f * 0.001f;
        float v1 = Mathf.PerlinNoise(v.x, v.z);
        float result = v1;
        return result;
    }
}
