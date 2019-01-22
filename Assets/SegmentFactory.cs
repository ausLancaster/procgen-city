
using UnityEngine;

public class SegmentFactory : MonoBehaviour {

    [SerializeField]
    Road prefab;

    private void Start()
    {
    }

    public Road Create(Vector3 start, Vector3 end, float t, RoadType type)
    {

        Road road = Instantiate(prefab);
        road.start = start;
        road.end = end;
        road.t = t;
        road.type = type;

        Vector3 diff = end - start;
        road.length = diff.magnitude;
        float width;
        if (type == RoadType.Highway) width = CityConfig.HIGHWAY_SEGMENT_WIDTH;
        else if (type == RoadType.Road) width = CityConfig.ROAD_SEGMENT_WIDTH;
        else throw new RoadTypeNotSupportedException("Factory does not support this road type");
        Vector3 scale = road.transform.localScale;
        scale.Scale(new Vector3(road.length, 1f, width));
        road.transform.localScale = scale;

        road.transform.localRotation = Quaternion.FromToRotation(Vector3.right, diff);
        road.transform.localPosition = (start + end) / 2f;

        return road;
    }

    public Road Create(Vector3 start, Quaternion dir, float length, float t, RoadType type)
    {
        Road road = Instantiate(prefab);
        road.start = start;
        road.length = length;
        road.t = t;
        road.type = type;

        float width;
        if (type == RoadType.Highway) width = CityConfig.HIGHWAY_SEGMENT_WIDTH;
        else if (type == RoadType.Road) width = CityConfig.ROAD_SEGMENT_WIDTH;
        else throw new RoadTypeNotSupportedException("Factory does not support this road type");
        Vector3 scale = road.transform.localScale;
        scale.Scale(new Vector3(length, 1f, width));
        road.transform.localScale = scale;

        road.transform.localRotation = dir;
        Vector3 direction = dir * Vector3.right;
        road.transform.localPosition = start + direction * length * 0.5f;
        road.end = start + direction * length;

        return road;
    }
}

public class RoadTypeNotSupportedException : System.Exception
{
    public RoadTypeNotSupportedException(string message) : base(message) { }
}