
using UnityEngine;

public class SegmentFactory : MonoBehaviour {

    [SerializeField]
    Road roadPrefab;
    [SerializeField]
    Junction junctionPrefab;


    private void Start()
    {
    }

    public Road CreateRoad(Vector3 start, Vector3 end, float t, RoadType type)
    {
        Vector3 diff = end - start;
        float length = diff.magnitude;
        Quaternion dir = Quaternion.FromToRotation(Vector3.right, diff);

        return CreateRoad(start, end, dir, length, t, type);
    }

    public Road CreateRoad(Vector3 start, Quaternion dir, float length, float t, RoadType type)
    {


        Vector3 direction = dir * Vector3.right;
        Vector3 end = start + direction * length;

        return CreateRoad(start, end, dir, length, t, type);

    }

    Road CreateRoad(Vector3 start, Vector3 end, Quaternion dir, float length, float t, RoadType type)
    {
        Road road = Instantiate(roadPrefab);
        road.start = start;
        road.end = end;
        road.length = length;
        road.t = t;
        road.type = type;


        float width;
        if (type == RoadType.Highway) width = CityConfig.HIGHWAY_SEGMENT_WIDTH;
        else if (type == RoadType.Street) width = CityConfig.STREET_SEGMENT_WIDTH;
        else throw new RoadTypeNotSupportedException("Factory does not support this road type");
        road.transform.localScale = new Vector3(length * 0.1f, 1f, width * 0.1f);

        road.transform.localRotation = dir;

        road.transform.localPosition = (start + end) / 2f;

        road.enabled = false;

        road.junction = CreateJunction(end, dir);

        return road;
    }

    public Junction CreateJunction(Vector3 pos, Quaternion rotation)
    {
        Junction junction = Instantiate(junctionPrefab);
        junction.transform.position = pos;
        junction.transform.localScale = new Vector3(CityConfig.JUNCTION_SIZE * 0.1f, 1f, CityConfig.JUNCTION_SIZE * 0.1f);
        junction.transform.rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
        return junction;
    }
}

public class RoadTypeNotSupportedException : System.Exception
{
    public RoadTypeNotSupportedException(string message) : base(message) { }
}