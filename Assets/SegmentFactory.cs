
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
        Quaternion dir = Quaternion.FromToRotation(Vector3.right, diff) * Quaternion.Euler(90f, 0, 0);

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
        road.transform.localScale = new Vector3(length, width, 1f);

        road.transform.localRotation = dir;

        road.transform.localPosition = (start + end) / 2f;

        //road.junction = CreateJunction(end, dir);

        /*road.UpdateBounds();
        Rect bounds = road.Bounds;

        road.junction = CreateJunction(
            new Vector3(bounds.center.x, 0, bounds.center.y),
            new Vector3(bounds.width, bounds.height, 1f),
            Quaternion.identity
            );*/

        return road;
    }

    public Junction CreateJunction(Vector3 pos, Vector3 scale, Quaternion rotation)
    {
        Junction junction = Instantiate(junctionPrefab);
        junction.transform.position = pos + new Vector3(0, 0.1f, 0);
        junction.transform.localScale = scale;
        junction.transform.rotation = rotation * Quaternion.Euler(90f, 0, 0);
        if (!CityConfig.SHOW_JUNCTIONS) junction.gameObject.SetActive(false);
        return junction;
    }

    public Junction CreateJunction(Vector3 pos, Quaternion rotation)
    {
        return CreateJunction(pos, new Vector3(CityConfig.JUNCTION_SIZE, CityConfig.JUNCTION_SIZE, 1f), rotation);
    }

}

public class RoadTypeNotSupportedException : System.Exception
{
    public RoadTypeNotSupportedException(string message) : base(message) { }
}