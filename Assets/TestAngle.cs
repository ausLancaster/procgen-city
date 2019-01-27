
using UnityEngine;

public class TestAngle : MonoBehaviour {

    [SerializeField]
    SegmentFactory segFactory;

    private void Start()
    {
        Road r0 = segFactory.CreateRoad(new Vector3(0, 0, -10), new Vector3(0, 0, 0), 0, RoadType.Street);
        Road r1 = segFactory.CreateRoad(new Vector3(-10, 0, 10), new Vector3(0, 0, 0), 0, RoadType.Street);
        Road r2 = segFactory.CreateRoad(new Vector3(10, 0, 10), new Vector3(0, 0, 0), 0, RoadType.Street);

        print(LotsGenerator.GetAngle(r0.transform.rotation, r1.transform.rotation, true, false));
        print(LotsGenerator.GetAngle(r0.transform.rotation, r2.transform.rotation, true, false));
    }
}
