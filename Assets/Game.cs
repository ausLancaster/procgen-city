using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour {

    SegmentFactory segFactory;
    GlobalGoals globalGoals;
    LocalConstraints localConstraints;
    RoadMap map;
    public static int seed;

    private void Start()
    {
        segFactory = GetComponent<SegmentFactory>();
        globalGoals = GetComponent<GlobalGoals>();
        localConstraints = GetComponent<LocalConstraints>();
        map = GetComponent<RoadMap>();

        seed = 0;
        //seed = Random.Range(0, 65536);
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
        root1.prev.Add(root2);
        root2.prev.Add(root1);
        priorityQ.Enqueue(root1);
        priorityQ.Enqueue(root2);

        int roadCount = 0;

        while (priorityQ.Count() > 0)
        {
            Road nextRoad = priorityQ.Dequeue();
            if (roadCount < CityConfig.MAX_ROADS)
            {
                // check local constraints
                bool accepted = localConstraints.Check(nextRoad, map);
                if (accepted)
                {
                    // set up branch links
                    // activate road
                    map.AddRoad(nextRoad);
                    roadCount++;

                    // generate new possible branches according to global goals
                    List<Road> goals = globalGoals.Generate(nextRoad, map);
                    foreach (Road r in goals)
                    {
                        priorityQ.Enqueue(r);
                    }
                }
            } else
            {
                // delete leftover roads after maximum has been exceeded
                Destroy(nextRoad.gameObject);
            }
        }
        print("Created " + roadCount + " roads!");
    }
}
