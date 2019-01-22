using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour {

    SegmentFactory segFactory;
    GlobalGoals globalGoals;
    LocalConstraints localConstraints;
    public static int seed;

    private void Start()
    {
        segFactory = GetComponent<SegmentFactory>();
        globalGoals = GetComponent<GlobalGoals>();
        localConstraints = GetComponent<LocalConstraints>();

        seed = 0;// Random.Range(0, 65536);
        Generate(seed);
    }

    public void Generate(int seed)
    {
        Random.InitState(seed);
        PriorityQueue<Road> priorityQ = new PriorityQueue<Road>();

        // set up first two segments in centre of map
        Road root1 = segFactory.CreateRoad(
            new Vector3(0, 0, 0), 
            new Vector3(CityConfig.HIGHWAY_SEGMENT_LENGTH, 0, 0),
            0, 
            RoadType.Highway);
        Road root2 = segFactory.CreateRoad(
            new Vector3(0, 0, 0),
            new Vector3(-CityConfig.HIGHWAY_SEGMENT_LENGTH, 0, 0),
            0,
            RoadType.Highway);
        priorityQ.Enqueue(root1);
        priorityQ.Enqueue(root2);
        segFactory.CreateJunction(Vector3.zero, Quaternion.identity);

        int roadCount = 0;

        while (priorityQ.Count() > 0 && roadCount <= CityConfig.MAX_ROADS)
        {

            Road minRoad = priorityQ.Dequeue();

            // check local constraints

            // set up branch links
            // activate road
            minRoad.enabled = true;
            roadCount++;

            List<Road> goals = globalGoals.Generate(minRoad);
            foreach (Road r in goals)
            {
                r.t = minRoad.t + 1;
                priorityQ.Enqueue(r);
            }
        }
    }
}
