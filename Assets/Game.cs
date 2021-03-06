﻿using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour {

    SegmentFactory segFactory;
    GlobalGoals globalGoals;
    LocalConstraints localConstraints;
    RoadMap map;
    [SerializeField]
    PopMapVisualisation heatmapVis;
    LotsGenerator lotsGenerator;
    public static int seed;

    private void Start()
    {
        segFactory = GetComponent<SegmentFactory>();
        globalGoals = GetComponent<GlobalGoals>();
        localConstraints = GetComponent<LocalConstraints>();
        map = GetComponent<RoadMap>();
        lotsGenerator = GetComponent<LotsGenerator>();

        seed = 8;
        //seed = Random.Range(0, 65536);
        Generate(seed);
    }

    public void Generate(int seed)
    {
        Random.InitState(seed);
        Heatmap.Seed(seed);
        if (CityConfig.SHOW_HEATMAP) heatmapVis.Generate();
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
        root1.Parent = root2;
        root2.Parent = root1;
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
                    foreach (Junction j in nextRoad.attachedSegments)
                    {
                        map.AddJunction(j);
                    }
                    roadCount++;

                    // generate new possible branches according to global goals
                    List<Road> goals = globalGoals.Generate(nextRoad, map);
                    foreach (Road r in goals)
                    {
                        priorityQ.Enqueue(r);
                    }
                } else
                {
                    DestroyImmediate(nextRoad.gameObject);
                }
            } else
            {
                // delete leftover roads after maximum has been exceeded
                DestroyImmediate(nextRoad.gameObject);
            }
        }
        print("Created " + roadCount + " roads!");

        lotsGenerator.Generate(map.allRoads);
    }
}
